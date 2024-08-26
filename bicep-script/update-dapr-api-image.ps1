param(
    # [Parameter(Mandatory=$false)]
    [string] $ImageName = "19balazs86/dapr-api:latest"
)

# Manage revisions in Azure Container Apps
# https://learn.microsoft.com/en-us/azure/container-apps/revisions-manage

# This update makes a copy of the current container-app BUT only changes the image (also it triggers a new revision)

az containerapp update `
    --name "dapr-api" `
    --resource-group "ContainerAppDaprTest" `
    --image $ImageName

# It can make additional changes, see to the documentation
# https://learn.microsoft.com/en-us/cli/azure/containerapp?view=azure-cli-latest#az-containerapp-update

# -------------------------------------------
# Create a new revision based on the previous
# It can be used as the 'az containerapp update'
# https://learn.microsoft.com/en-us/cli/azure/containerapp/revision?view=azure-cli-latest#az-containerapp-revision-copy

# az containerapp revision copy `
#     --name "dapr-api" `
#     --resource-group "ContainerAppDaprTest" `
#     --image $ImageName