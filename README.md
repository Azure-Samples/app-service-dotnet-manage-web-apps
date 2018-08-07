---
services: App-Service
platforms: dotnet
author: jianghaolu
---

# Getting started on managing Web Apps in C# #

          Azure App Service basic sample for managing web apps.
           - Create 3 web apps under the same new app service plan:
             - 1, 2 are in the same resource group, 3 in a different one
             - Stop and start 1, restart 2
             - Add Java support to app 3
           - List web apps
           - Delete a web app


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-java/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/app-service-dotnet-manage-web-apps.git

    cd app-service-dotnet-manage-web-apps

    dotnet restore

    dotnet run

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.