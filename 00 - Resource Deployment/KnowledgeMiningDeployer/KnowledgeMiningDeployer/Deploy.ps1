Configuration Deploy
{
  param 
  (
      [System.String]$MachineName = "prov-vm",
	  [System.String]$AdminAzureClientId,
	  [System.String]$AdminAzureClientSecret,
	  [System.String]$TenantId,
	  [System.String]$SubscriptionId,
	  [System.String]$Region = "westus2",
	  [System.String]$ResourceGroupName,
	  [System.String]$ResourcePrefix = "xyz",
	  [System.String]$SearchServiceApiVersion = "2019.05.06-Preview",
	  [System.Boolean]$UseSampleData = $true,
	  [System.String]$ConfigFilePath = "",
	  [System.String]$CustomDataZip = "",
	  [System.String]$Username = "",
	  [System.String]$Password = ""

  )

  $DscWorkingFolder = $PSScriptRoot;

  Node $MachineName
  {
        Script ScriptExample
        {
            SetScript = {

				try
				{
					$githubUrl = "https://github.com/solliancenet/Cognitive-search-JFK/raw/km-deliverable/00%20-%20Resource%20Deployment/KnowledgeMiningDeployer/KnowledgeMiningDeployer/Deployment/";
					$path = $using:dscworkingfolder;
					write-verbose "Script started in $path";
				
					cd $path -ErrorAction SilentlyContinue;
				
					write-verbose "Setting SSL to Tls12";
					[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12;

					write-verbose "Recreating the deployment directory";
					new-item -ItemType directory "$path\deployment" -ErrorAction SilentlyContinue;

					write-verbose "Downloading web, functions and database";
					#download the compiled web and functions...
					#
					Invoke-WebRequest -Uri "$githubUrl/CognitiveSearch.Skills.zip" -ErrorAction SilentlyContinue -Verbose
					Invoke-WebRequest -Uri "$githubUrl/CognitiveSearch.UI.zip" -OutFile "$path/Deployment/CognitiveSearch.UI.zip" -ErrorAction SilentlyContinue -Verbose
					Invoke-WebRequest -Uri "$githubUrl/documents.bacpac" -OutFile "$path/Deployment/documents.bacpac" -ErrorAction SilentlyContinue -Verbose

					write-verbose "Downloading sample files";
					#download the sample files...
					Invoke-WebRequest -Uri "$githubUrl/sample.zip" -OutFile "$path/Deployment/Sample.zip" -ErrorAction SilentlyContinue -Verbose

					write-verbose "Downloading starter files";
					#download extra started files...
					Invoke-WebRequest -Uri "$githubUrl/CustomDocuments.zip" -OutFile "$path/Deployment/CustomDocuments.zip" -ErrorAction SilentlyContinue -Verbose

					#download any custom files from customer
					if ($using:CustomDataZip)
					{
						write-verbose "Downloading Custom files";

						Invoke-WebRequest -Uri $using:CustomDataZip -OutFile "$path/Deployment/CustomDocuments.zip" -ErrorAction SilentlyContinue -Verbose				
					}

					write-verbose "Downloading Support files";
					#supporing files - since DSC deletes all the directories...
					Invoke-WebRequest -Uri "$githubUrl/AzureTemplates.zip" -OutFile "$path/AzureTemplates.zip" -ErrorAction SilentlyContinue -Verbose
					Invoke-WebRequest -Uri "$githubUrl/Configuration.zip" -OutFile "$path/Configuration.zip" -ErrorAction SilentlyContinue -Verbose

					write-verbose "Loading NewtonSoft.Json";
					add-type -Path "$path/Newtonsoft.Json.dll" -ErrorAction SilentlyContinue;

					#start with the default one
					write-verbose "Loading configuration file";
					$json = Get-Content "$path/configuration.development.json" -raw;

					#if they have a custom path, use it...
					if ($using:ConfigFilePath)
					{
						write-verbose "Loading configuration file from [$using:ConfigFilePath]";
                        Invoke-WebRequest -Uri $using:ConfigFilePath -OutFile "$path/configuration.development.json" -ErrorAction SilentlyContinue -Verbose
					}

					write-verbose "Setting properties";
					#override the local environment
					$configuration = [Newtonsoft.Json.JsonConvert]::DeserializeObject($json);
					$configuration.AdminAzureClientId = $using:AdminAzureClientId;
					$configuration.AdminAzureClientSecret = $using:AdminAzureClientSecret;
					$configuration.TenantId = $using:TenantId;
					$configuration.SubscriptionId = $using:SubscriptionId;
					$configuration.Region = $using:Region;
					$configuration.ResourceGroupName = $using:ResourceGroupName;
					$configuration.ResourcePrefix = $using:ResourcePrefix;
					$configuration.SearchServiceApiVersion = $using:SearchServiceApiVersion;
					$configuration.BlobStorageConnectionString = $using:BlobStorageConnectionString;
					$configuration.UseSampleData = $using:UseSampleData;
					$configuration.ConfigFilePath = $using:ConfigFilePath;
					$configuration.CustomDataZip = $using:CustomDataZip;
					$configuration.Username = $using:Username;
					$configuration.Password = $using:Password;
					$configuration.BingEndpoint = $using:BingEndpoint;
					$configuration.BingKey = $using:BingKey;

					write-verbose "Saving properties to production configuration";
					#save the configuration
					$json = [Newtonsoft.Json.JsonConvert]::SerializeObject($configuration);
					remove-item "$path/configuration.production.json" -ea SilentlyContinue
					add-content "$path/configuration.production.json" $json -ea SilentlyContinue;
	
					write-verbose "Running deployment tool";
					#run the deployment...
					& "$path\KnowledgeMiningDeployer.exe"
				}
				catch
				{
					write-verbose "Error occured $($_.Exception.Message)";
					#write-verbose get-pscallstack
				}

				<#
				write-verbose "Killing the DSC process to remove the cache";
				###
				### find the process that is hosting the DSC engine
				###
				$dscProcessID = Get-WmiObject msft_providers |
				Where-Object {$_.provider -like 'dsccore'} |
				Select-Object -ExpandProperty HostProcessIdentifier

				###
				### Stop the process
				###
				Get-Process -Id $dscProcessID | Stop-Process
				#>
            }
            TestScript = { return $false; }
            GetScript = {  }
        }
  }
} 

