New-AzResourceGroupDeployment `
    -name "MyDeployment" `
    -ResourceGroupName "ContainerAppDaprTest" `
    -TemplateFile "main.bicep" `
    -TemplateParameterFile "main.parameters.json"

# This command does not work in powershell
# az deployment group create --name MyDeployment --resource-group ContainerAppDaprTest --template-file main.bicep --parameters @main.parameters.json