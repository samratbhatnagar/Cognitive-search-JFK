#https://powerbi.microsoft.com/en-us/blog/working-with-powershell-in-power-bi/
#https://docs.microsoft.com/en-us/rest/api/power-bi/
#https://docs.microsoft.com/en-us/power-bi/developer/register-app

param (
    [System.Boolean]$doInstall = false,
    [System.String]$storageAccount = ""
)

if ($doInstall)
{
    Install-Module -Name MicrosoftPowerBIMgmt
}

#won't work with MFA;
#$creds = Get-credential "chris@solliance.net";
#Login-PowerBI -Credential $creds;

Login-PowerBI

$accessToken = Get-PowerBIAccessToken

$workspace = Get-PowerBIWorkspace -Scope Individual -Name "AzureSearch"

if (!$workspace)
{
    $workspace = New-PowerBIWorkspace -Name "AzureSearch"
}

#create data data...
New-PowerBIDataset -Name "AzureSearch";

#create the data source

#upload report...
$path = "";
New-PowerBIReport -Name "AzureSearch" -Path "" -Workspace $workspace;

#refresh the report...
#Invoke-PowerBIRestMethod