param userAssignedIdentityId string
param conatinerAppName string
param containerAppEnvId string
param appInsightsConnectionString string

var rgLocation = resourceGroup().location

// --> Container App: DaprApi
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps

resource daprApiContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: conatinerAppName
  location: rgLocation
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      // '${userAssignedIdentity.id}': {} // When it was in the main.bicep
      '${userAssignedIdentityId}': {} // Strange, but this is how it works
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvId
    configuration: {
      maxInactiveRevisions: 5
      ingress: {
        external: true
        targetPort: 8080
      }
      dapr: {
        enabled: true
        appId: conatinerAppName
        appProtocol: 'http'
        appPort: 8080
        enableApiLogging: true
      }
    }
    template: {
      containers: [
        {
          name: conatinerAppName
          image: '19balazs86/dapr-api:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsightsConnectionString
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
        rules: [
          {
            name: 'rule-http'
            http: {
              metadata:{
                concurrentRequests: '15'
              }
            }
          }
        ]
      }
    }
  }
}

output ingressFQDN string = daprApiContainerApp.properties.configuration.ingress.fqdn