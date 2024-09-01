param appName string

// --> Declare variables
var nameToken = substring(uniqueString(resourceGroup().id), 0, 4)

var storageAccountName = toLower('${appName}${nameToken}')

var keyVaultName = '${appName}${nameToken}'

// --> Module: Create 'job-queue'
module moduleCreateQueue 'module-CreateJobQueue.bicep' = {
  name: 'Main-JobCreateQueue'
  params: {
    storageAccountName: storageAccountName
  }
}

// --> Module: KeyVault
module moduleKeyVault 'module-KeyVault.bicep' = {
  name: 'Main-JobKeyVault'
  params: {
    appName: appName
    keyVaultName: keyVaultName
    storageAccountName: storageAccountName
  }
}

// --> Existing: Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

var storageQueueEndpoint = storageAccount.properties.primaryEndpoints.queue

// --> Module: QueueSender
module moduleJobQueueSender 'module-JobQueueSender.bicep' = {
  name: 'Main-JobQueueSender'
  params: {
    appName: appName
    storageQueueEndpoint: storageQueueEndpoint
    // cronExpression: '*/5 * * * *' // Every 5 minutes | Optional parameter: If not defined, any cron expression will result in a 'Manul' job
  }
}

// --> Module: QueueReceiver
module moduleJobQueueReceiver 'module-JobQueueReceiver.bicep' = {
  name: 'Main-JobQueueReceiver'
  params: {
    appName: appName
    storageAccountName: storageAccountName
    storageQueueEndpoint: storageQueueEndpoint
    connStringSecretIdentifier: moduleKeyVault.outputs.connStringSecretIdentifier
  }
}