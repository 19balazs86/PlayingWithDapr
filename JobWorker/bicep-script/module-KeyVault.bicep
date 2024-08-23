param appName string
param azureUserObjectID string

var rgLocation = resourceGroup().location
var tenantId = subscription().tenantId

// This is just for extra effort and fun. Overall, it makes things more complex but more secure.
// Much simpler version would be to just set the secret value in 'job-queue-receiver' with the Storage Account connection string

// --> KeyVault
// https://learn.microsoft.com/en-us/azure/templates/microsoft.keyvault/vaults

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: appName
  location: rgLocation
  properties: {
    tenantId: tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: azureUserObjectID
        permissions: {
          certificates: ['all']
          keys: ['all']
          secrets: ['all']
          storage: ['all']
        }
      }
    ]
  }
}

// --> Existing: Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: toLower(appName)
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

// --> Secret
// https://learn.microsoft.com/en-us/azure/templates/microsoft.keyvault/vaults/secrets

resource kvSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'StorageAccount-ConnString'
  parent: keyVault
  properties: {
    contentType: 'string'
    value: storageAccountConnectionString
  }
}

// --> Existing: UserAssigned Identity
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: appName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, userAssignedIdentity.id, 'KVSecretsUser')
  scope: keyVault
  properties: {
    principalId: userAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
  }
}

output connStringSecretIdentifier string = kvSecret.properties.secretUri