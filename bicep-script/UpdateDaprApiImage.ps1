param(
    # [Parameter(Mandatory=$false)]
    [string] $ImageName = "19balazs86/dapr-api:latest"
)

# This update makes a copy of the current container-app BUT only changes the image (also it triggers a new revision)

az containerapp update `
    --name "dapr-api" `
    --resource-group "ContainerAppDaprTest" `
    --image $ImageName

# It can make additional changes, see to the documentation
# https://learn.microsoft.com/en-us/cli/azure/containerapp?view=azure-cli-latest#az-containerapp-update