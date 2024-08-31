#!/bin/bash

# https://learn.microsoft.com/en-us/cli/azure/deployment/group?view=azure-cli-latest#az-deployment-group-create

az deployment group create \
    --name "Main-Deployment" \
    --resource-group "ContainerAppDaprTest" \
    --template-file "main.bicep" \
    --parameters "@main.parameters.json"
    # --query "properties.outputs" # https://jmespath.org

# Alternative methods of passing parameters
# One line, space-separated
# --parameters appName=MyAppname param2=$EnvVarName param3="Param 3 value"

# Multiline
# --parameters \
#     appName=MyAppname \
#     param2=$EnvVarName \
#     param3="Param 3 value"

# Use both: parameters and parameter
# --parameters "@main.parameters.json" \
# --parameter param2=$EnvVarName 