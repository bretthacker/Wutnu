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

