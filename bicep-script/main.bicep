param appName string
param rgLocation string = resourceGroup().location

// --> Storage account
// https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: toLower(appName)
  location: rgLocation
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

// --> LogAnalytics workspace
// https://learn.microsoft.com/en-us/azure/templates/microsoft.operationalinsights/workspaces

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: appName
  location: rgLocation
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

// --> Application Insights
// https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/components

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appName
  location: rgLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// --> Conainer App - Managed Environments
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: appName
  location: rgLocation
  properties: {
    // This value must be null configure Monitoring / OTel endpoints in Azure Portal
    daprAIConnectionString: null
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    // https://learn.microsoft.com/en-us/azure/container-apps/opentelemetry-agents
    // I could not set the Monitoring / OTel endpoints with the Bicep script, because the latest version does not have it
    // appInsightsConfiguration: {
    //   connectionString: applicationInsights.properties.ConnectionString
    // }
    // openTelemetryConfiguration: {
    //   tracesConfiguration: {
    //     destinations: ['appInsights']
    //   }
    //   logsConfiguration: {
    //     destinations: ['appInsights']
    //   }
    // }
  }
}

// --> User Assigned - Managed Identity
// https://learn.microsoft.com/en-us/azure/templates/microsoft.managedidentity/userassignedidentities

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: appName
  location: rgLocation
}

// --> Role assignment
// https://learn.microsoft.com/en-us/azure/templates/microsoft.authorization/roleassignments
// https://yourazurecoach.com/2023/02/02/my-developer-friendly-bicep-module-for-role-assignments

var roleDefinitions = [
  {
    Name: 'QueueContributor' // Storage Queue Data Contributor
    Id: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
  }
  {
    Name: 'TableContributor' // Storage Table Data Contributor
    Id: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
  }
]

resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for roleDefinition in roleDefinitions: {
  name: guid(resourceGroup().id, userAssignedIdentity.id, roleDefinition.Name)
  scope: storageAccount
  properties: {
    principalId: userAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', roleDefinition.Id)
  }
}]

// --> Container App - DaprApi
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps

var daprApiName = 'dapr-api'

resource daprApiContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: daprApiName
  location: rgLocation
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {} // Strange, but this is how it works
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      dapr: {
        enabled: true
        appId: daprApiName
        appProtocol: 'http'
        appPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: daprApiName
          image: '19balazs86/dapr-api:1.4'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
      }
    }
  }
}

// --> Dapr component: Bindings for Storage Queues
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/connectedenvironments/daprcomponents

resource bindingQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  name: 'order-binding'
  parent: containerAppEnv
  properties: {
    componentType: 'bindings.azure.storagequeues'
    version: 'v1'
    scopes: [daprApiName]
    metadata: [
      {
        name: 'accountName'
        value: storageAccount.name
      }
      {
        name: 'azureClientId' // Azure-specific value, you can use the managed identity instead of the storage key
        value: userAssignedIdentity.properties.clientId
      }
      {
        name: 'queueName'
        value: 'order'
      }
      {
        name: 'direction'
        value: 'input, output'
      }
      {
        name: 'route'
        value: '/order-binding/checkout'
      }
    ]
  }
}

// --> Dapr component: State store for Table storage
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/connectedenvironments/daprcomponents

resource storageTableComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  name: 'my-state-store'
  parent: containerAppEnv
  properties: {
    componentType: 'state.azure.tablestorage'
    version: 'v1'
    scopes: [daprApiName]
    metadata: [
      {
        name: 'accountName'
        value: storageAccount.name
      }
      {
        name: 'azureClientId' // Azure-specific value, you can use the managed identity instead of the storage key
        value: userAssignedIdentity.properties.clientId
      }
      {
        name: 'tableName'
        value: 'state'
      }
    ]
  }
}

// --> Container App - Echo-Server
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps

var echoServerName = 'echo-server'

resource echoServerContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: echoServerName
  location: rgLocation
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      dapr: {
        enabled: true
        appId: echoServerName
        appProtocol: 'http'
        appPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: echoServerName
          image: 'mendhak/http-https-echo:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output DaprApiURL string    = daprApiContainerApp.properties.configuration.ingress.fqdn
output EchoServerURL string = echoServerContainerApp.properties.configuration.ingress.fqdn