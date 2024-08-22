New-AzResourceGroupDeployment `
    -name "Main-Deployment" `
    -ResourceGroupName "ContainerAppDaprTest" `
    -TemplateFile "main.bicep" `
    -TemplateParameterFile "main.parameters.json"