# Azure deployment

The cloud deployment keeps the existing local Windows hosting untouched while
running the API and scheduled workloads in Azure Container Apps.

## Resources

- `home-api`: public Azure Container App, scaled between zero and one replica.
- `home-blood-tests-daily`: runs every day at 01:00 UTC.
- `home-imot-bg-daily`: runs every day at 01:30 UTC.
- Azure SQL Database `MyDbContext` on the Basic tier.
- Azure Container Registry Basic.
- Log Analytics workspace with 30-day retention.

The Container Apps resources run in North Europe. Azure SQL runs in Sweden
Central because this subscription currently rejects new SQL servers in North
Europe; the regions are separate parameters in the deployment script.

Azure SQL provides managed backups, so the Windows file-based backup task is
intentionally not deployed as a Container Apps Job.

The public API requires an `X-Api-Key` header. The deployment script generates
a 256-bit key once in the ignored `work/secrets/home-api-key.txt` file and stores
the same value as an Azure Container App secret. Local hosting does not require
the header.

## Initial deployment

1. Sign in and select the intended subscription:

   ```powershell
   az login
   az account set --subscription <subscription-id>
   ```

2. Run the deployment from the repository root:

   ```powershell
   .\infra\Deploy-Azure.ps1
   ```

The script creates the infrastructure, builds both images with the local Docker engine, exports and
imports the local database once, deploys the API and the two jobs, and prints
the final HTTPS API URL. It does not stop or reconfigure the local host.

## CI/CD

Run `Configure-GitHubOidc.ps1` after the resource group exists. Add its three
output values as GitHub Actions secrets named `AZURE_CLIENT_ID`,
`AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID`. Add the ACR name and login server
as repository variables `AZURE_ACR_NAME` and `AZURE_ACR_LOGIN_SERVER`.

Subsequent pushes to `Azure-Migration` build, test, publish immutable images,
and update the API and both jobs without repeating the database migration.
