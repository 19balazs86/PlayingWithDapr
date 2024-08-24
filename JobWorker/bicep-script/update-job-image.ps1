param(
    # [Parameter(Mandatory=$false)]
    [string] $ImageName = "19balazs86/job-worker:latest"
)

# https://learn.microsoft.com/en-us/cli/azure/containerapp/job?view=azure-cli-latest#az-containerapp-job-update

az containerapp job update `
    --name "job-sender-manual" `
    --resource-group "ContainerAppDaprTest" `
    --image $ImageName