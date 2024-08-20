param containerAppEnvName string
param componentScopes array
param storageAccountName string
param userAssignedIdentityClientId string

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvName
}

// --> Dapr component: Bindings for Storage Queues
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/connectedenvironments/daprcomponents

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
      }
      {
        name: 'tableName'
        value: 'state'
      }
    ]
  }
}