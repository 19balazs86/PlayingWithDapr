param appName string

// var rgLocation = resourceGroup().location

module moduleJobQueueSender 'module-JobQueueSender.bicep' = {
  name: 'Main-JobQueueSender'
  params: {
    appName: appName
  }
}

module moduleJobQueueReceiver 'module-JobQueueReceiver.bicep' = {
  name: 'Main-JobQueueReceiver'
  params: {
    appName: appName
  }
}