param(
    [Parameter(Mandatory = $true)]
    [string]$TargetServerFqdn,

    [Parameter(Mandatory = $true)]
    [string]$TargetAdminLogin,

    [string]$TargetDatabaseName = 'MyDbContext',
    [string]$SourceConnectionString = 'Server=PETKOV;Database=MyDbContext;Trusted_Connection=True;TrustServerCertificate=True',
    [string]$WorkingDirectory = (Join-Path $PSScriptRoot '..\work\database-migration')
)

$ErrorActionPreference = 'Stop'

$targetPassword = $env:HOMESERVICE_SQL_ADMIN_PASSWORD
if ([string]::IsNullOrWhiteSpace($targetPassword)) {
    throw 'Set HOMESERVICE_SQL_ADMIN_PASSWORD before running the migration.'
}

if (-not (Get-Command SqlPackage -ErrorAction SilentlyContinue)) {
    throw 'SqlPackage is required but was not found in PATH.'
}

New-Item -ItemType Directory -Path $WorkingDirectory -Force | Out-Null
$bacpacPath = Join-Path $WorkingDirectory "$TargetDatabaseName.bacpac"

Write-Output 'Exporting the local database to a temporary BACPAC...'
SqlPackage `
    /Action:Export `
    "/SourceConnectionString:$SourceConnectionString" `
    "/TargetFile:$bacpacPath" `
    /p:VerifyExtraction=True
if ($LASTEXITCODE -ne 0) {
    throw 'Local database export failed.'
}

$targetConnectionString = "Server=tcp:$TargetServerFqdn,1433;Initial Catalog=$TargetDatabaseName;User ID=$TargetAdminLogin;Password=$targetPassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"

Write-Output 'Importing the BACPAC into Azure SQL Basic...'
SqlPackage `
    /Action:Import `
    "/SourceFile:$bacpacPath" `
    "/TargetConnectionString:$targetConnectionString" `
    /p:DatabaseEdition=Basic `
    /p:DatabaseServiceObjective=Basic
if ($LASTEXITCODE -ne 0) {
    throw 'Azure SQL import failed.'
}

Write-Output 'Verifying migrated aggregate row counts...'
$verifyQuery = @'
SET NOCOUNT ON;
SELECT 'MedSestriPatients' AS TableName, COUNT(*) AS [RowCount] FROM MedSestriPatients
UNION ALL SELECT 'MedSestriBloodTests', COUNT(*) FROM MedSestriBloodTests
UNION ALL SELECT 'MedSestriCatheters', COUNT(*) FROM MedSestriCatheters
UNION ALL SELECT 'ImotBgApartments', COUNT(*) FROM ImotBgApartments;
'@

sqlcmd `
    -S "tcp:$TargetServerFqdn,1433" `
    -d $TargetDatabaseName `
    -U $TargetAdminLogin `
    -P $targetPassword `
    -C `
    -Q $verifyQuery
if ($LASTEXITCODE -ne 0) {
    throw 'Azure SQL verification query failed.'
}

Write-Output "Database migration completed. Temporary BACPAC: $bacpacPath"
