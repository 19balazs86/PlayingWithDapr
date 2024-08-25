param appName string
param azureUserObjectID string

// var rgLocation = resourceGroup().location

// --> Module: Create 'job-queue'
module moduleCreateQueue 'module-CreateJobQueue.bicep' = {
  name: 'Main-JobCreateQueue'
  params: {
    appName: appName
  }
}

// --> Module: KeyVault
module moduleKeyVault 'module-KeyVault.bicep' = {
  name: 'Main-JobKeyVault'
  params: {
    appName: appName
    azureUserObjectID: azureUserObjectID
  }
}

// --> Module: QueueSender
module moduleJobQueueSender 'module-JobQueueSender.bicep' = {
  name: 'Main-JobQueueSender'
  params: {
    appName: appName
    // cronExpression: '*/5 * * * *' // Every 5 minutes | Optional parameter: If not defined, any cron expression will result in a 'Manul' job
  }
}

// --> Module: QueueReceiver
module moduleJobQueueReceiver 'module-JobQueueReceiver.bicep' = {
  name: 'Main-JobQueueReceiver'
  params: {
    appName: appName
    connStringSecretIdentifier: moduleKeyVault.outputs.connStringSecretIdentifier
  }
}