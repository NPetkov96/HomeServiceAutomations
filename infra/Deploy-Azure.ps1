param(
    [string]$ResourceGroup = 'rg-homeservice-prod',
    [string]$Location = 'northeurope',
    [string]$SqlLocation = 'swedencentral',
    [string]$Prefix = 'homeservice',
    [string]$SqlAdminLogin = 'homeserviceadmin',
    [switch]$SkipDatabaseMigration
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$apiKeyPath = Join-Path $repositoryRoot 'work\secrets\home-api-key.txt'

az account show --only-show-errors | Out-Null

if ([string]::IsNullOrWhiteSpace($env:HOMESERVICE_SQL_ADMIN_PASSWORD)) {
    $randomBytes = New-Object byte[] 36
    [Security.Cryptography.RandomNumberGenerator]::Fill($randomBytes)
    $env:HOMESERVICE_SQL_ADMIN_PASSWORD = [Convert]::ToBase64String($randomBytes) + '!aA9'
    Write-Output 'Generated a SQL administrator password for this deployment session.'
}

if ([string]::IsNullOrWhiteSpace($env:HOMESERVICE_API_KEY)) {
    & (Join-Path $PSScriptRoot 'Generate-ApiKey.ps1') -OutputPath $apiKeyPath | Out-Null
    $env:HOMESERVICE_API_KEY = (Get-Content -LiteralPath $apiKeyPath -Raw).Trim()
}

$clientIp = (Invoke-RestMethod -Uri 'https://api.ipify.org').Trim()

az group create `
    --name $ResourceGroup `
    --location $Location `
    --only-show-errors `
    --output none

$foundationJson = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file (Join-Path $PSScriptRoot 'foundation.bicep') `
    --parameters `
        prefix=$Prefix `
        location=$Location `
        sqlLocation=$SqlLocation `
        sqlAdminLogin=$SqlAdminLogin `
        sqlAdminPassword=$env:HOMESERVICE_SQL_ADMIN_PASSWORD `
        deploymentClientIp=$clientIp `
    --query properties.outputs `
    --only-show-errors `
    --output json

$foundation = $foundationJson | ConvertFrom-Json
$registryName = $foundation.registryName.value
$imageTag = (git -C $repositoryRoot rev-parse --short HEAD).Trim()

$registryLoginServer = $foundation.registryLoginServer.value
az acr login --name $registryName --only-show-errors | Out-Null

Write-Output "Building and pushing home-api:$imageTag..."
docker build `
    --file (Join-Path $repositoryRoot 'Dockerfile.api') `
    --tag "$registryLoginServer/home-api:$imageTag" `
    $repositoryRoot
if ($LASTEXITCODE -ne 0) { throw 'HomeApi container build failed.' }
docker push "$registryLoginServer/home-api:$imageTag"
if ($LASTEXITCODE -ne 0) { throw 'HomeApi container push failed.' }

Write-Output "Building and pushing home-jobs:$imageTag..."
docker build `
    --file (Join-Path $repositoryRoot 'Dockerfile.jobs') `
    --tag "$registryLoginServer/home-jobs:$imageTag" `
    $repositoryRoot
if ($LASTEXITCODE -ne 0) { throw 'HomeJobs container build failed.' }
docker push "$registryLoginServer/home-jobs:$imageTag"
if ($LASTEXITCODE -ne 0) { throw 'HomeJobs container push failed.' }

if (-not $SkipDatabaseMigration) {
    $databaseExists = az sql db show `
        --resource-group $ResourceGroup `
        --server $foundation.sqlServerName.value `
        --name MyDbContext `
        --query name `
        --output tsv `
        --only-show-errors 2>$null

    if ([string]::IsNullOrWhiteSpace($databaseExists)) {
        & (Join-Path $PSScriptRoot 'Migrate-Database.ps1') `
            -TargetServerFqdn $foundation.sqlServerFqdn.value `
            -TargetAdminLogin $SqlAdminLogin
    }
    else {
        Write-Output 'Azure database already exists; skipping the one-time migration.'
    }
}

$databaseConnectionString = "Server=tcp:$($foundation.sqlServerFqdn.value),1433;Initial Catalog=MyDbContext;Persist Security Info=False;User ID=$SqlAdminLogin;Password=$($env:HOMESERVICE_SQL_ADMIN_PASSWORD);MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$appsJson = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file (Join-Path $PSScriptRoot 'apps.bicep') `
    --parameters `
        location=$Location `
        containerAppsEnvironmentName=$foundation.containerAppsEnvironmentName.value `
        registryName=$registryName `
        identityName=$foundation.identityName.value `
        databaseConnectionString=$databaseConnectionString `
        apiKey=$env:HOMESERVICE_API_KEY `
        apiImageTag=$imageTag `
        jobsImageTag=$imageTag `
    --query properties.outputs `
    --only-show-errors `
    --output json

$apps = $appsJson | ConvertFrom-Json
Write-Output "Azure deployment completed: $($apps.apiUrl.value)"
