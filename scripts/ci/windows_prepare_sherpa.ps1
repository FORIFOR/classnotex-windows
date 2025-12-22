# scripts/ci/windows_prepare_sherpa.ps1
# Downloads sherpa-onnx-win-x64 runtimes/dlls and places them where the app expects them.

$ErrorActionPreference = "Stop"

$SHERPA_VERSION = "1.12.20"
$BASE_URL = "https://github.com/k2-fsa/sherpa-onnx/releases/download/v$SHERPA_VERSION"
$FILENAME = "sherpa-onnx-v$SHERPA_VERSION-win-x64-shared.tar.bz2"
$DOWNLOAD_URL = "$BASE_URL/$FILENAME"

$OUTPUT_DIR = "src/ClassnoteX.WinUI/runtimes/win-x64/native"
$TEMP_FILE = "$env:TEMP/sherpa.tar.bz2"
$TEMP_TAR = "$env:TEMP/sherpa.tar"
$EXTRACT_DIR = "$env:TEMP/sherpa_extracted"

Write-Host "--- Sherpa-ONNX Preparation Start ---"
Write-Host "Downloading from: $DOWNLOAD_URL"

try {
    Invoke-WebRequest -Uri $DOWNLOAD_URL -OutFile $TEMP_FILE -ErrorAction Stop
} catch {
    Write-Error "Failed to download Sherpa-ONNX. Check the URL and version. Error: $_"
    exit 1
}

if (!(Test-Path $TEMP_FILE)) {
    Write-Error "Download failed: $TEMP_FILE does not exist."
    exit 1
}

$fileSize = (Get-Item $TEMP_FILE).Length
Write-Host "Downloaded file size: $fileSize bytes"

# Extract .bz2 to .tar
Write-Host "Extracting bz2..."
7z x $TEMP_FILE -o"$env:TEMP" -y | Out-Null

if (!(Test-Path $TEMP_TAR)) {
    Write-Error "bz2 extraction failed: $TEMP_TAR does not exist."
    exit 1
}

# Extract tar
Write-Host "Extracting tar..."
if (!(Test-Path $EXTRACT_DIR)) { New-Item -ItemType Directory -Path $EXTRACT_DIR }
7z x $TEMP_TAR -o"$EXTRACT_DIR" -y | Out-Null

if (!(Test-Path $OUTPUT_DIR)) {
    New-Item -ItemType Directory -Force -Path $OUTPUT_DIR | Out-Null
}

# Search for DLLs recursively and copy them
Write-Host "Searching for DLLs in extracted folder: $EXTRACT_DIR"
$dlls = Get-ChildItem -Path $EXTRACT_DIR -Filter "*.dll" -Recurse
if (@($dlls).Count -eq 0) {
    Write-Warning "No DLLs found in the extracted archive: $EXTRACT_DIR"
} else {
    foreach ($dll in $dlls) {
        Write-Host "Copying $($dll.Name) (from $($dll.FullName)) to $OUTPUT_DIR"
        Copy-Item -Path $dll.FullName -Destination $OUTPUT_DIR -Force
    }
}

Write-Host "Sherpa DLLs currently in $OUTPUT_DIR:"
Get-ChildItem $OUTPUT_DIR | Select-Object Name

# Clean up
Write-Host "Cleaning up... "
Remove-Item $TEMP_FILE -Force -ErrorAction SilentlyContinue
Remove-Item $TEMP_TAR -Force -ErrorAction SilentlyContinue
Remove-Item $EXTRACT_DIR -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "--- Sherpa-ONNX Preparation Complete ---"
