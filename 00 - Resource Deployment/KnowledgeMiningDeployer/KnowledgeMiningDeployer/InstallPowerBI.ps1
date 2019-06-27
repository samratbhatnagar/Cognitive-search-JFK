Install-Module -Name MicrosoftPowerBIMgmt

$creds = Get-credential "chris@solliance.net";

Login-PowerBI -Credential $creds

Get-PowerBIWorkspace -Scope Individual

$accessToken = Get-PowerBIAccessToken

Invoke-PowerBIRestMethod

