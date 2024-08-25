param appName string

var jobQueueName = 'job-queue'

// Queue Sender and Receiver create the 'job-queue' if it does not exist
// The event trigger starts to work when the queue exists, but until then, warning messages are logged in ContainerAppSystemLogs_CL

// --> Existing: Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: toLower(appName)
}

// --> Queue Service
// https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts/queueservices

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
}

// --> Create Queue
// https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts/queueservices/queues

resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: jobQueueName
  parent: queueService
}