# WutNu
Based on the initial "Unofficial Work Account &amp; B2C Combined Sample App" by Danny Strockis at [https://github.com/dstrockis/AAD-B2C-Hybrid](https://github.com/dstrockis/AAD-B2C-Hybrid). 

This app is designed as a multi-purpose Azure demo, utilizing:

- Web Apps
- Web Jobs
- Azure Storage (blobs and queues)
- Storage SAS tokens
- Azure SQL DB
- Azure Redis Cache
- Azure Active Directory/B2C

The app presents as a general purpose URL shortener. Logging in enables shortened links to require a login from AAD before they are lengthened. Social accounts are enabled via AAD B2C, and work accounts are enabled via AAD user accounts and optionally, accounts linked via AAD B2B.

Files are uploadable via the web portal - each authenticated user gets a blob storage container in the designated storage account, linked to their login. These uploaded files can then be shortened, and the shortened URLs can be protected behind a login so they can't be lengthened until the person authenticates. Finally, queues are used to audit the URL lengthening, and a rudimentary report is available.

User accounts have a rudimentary "API Key" generated that is associated with their blob container. The "BlobCopy" project is a simple console app that allows list/put/get operations against the container associated with the api key (kept in the settings file). The utility makes an API call and, for put and get operations, gets a SAS token to upload and download directly to blob storage.

ISSUES:
- There is an ARM project (WutNuRG) in the solution but it's not complete.
- The report engine "works on my box" but rendering is having an issue in Azure - the report renders and the database is called; it may be font-related.
- If you decide to use the reports, they are designed to be run from blob storage - create a "reports" container and copy the following from Wutnu.Web/localreports:
  - TrafficReport.cshtml
  - TrafficReport.rdlc
  - _ViewStart.cshtml
- web.config has application settings commented out and is wired to look for "settings.config". You will need to comment out the settings.config call and uncomment the local settings, or create your own settings.config.
- The ARM template has all of the application settings defined for the web app, but it's not ready for deployment yet.
- This is sample code - it's strictly a proof of concept.

INSTALLATION:
- Setup an Azure Web App in a new Resource Group (place all the other resources in this group)
  - [https://azure.microsoft.com/en-us/documentation/services/app-service/web/](https://azure.microsoft.com/en-us/documentation/services/app-service/web/)
- Create a Storage Account
  - [https://azure.microsoft.com/en-us/documentation/services/storage/](https://azure.microsoft.com/en-us/documentation/services/storage/)
- Create a Redis Cache
  - [https://azure.microsoft.com/en-us/documentation/services/redis-cache/](https://azure.microsoft.com/en-us/documentation/services/redis-cache/)
- Create a SQL DB
  - [https://azure.microsoft.com/en-us/documentation/services/sql-database/](https://azure.microsoft.com/en-us/documentation/services/sql-database/)
- The web app has a lot of custom settings that you might like to use in Azure. Typically, you will use some settings while developing in Visual Studio, then different settings in Azure, and possibly different settings for each slot of your app in Azure. If you want to have settings in Azure override the settings in your deployed web.config, you can use the following PowerShell to make it easier to establish the name/value pairs in your app:
  
```powershell
$webappname="wuttest"
$webappRG="wuttest"

Add-AzureRmAccount
#Set-AzureRmContext -SubscriptionName "yourSubscriptionName"

$settings=@{
    "DomainName" = "localhost:44316";
    "RedisConnection" = "[parameters('RedisConnectionString')]";
    "RedisUrlDBNum" = "[parameters('RedisUrlDBNum')]";
    "RedisUserDBNum" = "[parameters('RedisUserDBNum')]";
    "StorageConnectionString" = "[parameters('StorageConnectionString')]";
    "GraphKey" = "[parameters('AADGraphKey')]";
    "ActivateWebApiTracing" = "false";
    "ida:AadInstance" = "https://login.microsoftonline.com/{0}";
    "ida:RedirectUri" = "https://localhost:44316";
    "ida:TenantB2B" = "[parameters('TenantB2B')]";
    "ida:ClientIdB2B" = "[parameters('ClientIdB2B')]";
    "ida:TenantB2C" = "[parameters('TenantB2C')]";
    "ida:ClientIdB2C" = "[parameters('ClientIdB2C')]";
    "ida:SignUpPolicyId" = "B2C_1_DefaultSignup";
    "ida:SignInPolicyId" = "B2C_1_DefaultSignin";
    "ida:UserProfilePolicyId" = "B2C_1_DefaultProfileEditPolicy";
    "ConfigStorageCors" = "true";
    "Environment" = "[parameters('DeploymentEnvironment')]";
    "LocalReports" = "false";
    "EnableDashboardLogging" = "true"
}

Set-AzureRmWebApp -AppSettings $settings -Name $webappname -ResourceGroupName $webappRG
```

For each of those settings, either in the Azure web application settings, or in your web.config, you'll need to populate the values from the services you just spun up.

For the Azure Active Directory settings, please see the following articles for details:

- [https://azure.microsoft.com/en-us/documentation/articles/active-directory-devquickstarts-webapp-dotnet/](https://azure.microsoft.com/en-us/documentation/articles/active-directory-devquickstarts-webapp-dotnet/)
- [https://azure.microsoft.com/en-us/documentation/services/active-directory-b2c/](https://azure.microsoft.com/en-us/documentation/services/active-directory-b2c/)

For your primary AAD, create a group named "WutNuAdmins" and add any users that need access to the error logs.