#!/bin/bash

# Set a default value if $1 is not provided in this way: ./update-job-image.sh 19balazs86/job-worker:latest
imageName=${1:-'19balazs86/job-worker:latest'}

# https://learn.microsoft.com/en-us/cli/azure/containerapp/job?view=azure-cli-latest#az-containerapp-job-update

az containerapp job update \
    --name "job-queue-sender" \
    --resource-group "ContainerAppDaprTest" \
    --image $imageName