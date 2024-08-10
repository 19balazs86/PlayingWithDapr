# Playing with Dapr

This repository contains several examples to test Dapr features.

## Prerequisites

- [Install Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli)
  - Option #1: [Downalod and install the dapr.msi](https://github.com/dapr/cli/releases) 👤*Dapr-CLI-releases*
  - Option #2: `winget install Dapr.CLI`
- [Dapr in self-hosted mode without Docker](https://docs.dapr.io/operations/hosting/self-hosted/self-hosted-no-docker) `dapr init --slim`
- Setup: Redis server on localhost
- Run the dapr.yaml file
  - Option #1: Using the Darp CLI: `dapr run -f .`
  - Option #2: [Visual Studio Dapr extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vs-dapr) 📚*Marketplace* | You can select Dapr as startup project and run it

## Resources

- [Documentation](https://docs.dapr.io) 📓*Official*
  - [QuickStarts examples](https://docs.dapr.io/getting-started/quickstarts) | [Example code](https://github.com/dapr/quickstarts) 👤*Dapr Quickstarts*
  - [Reference: Component specs](https://docs.dapr.io/reference/components-reference)
  - [Building blocks](https://docs.dapr.io/developing-applications/building-blocks)
  - [Reference: Dapr CLI](https://docs.dapr.io/reference/cli)
  - [Multi-App Run](https://docs.dapr.io/developing-applications/local-development/multi-app-dapr-run) (with file: dapr.yaml)
- [Dapr SDK](https://github.com/dapr/dotnet-sdk), [Examples](https://github.com/dapr/dotnet-sdk/tree/master/examples) 👤*Dapr*
- [Azure Container Apps with Dapr overview](https://learn.microsoft.com/en-us/azure/container-apps/dapr-overview) 📚*MS-Learn*
- [eBook: Dapr for .NET Developers](https://github.com/dotnet-architecture/eBooks/blob/1ed30275281b9060964fcb2a4c363fe7797fe3f3/current/dapr-for-net-developers/Dapr-for-NET-Developers.pdf) 👤*.NET Architecture*