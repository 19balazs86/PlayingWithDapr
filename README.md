# Playing with Dapr and Container Apps

This repository contains various examples for testing Dapr features both locally and with Azure Container Apps.

## Table of content

- [Projects in the solution](#projects-in-the-solution)
- [Resources](#resources)
- [Prerequisites](#prerequisites)
- [Infrastructure provisioning](#infrastructure-provisioning-with-a-bicep-template)

## Projects in the solution

#### `DaprWebApi`

- [InvokeMethodEndpoints.cs](DaprWebApi/Endpoints/InvokeMethodEndpoints.cs): invoke the EchoWebApi, a resiliency is defined in [invoke-echo-resiliency.yaml](DaprWebApi/dapr-resources/invoke-echo-resiliency.yaml)
- [StateEndpoints.cs](DaprWebApi/Endpoints/StateEndpoints.cs): state management, defined in [statestore.yaml](common-resources/statestore.yaml)
- [OrderPubSubEndpoints.cs](DaprWebApi/Endpoints/OrderPubSubEndpoints.cs): publish and receive messages, defined in [pubsub.yaml](DaprWebApi/dapr-resources/pubsub.yaml) and [subscription.yaml](DaprWebApi/dapr-resources/subscription.yaml)
- [OrderBindingEndpoints.cs](DaprWebApi/Endpoints/OrderBindingEndpoints.cs): input and output binding using Azure Storage Queue. [binding.yaml](DaprWebApi/dapr-resources/binding.yaml) and [local-secret-store.yaml](DaprWebApi/dapr-resources/local-secret-store.yaml)
- [CronJobEndpoints.cs](DaprWebApi/Endpoints/CronJobEndpoints.cs): handle the Cron binding triggered event, defined in [cron-job-binding.yaml](DaprWebApi/dapr-resources/cron-job-binding.yaml)

#### `EchoWebApi`

- Just a simple Web API that echoes back the request details in a JSON response

#### `JobWorker`

- This is a worker service responsible for sending and receiving messages from a Storage Queue
- Based on the configuration, it can either be a short-running Container-Job or a long-running Container-App (scaling rule can be applied)

#### `ActorService`

- A simple project with a [CounterActor](ActorService/CounterFeature/CounterActor.cs ) to explore Dapr Actor framework

## Resources

- [Documentation](https://docs.dapr.io) 📓*Official*
  - [Building blocks](https://docs.dapr.io/developing-applications/building-blocks)
    - [Actors](https://docs.dapr.io/developing-applications/building-blocks/actors) | [.NET SDK](https://docs.dapr.io/developing-applications/sdks/dotnet/dotnet-actors) | [Quickstart](https://docs.dapr.io/getting-started/quickstarts/actors-quickstart) | [Example](https://github.com/dapr/quickstarts/tree/master/actors) 👤
  - Reference
    - [Dapr API](https://docs.dapr.io/reference/api)
    - [Component specs](https://docs.dapr.io/reference/components-reference)
    - [Dapr CLI](https://docs.dapr.io/reference/cli)
  - [Multi-App Run](https://docs.dapr.io/developing-applications/local-development/multi-app-dapr-run)
- Examples
  - [QuickStarts examples](https://docs.dapr.io/getting-started/quickstarts) 📓 | [with code](https://github.com/dapr/quickstarts) 👤*Quickstarts*
  - [Examples](https://github.com/dapr/dotnet-sdk/tree/master/examples) 👤*SDK*
- eBook: [Dapr for .NET Developers](https://github.com/dotnet-architecture/eBooks/blob/1ed30275281b9060964fcb2a4c363fe7797fe3f3/current/dapr-for-net-developers/Dapr-for-NET-Developers.pdf) 👤*DotNET Architecture*
- [Microservices with Dapr and Azure Container Apps](https://youtu.be/-LeCQvXka9Y) 📽️*1 hour - NDC Conf 2024* | [DaprShop](https://github.com/william-liebenberg/practical-dapr) 👤*William Liebenberg*
- Other
  - [Azure Container Apps with Dapr overview](https://learn.microsoft.com/en-us/azure/container-apps/dapr-overview) 📚*MS-Learn*
  - [Dapr with .NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/frameworks/dapr) 📚*MS-Learn*
  - [Introduction to Dapr](https://www.milanjovanovic.tech/blog/introduction-to-dapr-for-dotnet-developers) 📓*Milan newsletter*

## Prerequisites

- Install: [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli)
  - Option #1: [Downalod and install the dapr.msi](https://github.com/dapr/cli/releases/latest) 👤*Dapr-CLI-releases*
  - Option #2: `winget install Dapr.CLI`
- Runtime: [Upgrade](https://docs.dapr.io/operations/hosting/self-hosted/self-hosted-upgrade) or install: [Dapr in self-hosted mode without Docker](https://docs.dapr.io/operations/hosting/self-hosted/self-hosted-no-docker) `dapr init --slim`
- Dependencies
  - Optional - Redis server: used in [statestore.yaml](common-resources/statestore.yaml) and [pubsub.yaml](DaprWebApi/dapr-resources/pubsub.yaml), but the default is in-memory for both
  - Azure storage account: used storage-queues in [binding.yaml](DaprWebApi/dapr-resources/binding.yaml)
- Run the [dapr.yaml](dapr.yaml) file
  - Option #1: Using the Darp CLI: `dapr run -f .`
  - Option #2: [Visual Studio Dapr extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vs-dapr) 📚*Marketplace* | You can select Dapr as startup project and run it
- Configuration: the [local-secret-store.yaml](DaprWebApi/dapr-resources/local-secret-store.yaml) uses a *secrets.json* file, which can be placed anywhere on your machine with the following content

```json
{
  "storage": {
    "accountName": "The name of the Azure Storage account",
    "accountKey": "The access key of the Azure Storage account"
  }
}
```

## Infrastructure provisioning with a Bicep template

#### `DaprWebApi and EchoWebApi`

- You can find a [main.bicep](bicep-script/main.bicep) template file that contains all the related objects for provisioning the infrastructure
- Container Apps Environment, Storage account, Managed Identity, Container App, Dapr Components

![Bicep template](bicep-script/bicep-infrastructure.JPG)

#### `JobWorker`

- This is a continuation of the previous template with the [main-job.bicep](JobWorker/bicep-script/main-job.bicep) file
- KeyVault with a secret holding the Storage Account connection string
- Jobs: Queue Sender (trigger: Manual) | Queue Receiver (trigger: Event)

![Bicep Job template](JobWorker/bicep-script/bicep-infrastructure.JPG)