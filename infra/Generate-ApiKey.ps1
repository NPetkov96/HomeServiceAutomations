param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\work\secrets\home-api-key.txt')
)

$ErrorActionPreference = 'Stop'

if (Test-Path -LiteralPath $OutputPath) {
    Write-Output (Resolve-Path -LiteralPath $OutputPath).Path
    exit 0
}

$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$apiKey = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')

$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $directory -Force | Out-Null
[IO.File]::WriteAllText($OutputPath, $apiKey, [Text.UTF8Encoding]::new($false))

Write-Output (Resolve-Path -LiteralPath $OutputPath).Path
