targetScope = 'resourceGroup'

param location string = resourceGroup().location
param containerAppsEnvironmentName string
param registryName string
param identityName string
@secure()
param databaseConnectionString string

@secure()
param apiKey string = ''

param apiImageTag string = 'latest'
param jobsImageTag string = 'latest'
param deployApi bool = true

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: registryName
}

resource workloadIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

var apiImage = '${registry.properties.loginServer}/home-api:${apiImageTag}'
var jobsImage = '${registry.properties.loginServer}/home-jobs:${jobsImageTag}'
resource api 'Microsoft.App/containerApps@2024-03-01' = if (deployApi) {
  name: 'home-api'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${workloadIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 3
      ingress: {
        external: true
        allowInsecure: false
        targetPort: 8080
        transport: 'auto'
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: registry.properties.loginServer
          identity: workloadIdentity.id
        }
      ]
      secrets: [
        {
          name: 'db-connection'
          value: databaseConnectionString
        }
        {
          name: 'api-key'
          value: apiKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'home-api'
          image: apiImage
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'db-connection'
            }
            {
              name: 'ApiSecurity__RequireApiKey'
              value: 'true'
            }
            {
              name: 'ApiSecurity__ApiKey'
              secretRef: 'api-key'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 10
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '20'
              }
            }
          }
        ]
      }
    }
  }
}

var commonJobSecrets = [
  {
    name: 'db-connection'
    value: databaseConnectionString
  }
]

var commonJobRegistry = [
  {
    server: registry.properties.loginServer
    identity: workloadIdentity.id
  }
]

resource bloodTestsJob 'Microsoft.App/jobs@2024-03-01' = {
  name: 'home-blood-tests-daily'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${workloadIdentity.id}': {}
    }
  }
  properties: {
    environmentId: environment.id
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 7200
      replicaRetryLimit: 1
      scheduleTriggerConfig: {
        cronExpression: '0 1 * * *'
        parallelism: 1
        replicaCompletionCount: 1
      }
      registries: commonJobRegistry
      secrets: commonJobSecrets
    }
    template: {
      containers: [
        {
          name: 'blood-tests'
          image: jobsImage
          args: [ 'blood-tests' ]
          env: [
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'db-connection'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
    }
  }
}

resource imotBgJob 'Microsoft.App/jobs@2024-03-01' = {
  name: 'home-imot-bg-daily'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${workloadIdentity.id}': {}
    }
  }
  properties: {
    environmentId: environment.id
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 14400
      replicaRetryLimit: 1
      scheduleTriggerConfig: {
        cronExpression: '30 1 * * *'
        parallelism: 1
        replicaCompletionCount: 1
      }
      registries: commonJobRegistry
      secrets: commonJobSecrets
    }
    template: {
      containers: [
        {
          name: 'imot-bg'
          image: jobsImage
          args: [ 'imot-bg' ]
          env: [
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'db-connection'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
    }
  }
}

output apiFqdn string = deployApi ? api!.properties.configuration.ingress.fqdn : ''
output apiUrl string = deployApi ? 'https://${api!.properties.configuration.ingress.fqdn}' : ''
output dailySchedulesUtc object = {
  bloodTests: '01:00 UTC'
  imotBg: '01:30 UTC'
}
