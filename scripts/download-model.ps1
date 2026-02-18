# sherpa-onnx 日本語モデルダウンロードスクリプト
# 実行: powershell -ExecutionPolicy Bypass -File download-model.ps1

$modelUrl = "https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/sherpa-onnx-zipformer-ja-reazonspeech-2024-08-01.tar.bz2"
$modelDir = "..\DeepNoteWindows\models"
$tempFile = "sherpa-onnx-zipformer-ja-reazonspeech-2024-08-01.tar.bz2"

Write-Host "sherpa-onnx 日本語モデルをダウンロードします..." -ForegroundColor Cyan

# ディレクトリ作成
if (-not (Test-Path $modelDir)) {
    New-Item -ItemType Directory -Path $modelDir -Force | Out-Null
}

Set-Location $modelDir

# ダウンロード
Write-Host "ダウンロード中: $modelUrl" -ForegroundColor Yellow
Invoke-WebRequest -Uri $modelUrl -OutFile $tempFile

# 解凍 (7-Zip が必要)
Write-Host "解凍中..." -ForegroundColor Yellow

# tar.bz2 の解凍には 7-Zip または tar コマンドを使用
if (Get-Command "tar" -ErrorAction SilentlyContinue) {
    tar -xjf $tempFile
} elseif (Test-Path "C:\Program Files\7-Zip\7z.exe") {
    & "C:\Program Files\7-Zip\7z.exe" x $tempFile -y
    & "C:\Program Files\7-Zip\7z.exe" x ($tempFile -replace ".bz2$", "") -y
    Remove-Item ($tempFile -replace ".bz2$", "") -Force
} else {
    Write-Host "エラー: tar または 7-Zip が見つかりません" -ForegroundColor Red
    Write-Host "7-Zip をインストールするか、手動で解凍してください" -ForegroundColor Yellow
    exit 1
}

# 一時ファイル削除
Remove-Item $tempFile -Force -ErrorAction SilentlyContinue

Write-Host "完了しました！" -ForegroundColor Green
Write-Host "モデルパス: $((Resolve-Path .).Path)\sherpa-onnx-zipformer-ja-reazonspeech-2024-08-01" -ForegroundColor Cyan
