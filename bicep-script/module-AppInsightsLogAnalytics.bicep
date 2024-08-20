param appName string

var rgLocation = resourceGroup().location

// --> LogAnalytics workspace
// https://learn.microsoft.com/en-us/azure/templates/microsoft.operationalinsights/workspaces

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: appName
  location: rgLocation
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

// --> Application Insights
// https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/components

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appName
  location: rgLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

output logAnalytics_CustomerId string      = logAnalytics.properties.customerId
#disable-next-line outputs-should-not-contain-secrets
output logAnalytics_SharedKey string       = logAnalytics.listKeys().primarySharedKey
output appInsights_ConnectionString string = applicationInsights.properties.ConnectionString