#testing  ./Install.ps1 -AzureAdminClientId "" -AzureAdminClientSecret "" -TenantId "" -SubscriptionId "" -Region "" -ResourceGroupName "" -ResourcePrefix "" -SearchServiceApiVersion "" -UseSampleData $true

param(
[System.String]$AzureAdminClientId,
[System.String] $AzureAdminClientSecret,
[System.String] $TenantId,
[System.String] $SubscriptionId,
[System.String] $Region,
[System.String] $ResourceGroupName,
[System.String] $ResourcePrefix,
[System.String] $SearchServiceApiVersion,
[System.String] $UseSampleData)

add-type -Path "Newtonsoft.Json.dll";

$json = Get-Content "configuration.development.json" -raw;

$configuration = [Newtonsoft.Json.JsonConvert]::DeserializeObject($json);
	
$configuration.AdminAzureClientId = $AzureAdminClientId;
$configuration.AdminAzureClientSecret = $AzureAdminClientSecret;
$configuration.TenantId = $TenantId;
$configuration.SubscriptionId = $SubscriptionId;
$configuration.Region = $Region;
$configuration.ResourceGroupName = $ResourceGroupName;
$configuration.ResourcePrefix = $ResourcePrefix;
$configuration.SearchServiceApiVersion = $SearchServiceApiVersion;
$configuration.UseSampleData = $UseSampleData;

$json = [Newtonsoft.Json.JsonConvert]::SerializeObject($configuration);

remove-item "configuration.production.json"
add-content "configuration.production.json" $json;
	
#run the deployment...
.\KnowledgeMiningDeployer.exe