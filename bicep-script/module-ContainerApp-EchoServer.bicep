param conatinerAppName string
param containerAppEnvId string

var rgLocation = resourceGroup().location

// --> Container App: Echo-Server
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps

resource echoServerContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: conatinerAppName
  location: rgLocation
  properties: {
    managedEnvironmentId: containerAppEnvId
    configuration: {
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
          image: 'mendhak/http-https-echo:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output ingressFQDN string = echoServerContainerApp.properties.configuration.ingress.fqdn