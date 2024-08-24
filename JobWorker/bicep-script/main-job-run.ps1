New-AzResourceGroupDeployment `
    -name "Main-Job-Deployment" `
    -ResourceGroupName "ContainerAppDaprTest" `
    -TemplateFile "main-job.bicep" `
    -TemplateParameterFile "main-job.parameters.json"