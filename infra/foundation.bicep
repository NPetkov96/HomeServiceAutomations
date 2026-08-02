targetScope = 'resourceGroup'

@description('Short resource name prefix.')
param prefix string = 'homeservice'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Azure region for SQL. Kept separate because new-subscription capacity can differ by service.')
param sqlLocation string = location

@description('SQL administrator login name.')
param sqlAdminLogin string = 'homeserviceadmin'

@secure()
@description('SQL administrator password. Used only by the SQL server resource.')
param sqlAdminPassword string

@description('Public IPv4 address allowed temporarily for database migration.')
param deploymentClientIp string = ''

var suffix = toLower(take(uniqueString(subscription().id, resourceGroup().id), 8))
var compactPrefix = replace(toLower(prefix), '-', '')
var registryName = take('${compactPrefix}${suffix}', 50)
var sqlRegionSuffix = toLower(take(uniqueString(sqlLocation), 4))
var sqlServerName = '${toLower(prefix)}-sql-${suffix}-${sqlRegionSuffix}'
var environmentName = '${toLower(prefix)}-env'
var workspaceName = '${toLower(prefix)}-logs'
var identityName = '${toLower(prefix)}-identity'

resource workspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    workspaceCapping: {
      dailyQuotaGb: json('0.023')
    }
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: workspace.properties.customerId
        sharedKey: workspace.listKeys().primarySharedKey
      }
    }
  }
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

resource workloadIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, workloadIdentity.id, 'AcrPull')
  scope: registry
  properties: {
    principalId: workloadIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d'
    )
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: sqlServerName
  location: sqlLocation
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource deploymentClientFirewall 'Microsoft.Sql/servers/firewallRules@2023-08-01' = if (!empty(deploymentClientIp)) {
  parent: sqlServer
  name: 'DeploymentClient'
  properties: {
    startIpAddress: deploymentClientIp
    endIpAddress: deploymentClientIp
  }
}

output containerAppsEnvironmentName string = environment.name
output registryName string = registry.name
output registryLoginServer string = registry.properties.loginServer
output identityName string = workloadIdentity.name
output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlAdminLogin string = sqlAdminLogin
