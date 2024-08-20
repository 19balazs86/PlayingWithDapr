New-AzResourceGroupDeployment `
    -name "Main-Deployment" `
    -ResourceGroupName "ContainerAppDaprTest" `
    -TemplateFile "main.bicep" `
    -TemplateParameterFile "main.parameters.json"

# This command does not work in powershell
# az deployment group create --name Main-Deployment --resource-group ContainerAppDaprTest --template-file main.bicep --parameters @main.parameters.json