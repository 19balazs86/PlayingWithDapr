#!/bin/bash

# azureUserObjectID=$(az ad user show --id "<AzureUserPrincipalName>" --query "id" --output tsv)

# https://learn.microsoft.com/en-us/cli/azure/deployment/group?view=azure-cli-latest#az-deployment-group-create

az deployment group create \
    --name "Main-Job-Deployment" \
    --resource-group "ContainerAppDaprTest" \
    --template-file "main-job.bicep" \
    --parameters "@main-job.parameters.json"
    # --parameter azureUserObjectID=$AZURE_USER_OBJECT_ID # From env-variable