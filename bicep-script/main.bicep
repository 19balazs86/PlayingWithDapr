﻿param appName string
param rgLocation string = resourceGroup().location

var daprApiName    = 'dapr-api'
var echoServerName = 'echo-server'

var nameToken = substring(uniqueString(resourceGroup().id), 0, 4)

var storageAccountName = toLower('${appName}${nameToken}')

// --> Storage account
// https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: rgLocation
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

// --> Module: LogAnalytics workspace + Application Insights
// https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/modules

module moduleAppInsights 'module-AppInsightsLogAnalytics.bicep' = {
  // scope: resourceGroup() // By default the scope is the same as main.bicep
  name: 'Insights-Deployment' // Modules appear as a separate deployment in Azure Settings/Deployments
  params: {
    appName: appName
  }
}

// --> Conainer App - Managed Environments
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: appName
  location: rgLocation
  properties: {
    // In order to configure Monitoring / OTel endpoints in Azure Portal, this value must be null
    daprAIConnectionString: null
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: moduleAppInsights.outputs.logAnalytics_CustomerId
        sharedKey: moduleAppInsights.outputs.logAnalytics_SharedKey
      }
    }
    // https://learn.microsoft.com/en-us/azure/container-apps/opentelemetry-agents
    // I could not set the Monitoring / OTel endpoints with the Bicep script, because the latest version does not have it
    // appInsightsConfiguration: {
    //   connectionString: moduleAppInsights.outputs.appInsights_ConnectionString
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

// --> Module: Managed Identity + Role assignment
module moduleManagedIdentity 'module-ManagedIdentity.bicep' = {
  name: 'ManagedIdentity-Deployment'
  params: {
    userAssignedIdentityName: appName
    storageAccountName: storageAccount.name
  }
}

// --> Module: Container App: DaprApi
module moduleContainerAppDaprApi 'module-ContainerApp-DaprApi.bicep' = {
  name: 'ContainerAppDaprApi-Deployment'
  params: {
    userAssignedIdentityId: moduleManagedIdentity.outputs.id
    conatinerAppName: daprApiName
    containerAppEnvId: containerAppEnv.id
    appInsightsConnectionString: moduleAppInsights.outputs.appInsights_ConnectionString
  }
}

// --> Module: Container App: Echo-Server
module moduleContainerAppEchoServer 'module-ContainerApp-EchoServer.bicep' = {
  name: 'ContainerAppEchoServer-Deployment'
  params: {
    conatinerAppName: echoServerName
    containerAppEnvId: containerAppEnv.id
  }
}

// --> Module: Dapr components
module modulDaprComponents 'module-DaprComponents.bicep' = {
  name: 'DaprComponents-Deployment'
  params: {
    containerAppEnvName: containerAppEnv.name
    componentScopes: [daprApiName]
    storageAccountName: storageAccount.name
    userAssignedIdentityClientId: moduleManagedIdentity.outputs.clientId
  }
}

output DaprApiURL string    = moduleContainerAppDaprApi.outputs.ingressFQDN
output EchoServerURL string = moduleContainerAppEchoServer.outputs.ingressFQDN