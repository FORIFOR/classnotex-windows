using SherpaOnnx;
using System.Text.Json;

namespace DeepNoteWindows.Services;

public class SherpaOnnxService : ISherpaOnnxService, IDisposable
{
    private OfflineRecognizer? _recognizer;
    private readonly object _lock = new();

    public bool IsModelLoaded => _recognizer != null;

    public Task<bool> LoadModelAsync(string modelPath)
    {
        return Task.Run(() =>
        {
            lock (_lock)
            {
                try
                {
                    UnloadModel();

                    var basePath = Path.Combine(AppContext.BaseDirectory, modelPath);

                    // sherpa-onnx-zipformer-ja-reazonspeech-2024-08-01 のモデル構成
                    var config = new OfflineRecognizerConfig();
                    config.ModelConfig.Transducer.Encoder = Path.Combine(basePath, "encoder-epoch-99-avg-1.onnx");
                    config.ModelConfig.Transducer.Decoder = Path.Combine(basePath, "decoder-epoch-99-avg-1.onnx");
                    config.ModelConfig.Transducer.Joiner = Path.Combine(basePath, "joiner-epoch-99-avg-1.onnx");
                    config.ModelConfig.Tokens = Path.Combine(basePath, "tokens.txt");
                    config.ModelConfig.NumThreads = 4;
                    config.ModelConfig.Provider = "cpu";
                    config.ModelConfig.Debug = 0;
                    config.DecodingMethod = "greedy_search";

                    // 日本語用のFeature抽出設定
                    config.FeatConfig.SampleRate = 16000;
                    config.FeatConfig.FeatureDim = 80;

                    _recognizer = new OfflineRecognizer(config);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"モデル読み込みエラー: {ex.Message}");
                    return false;
                }
            }
        });
    }

    public Task<string> TranscribeAsync(float[] samples, int sampleRate)
    {
        return Task.Run(() =>
        {
            lock (_lock)
            {
                if (_recognizer == null)
                    throw new InvalidOperationException("モデルが読み込まれていません");

                if (samples.Length == 0)
                    return string.Empty;

                try
                {
                    // ストリームを作成
                    var stream = _recognizer.CreateStream();

                    // サンプルを受け入れる
                    stream.AcceptWaveform(sampleRate, samples);

                    // 入力の終了を示す
                    stream.InputFinished();

                    // デコード
                    _recognizer.Decode(stream);

                    // 結果を取得
                    var result = stream.Result;
                    return result.Text.Trim();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"文字起こしエラー: {ex.Message}");
                    return string.Empty;
                }
            }
        });
    }

    public void UnloadModel()
    {
        lock (_lock)
        {
            _recognizer?.Dispose();
            _recognizer = null;
        }
    }

    public void Dispose()
    {
        UnloadModel();
    }
}
