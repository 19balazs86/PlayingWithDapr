param appName string

// var rgLocation = resourceGroup().location

// resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
//   name: toLower(appName)
// }

// var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

module moduleJobQueueSender 'module-JobQueueSender.bicep' = {
  name: 'Main-JobQueueSender'
  params: {
    appName: appName
  }
}