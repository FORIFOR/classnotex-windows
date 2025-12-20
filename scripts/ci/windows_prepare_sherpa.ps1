# scripts/ci/windows_prepare_sherpa.ps1
# Downloads sherpa-onnx-win-x64 runtimes/dlls and places them where the app expects them.

$ErrorActionPreference = "Stop"

$SHERPA_VERSION = "1.12.20"
$BASE_URL = "https://github.com/k2-fsa/sherpa-onnx/releases/download/v$SHERPA_VERSION"
$FILENAME = "sherpa-onnx-v$SHERPA_VERSION-win-x64-shared.tar.bz2"
$DOWNLOAD_URL = "$BASE_URL/$FILENAME"

$OUTPUT_DIR = "runtimes/win-x64/native"
$TEMP_FILE = "$env:TEMP/sherpa.tar.bz2"
$TEMP_TAR = "$env:TEMP/sherpa.tar"

Write-Host "Downloading Sherpa-ONNX from $DOWNLOAD_URL"
Invoke-WebRequest -Uri $DOWNLOAD_URL -OutFile $TEMP_FILE

# Extract .bz2 to .tar (PowerShell doesn't natively support bz2 but 7z is available on GitHub runners)
Write-Host "Extracting bz2..."
7z x $TEMP_FILE -o"$env:TEMP" -y

Write-Host "Extracting tar..."
7z x $TEMP_TAR -o"$env:TEMP/sherpa_extracted" -y

if (!(Test-Path $OUTPUT_DIR)) {
    New-Item -ItemType Directory -Force -Path $OUTPUT_DIR | Out-Null
}

# Move the DLLs to the target runtime folder
# The tar extracts to a folder like sherpa-onnx-v1.12.20-win-x64-shared
$SourceDir = "$env:TEMP/sherpa_extracted/sherpa-onnx-v$SHERPA_VERSION-win-x64-shared"
if (Test-Path "$SourceDir/lib") {
    Copy-Item -Path "$SourceDir/lib/*.dll" -Destination $OUTPUT_DIR -Force -ErrorAction SilentlyContinue
}
if (Test-Path "$SourceDir/bin") {
    Copy-Item -Path "$SourceDir/bin/*.dll" -Destination $OUTPUT_DIR -Force -ErrorAction SilentlyContinue
}
# Some builds put dlls directly in the root or lib folder
Copy-Item -Path "$SourceDir/*.dll" -Destination $OUTPUT_DIR -Force -ErrorAction SilentlyContinue

Write-Host "Sherpa DLLs placed in $OUTPUT_DIR"
Get-ChildItem $OUTPUT_DIR

# Clean up
Remove-Item $TEMP_FILE -Force -ErrorAction SilentlyContinue
Remove-Item $TEMP_TAR -Force -ErrorAction SilentlyContinue
Remove-Item "$env:TEMP/sherpa_extracted" -Recurse -Force -ErrorAction SilentlyContinue
