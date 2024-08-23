param appName string

var rgLocation = resourceGroup().location

// --> Existing: Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: toLower(appName)
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

// --> Existing: Managed Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: appName
}

// --> Existing: UserAssigned Identity
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: appName
}

// --> Container Job: Queue-receiver (Trigger: Event)

// - Bicep template: https://learn.microsoft.com/en-us/azure/templates/microsoft.app/jobs
// - Examples with trigger types: https://learn.microsoft.com/en-us/azure/container-apps/jobs
// - Example: https://learn.microsoft.com/en-us/azure/container-apps/tutorial-event-driven-jobs
// - Scaling rules docs: https://learn.microsoft.com/en-us/azure/container-apps/scale-app

resource containerJob 'Microsoft.App/jobs@2024-03-01' = {
  name: 'job-queue-receiver'
  location: rgLocation
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}': {}
    }
  }
  properties: {
    environmentId: containerAppEnv.id
    configuration: {
      triggerType: 'Event'
      replicaTimeout: 180 // 3 minutes
      replicaRetryLimit: 0
      secrets: [
        {
          name: 'secret-storage-conn-string'
          value: storageAccountConnectionString
        }
      ]
      eventTriggerConfig: {
        parallelism: 1 // Number of parallel replicas of a job that can run in an execution
        replicaCompletionCount: 1
        scale: {
          minExecutions: 0
          // The receiver processes messages until the queue is empty. Therefore, the maxExecutions value can be set to 1
          // However, based on the metadata.queueLength value, multiple executions can process messages faster
          maxExecutions: 1
          pollingInterval: 30 // seconds
          rules: [
            {
              name: 'queue-scaling-rule'
              type: 'azure-queue'
              auth: [
                {
                  secretRef: 'secret-storage-conn-string'
                  triggerParameter: 'connection'
                }
              ]
              metadata: {
                accountName: storageAccount.name
                queueLength: '5'
                queueName: 'job-queue'
              }
            }
          ]
        }
      }
    }
    template: {
      containers: [
        {
          name: 'queue-receiver'
          // https://github.com/MicrosoftDocs/azure-docs/blob/main/articles/container-apps/background-processing.md#deploy-the-background-application
          // Image for test purpose mcr.microsoft.com/azuredocs/containerapps-queuereader
          image: '19balazs86/job-worker:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'WorkerSettings__SendNumberOfMessages'
              value: '0' // 0 means not sending messages but receiving them
            }
            {
              name: 'WorkerSettings__IsShortRunningJob'
              value: 'true'
            }
            {
              name: 'AZURE_CLIENT_ID' // MUST specify which UserAssigned Identity can be used by the DefaultAzureCredential
              value: userAssignedIdentity.properties.clientId
            }
          ]
        }
      ]
    }
  }
}