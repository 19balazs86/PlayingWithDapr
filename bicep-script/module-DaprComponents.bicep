param containerAppEnvName string
param componentScopes array
param storageAccountName string
param userAssignedIdentityClientId string

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvName
}

// --> Dapr component: Bindings for Storage Queues
// Bicep: https://learn.microsoft.com/en-us/azure/templates/microsoft.app/connectedenvironments/daprcomponents

// Get the JSON definition of the component
// az resource show --ids /subscriptions/<GUID>/resourceGroups/<ResGroupName>/providers/Microsoft.App/managedEnvironments/<ManagedEnvname>/daprComponents/<DaprComponentName>

// Aside from Dapr Components, there is another way in Azure to connect services to the application
// Service Connector (preview) is a general solution for other services as well (Azure Functions, App Services)
// - Documentation: https://learn.microsoft.com/en-us/azure/service-connector/overview
// - Example in Container App: https://learn.microsoft.com/en-us/azure/container-apps/service-connector
// - Bicep: https://learn.microsoft.com/en-us/azure/templates/microsoft.servicelinker/linkers
// Note: Currently, I prefer the Dapr Components

resource bindingQueueComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  name: 'order-binding'
  parent: containerAppEnv
  properties: {
    componentType: 'bindings.azure.storagequeues'
    version: 'v1'
    scopes: componentScopes
    metadata: [
      {
        name: 'accountName'
        value: storageAccountName
      }
      {
        name: 'azureClientId' // Azure-specific value, you can use the managed identity instead of the storage key
        value: userAssignedIdentityClientId
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
    scopes: componentScopes
    metadata: [
      {
        name: 'accountName'
        value: storageAccountName
      }
      {
        name: 'azureClientId' // Azure-specific value, you can use the managed identity instead of the storage key
        value: userAssignedIdentityClientId
        // secretRef: You can use this field in the same way in the module-ContainerApp-EchoServer.bicep by defining the secrets below
      }
      {
        name: 'tableName'
        value: 'state'
      }
    ]
    // secrets: []
  }
}

// --> Dapr component: Azure Key Vault secret store
// You can define it similarly to the other components, just enough to populate the vaultName and azureClientId metadata fields
// https://docs.dapr.io/reference/components-reference/supported-secret-stores/azure-keyvault