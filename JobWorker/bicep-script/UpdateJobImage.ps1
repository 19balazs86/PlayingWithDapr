param(
    # [Parameter(Mandatory=$false)]
    [string] $ImageName = "19balazs86/job-worker:latest"
)

az containerapp job update `
    --name "job-sender-manual" `
    --resource-group "ContainerAppDaprTest" `
    --image $ImageName