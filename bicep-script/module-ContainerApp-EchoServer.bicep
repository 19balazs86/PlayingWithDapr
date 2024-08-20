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
      secrets: [
        {
          name: 'simple-secret'
          value: 'My simple secret value'
        }
        // {
        //   name: 'kv-secret' // You can use it with secretRef in env variables
        //   // You can find this URL by navigating to KV, selecting a secret, opening the current revision, and copying the value from the 'Secret Identifier' field
        //   keyVaultUrl: 'https://KeyVaultName.vault.azure.net/secrets/MyConnectionString/cb6eb74f67434e6f83996e867e4f4c76'
        //   // This is the userAssignedIdentity.id value
        //   identity: '/subscriptions/<GUID>/resourcegroups/<ResGroupName>/providers/Microsoft.ManagedIdentity/userAssignedIdentities/<MyIdentityName>'
        // }
      ]
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
          env: [
            {
              name: 'env-simple-secret'
              secretRef: 'simple-secret'
            }
          ]
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