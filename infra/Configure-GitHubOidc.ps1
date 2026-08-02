param(
    [string]$GitHubOwner = 'NPetkov96',
    [string]$GitHubRepository = 'HomeServiceAutomations',
    [string]$Branch = 'Azure-Migration',
    [string]$ResourceGroup = 'rg-homeservice-prod',
    [string]$RegistryName = 'homeservicex3tpibnr',
    [string]$ApplicationName = 'homeservice-github-actions'
)

$ErrorActionPreference = 'Stop'
$account = az account show --output json | ConvertFrom-Json

$application = az ad app list `
    --display-name $ApplicationName `
    --query '[0]' `
    --output json | ConvertFrom-Json

if ($null -eq $application) {
    $application = az ad app create `
        --display-name $ApplicationName `
        --output json | ConvertFrom-Json
}

$servicePrincipal = az ad sp show `
    --id $application.appId `
    --output json `
    2>$null | ConvertFrom-Json

if ($null -eq $servicePrincipal) {
    $servicePrincipal = az ad sp create `
        --id $application.appId `
        --output json | ConvertFrom-Json
}

$resourceGroupId = az group show `
    --name $ResourceGroup `
    --query id `
    --output tsv

az role assignment create `
    --assignee-object-id $servicePrincipal.id `
    --assignee-principal-type ServicePrincipal `
    --role Contributor `
    --scope $resourceGroupId `
    --only-show-errors `
    --output none

$registryId = az acr show `
    --name $RegistryName `
    --resource-group $ResourceGroup `
    --query id `
    --output tsv

az role assignment create `
    --assignee-object-id $servicePrincipal.id `
    --assignee-principal-type ServicePrincipal `
    --role AcrPush `
    --scope $registryId `
    --only-show-errors `
    --output none

$credentialName = "github-$GitHubOwner-$GitHubRepository-$Branch".ToLowerInvariant()
$existingCredential = az ad app federated-credential list `
    --id $application.appId `
    --query "[?name=='$credentialName'].name | [0]" `
    --output tsv

if ([string]::IsNullOrWhiteSpace($existingCredential)) {
    $credential = @{
        name = $credentialName
        issuer = 'https://token.actions.githubusercontent.com'
        subject = "repo:$GitHubOwner/$GitHubRepository`:ref:refs/heads/$Branch"
        description = 'GitHub Actions deployment identity for HomeService Azure migration'
        audiences = @('api://AzureADTokenExchange')
    }

    $temporaryFile = Join-Path ([IO.Path]::GetTempPath()) "$credentialName.json"
    try {
        $credential | ConvertTo-Json | Set-Content -LiteralPath $temporaryFile -Encoding utf8
        az ad app federated-credential create `
            --id $application.appId `
            --parameters $temporaryFile `
            --only-show-errors `
            --output none
    }
    finally {
        Remove-Item -LiteralPath $temporaryFile -Force -ErrorAction SilentlyContinue
    }
}

Write-Output "AZURE_CLIENT_ID=$($application.appId)"
Write-Output "AZURE_TENANT_ID=$($account.tenantId)"
Write-Output "AZURE_SUBSCRIPTION_ID=$($account.id)"
