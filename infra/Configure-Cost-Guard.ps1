param(
    [string]$ResourceGroup = 'rg-homeservice-prod',
    [string]$WorkspaceName = 'homeservice-logs',
    [string]$ContactEmail = 'nikolaypetkow96@icloud.com',
    [int]$MonthlyBudget = 15
)

$ErrorActionPreference = 'Stop'
$account = az account show --output json | ConvertFrom-Json
$startDate = Get-Date -Day 1 -Hour 0 -Minute 0 -Second 0 -Format 'yyyy-MM-ddTHH:mm:ssZ'
$endDate = (Get-Date $startDate).AddYears(10).ToString('yyyy-MM-ddTHH:mm:ssZ')

az monitor log-analytics workspace update `
    --resource-group $ResourceGroup `
    --workspace-name $WorkspaceName `
    --quota 0.023 `
    --only-show-errors `
    --output none
if ($LASTEXITCODE -ne 0) { throw 'Could not set the Log Analytics daily cap.' }

az deployment sub create `
    --name homeservice-cost-guard `
    --location northeurope `
    --template-file (Join-Path $PSScriptRoot 'budget.bicep') `
    --parameters `
        contactEmail=$ContactEmail `
        amount=$MonthlyBudget `
        startDate=$startDate `
        endDate=$endDate `
    --only-show-errors `
    --output none
if ($LASTEXITCODE -ne 0) { throw 'Could not create the Azure monthly budget.' }

Write-Output "Configured a $MonthlyBudget billing-currency monthly budget and a 0.023 GB/day Log Analytics cap for subscription $($account.id)."
