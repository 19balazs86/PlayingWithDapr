#!/bin/bash

# https://learn.microsoft.com/en-us/cli/azure/deployment/group?view=azure-cli-latest#az-deployment-group-create

az deployment group create \
    --name "Main-Job-Deployment" \
    --resource-group "ContainerAppDaprTest" \
    --template-file "main-job.bicep" \
    --parameters "@main-job.parameters.json"
    # --query "properties.outputs" # https://jmespath.org