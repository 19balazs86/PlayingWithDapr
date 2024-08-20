param userAssignedIdentityName string
param storageAccountName string

var rgLocation = resourceGroup().location

// --> User Assigned - Managed Identity
// https://learn.microsoft.com/en-us/azure/templates/microsoft.managedidentity/userassignedidentities

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentityName
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

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for roleDefinition in roleDefinitions: {
  name: guid(resourceGroup().id, userAssignedIdentity.id, roleDefinition.Name)
  scope: storageAccount
  properties: {
    principalId: userAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', roleDefinition.Id)
  }
}]

output id string       = userAssignedIdentity.id
output clientId string = userAssignedIdentity.properties.clientId