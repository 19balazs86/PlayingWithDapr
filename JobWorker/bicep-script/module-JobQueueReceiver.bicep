param appName string
#disable-next-line secure-secrets-in-params
param connStringSecretIdentifier string

var rgLocation = resourceGroup().location

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
// - Keda scaler: https://keda.sh/docs/2.15/scalers/azure-storage-queue

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
          keyVaultUrl: connStringSecretIdentifier
          identity: userAssignedIdentity.id
        }
      ]
      eventTriggerConfig: {
        parallelism: 1 // Number of parallel replicas in a job execution
        replicaCompletionCount: 1
        scale: {
          minExecutions: 0
          // Allowing more executions at the same time can process messages faster
          // This is determined by: NumberOfMessagesInTheQueue / metadata.queueLength
          maxExecutions: 1
          pollingInterval: 30 // seconds
          rules: [
            {
              name: 'queue-scaling-rule'
              type: 'azure-queue'
              // identity: was removed in the latest template, it does not support auth via ManagedIdentity, need to use the con-string
              auth: [
                {
                  secretRef: 'secret-storage-conn-string'
                  triggerParameter: 'connection'
                }
              ]
              metadata: {
                accountName: toLower(appName) // Storage Account name
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