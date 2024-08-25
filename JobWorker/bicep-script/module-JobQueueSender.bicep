param appName string
param cronExpression string = ''

var rgLocation = resourceGroup().location

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: appName
}

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: appName
}

// --> Container Job: Queue-sender (Trigger: Manual or Schedule)

// - Bicep template: https://learn.microsoft.com/en-us/azure/templates/microsoft.app/jobs
// - Examples with trigger types: https://learn.microsoft.com/en-us/azure/container-apps/jobs
// - Example: https://learn.microsoft.com/en-us/azure/container-apps/tutorial-event-driven-jobs

resource containerJob 'Microsoft.App/jobs@2024-03-01' = {
  name: 'job-queue-sender'
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
      triggerType: cronExpression == '' ? 'Manual' : 'Schedule'
      replicaTimeout: 90 // seconds
      replicaRetryLimit: 0
      manualTriggerConfig: cronExpression != '' ? null : {
        parallelism: 1
        replicaCompletionCount: 1
      }
      scheduleTriggerConfig: cronExpression == '' ? null : {
        cronExpression: cronExpression
        parallelism: 1
        replicaCompletionCount: 1
      }
    }
    template: {
      containers: [
        {
          name: 'queue-sender'
          image: '19balazs86/job-worker:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'WorkerSettings__SendNumberOfMessages'
              value: '30'
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