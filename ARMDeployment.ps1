$startTime=Get-Date
Write-Host "Beginning deployment at $starttime"

Import-Module Azure -ErrorAction SilentlyContinue
$version ++

#DEPLOYMENT OPTIONS
#Please review the azuredeploy.json file for available options
$RGName        = "[YOUR RESOURCE GROUP]"
$DeployRegion  = "[SELECT AZURE REGION]"
$AssetLocation = "https://raw.githubusercontent.com/bretthacker/wutnu/master/azuredeploy.json"

$parms = @{
    #Azure Web App
    "hostingPlanName"                 = "[WEB SITE NAME, like 'shortener']";

    #SQLDb
    "sqlAdministratorLoginPassword"   = "[SQL database admin account name]";

    #Azure AD
    "B2BGraphKey"                     = "[API key for Azure AD app registration]";
    "tenantB2B"                       = "[name of Azure AD tenant, like contoso.onmicrosoft.com, or tenant ID (guid)]";
    "clientIdB2B"                     = "[Application ID for Azure AD app registration]";
    "B2CGraphKey"                     = "[API key for Azure AD B2C app registration]";
    "tenantB2C"                       = "[Name of Azure AD B2C tenant, like contosob2c.onmicrosoft.com, or tenant ID (guid)]";
    "clientIdB2C"                     = "[Application ID for Azure AD B2C app registration]";
    "AdminGroupId"                    = "[GUID group ID of group in Azure AD tenant containing admin users]";
}
#END DEPLOYMENT OPTIONS

#Dot-sourced variable override (optional, comment out if not using)
$dotsourceSettings="C:\Users\brhacke\OneDrive\Dev\MSFT\A_CustomDeploySettings\wutnu.ps1"
if (Test-Path $dotsourceSettings) {
    . $dotsourceSettings
}

#ensure we're logged in
Get-AzureRmContext -ErrorAction Stop

#deploy

$TemplateFile = "$($AssetLocation)?x=$version"

try {
    Get-AzureRmResourceGroup -Name $RGName -ErrorAction Stop
    Write-Host "Resource group $RGName exists, updating deployment"
}
catch {
    $RG = New-AzureRmResourceGroup -Name $RGName -Location $DeployRegion
    Write-Host "Created new resource group $RGName."
}
$deployError = $null
$deployment = New-AzureRmResourceGroupDeployment -ResourceGroupName $RGName -TemplateParameterObject $parms -TemplateFile $TemplateFile -Name "WutNu$version"  -Force -Verbose -ErrorAction SilentlyContinue -ErrorVariable deployError
if ($deployError) {
    $str = $deployError[0].Message
    $trackingIdSt = $str.IndexOf("The tracking id is '")
    if ($trackingIdSt -lt 0) {
        throw $deployError
    }
    trackingIdSt += "The tracking id is '".Length
    $trackingId = $str.Substring($trackingIdSt, 36)   #length of GUID string
    Start-Sleep -Seconds 2                            #wait for ARM log to complete
    $errorDetail =  get-azurermlog -CorrelationId $trackingId -DetailedOutput
    $errorContent = (ConvertFrom-Json $errorDetail[0].Properties.Content.statusMessage.ToString()).error.details | ConvertTo-Json
    throw $errorContent
}

if ($deployment.ProvisioningState -eq "Succeeded") {
    $siteName = $deployment.Outputs.webSiteName.Value
    start "https://$($siteName).azurewebsites.net/"
    Write-Host "---------"
    $deployment.Outputs | ConvertTo-Json

} else {
    $deperr = Get-AzureRmResourceGroupDeploymentOperation -DeploymentName "WutNu$version" -ResourceGroupName $RGName -ErrorAction Stop
    $deperr | ConvertTo-Json
}

$endTime=Get-Date

Write-Host ""
Write-Host "Total Deployment time:"
New-TimeSpan -Start $startTime -End $endTime | Select Hours, Minutes, Seconds
