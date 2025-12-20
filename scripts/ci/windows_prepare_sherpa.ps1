# scripts/ci/windows_prepare_sherpa.ps1
# Downloads sherpa-onnx-win-x64 runtimes/dlls and places them where the app expects them.

$ErrorActionPreference = "Stop"

$SHERPA_VERSION = "1.9.13" # Example version
$BASE_URL = "https://github.com/k2-fsa/sherpa-onnx/releases/download/v$SHERPA_VERSION"
$FILENAME = "sherpa-onnx-v$SHERPA_VERSION-win-x64.zip"
$DOWNLOAD_URL = "$BASE_URL/$FILENAME"

$OUTPUT_DIR = "runtimes/win-x64/native"
$TEMP_ZIP = "$env:TEMP/sherpa.zip"

Write-Host "Downloading Sherpa-ONNX from $DOWNLOAD_URL"
Invoke-WebRequest -Uri $DOWNLOAD_URL -OutFile $TEMP_ZIP

Write-Host "Extracting to $OUTPUT_DIR"
if (!(Test-Path $OUTPUT_DIR)) {
    New-Item -ItemType Directory -Force -Path $OUTPUT_DIR | Out-Null
}

Expand-Archive -Path $TEMP_ZIP -DestinationPath "$env:TEMP/sherpa_extracted" -Force

# Move the DLLs to the target runtime folder
# Note: The zip structure usually contains a subfolder. Adjust logic based on actual zip content.
# Assuming content is in extracted root or subfolder.
$SourcePath = "$env:TEMP/sherpa_extracted/sherpa-onnx-v$SHERPA_VERSION-win-x64/bin/*.dll"

Copy-Item -Path $SourcePath -Destination $OUTPUT_DIR -Force
Write-Host "Sherpa DLLs placed in $OUTPUT_DIR"

# Clean up
Remove-Item $TEMP_ZIP -Force
Remove-Item "$env:TEMP/sherpa_extracted" -Recurse -Force
