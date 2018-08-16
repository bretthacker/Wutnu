# WutNu - Mother of All Azure Demos (MOAAD)

__Quick Start__

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fbretthacker%2Fwutnu%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

__Overview__

This app is designed as a multi-purpose Azure demo, utilizing:

- Web Apps
- Web Jobs
- Azure Storage (blobs and queues)
- Storage SAS tokens
- Azure SQL DB
- Azure Redis Cache
- Azure Active Directory/B2C

__Azure AD Integration Demo__

The app presents as a general purpose URL shortener. Logging in enables shortened links to require a login from AAD before they are lengthened. Social accounts are enabled via AAD B2C, and work accounts are enabled via AAD user accounts and optionally, accounts linked via AAD B2B.

For the Azure Active Directory settings, please see the following articles for details:

- [https://azure.microsoft.com/en-us/documentation/articles/active-directory-devquickstarts-webapp-dotnet/](https://azure.microsoft.com/en-us/documentation/articles/active-directory-devquickstarts-webapp-dotnet/)
- [https://azure.microsoft.com/en-us/documentation/services/active-directory-b2c/](https://azure.microsoft.com/en-us/documentation/services/active-directory-b2c/)

For your primary AAD, create a group named "WutNuAdmins" and add any users that need access to the error logs.

__Storage and Web Jobs Demo__

Files are uploadable via the web portal - each authenticated user gets a blob storage container in the designated storage account, linked to their login. These uploaded files can then be shortened, and the shortened URLs can be protected behind a login so they can't be lengthened until the person authenticates. Finally, queues are used to audit the URL lengthening. PowerBI embedded integration is on the roadmap.

__Console Transfer Utility__

User accounts have a rudimentary "API Key" generated that is associated with their blob container. The "BlobCopy" project is a simple console app that allows list/put/get operations against the container associated with the api key (kept in the settings file). The utility makes an API call and, for put and get operations, gets a SAS token to upload and download directly to blob storage.

__Powershell Automation__

Included in the repo is ARMDeployment.ps1. This script can be downloaded and run locally. It will allow you to set your variables in script then execute the ARM deployment against this GitHub repo.

__Sample Code__

- This is sample code - it's strictly a proof of concept.
