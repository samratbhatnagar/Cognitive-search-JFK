# Azure Search Cogs Deployment (Generic JFK Demo)

##  Welcome to the Cognitive Services Knowledge Mining Deployment tool.  

You can use this tool to deploy all the items necessary to have an Azure Resource Group with all the Azure resources necessary to implement the Microsoft JFK Demo using custom data sources and data as well as a fully customizable UI portal for POC demostrations.

##  Before you Begin

Most of the items are deployed via the main.json ARM template.  This template will create many of the resources needed, however higher level functionality and deployment must be done via Script/EXE.

You will need the following items before you can utilize this deployment tool:

-  Paid Azure Subscription 
-  Azure Application Client Id and Secret with access to your subscription to create a resource group and all the necessary items
-  Bing Instance (for the Bing Skill)

##  Configuration

Everything is driven by the configuration.{mode}.json file.  This file contains all the settings the EXE will need to deploy the various resources to the items created in the ARM template.

The {mode} paramter is driven by the app.config (KnowledgeMiningDeployer.exe.config) and is default set to "production".  Therefore the tool will look for the **configuration.production.json** file in the same directory.

Everything the tool needs to do the deployment is inside the configuration file.  You will need the following items added to the configuration file based on the following major sections:

-   Configuration variables
-   Azure Search Skillsets
-   Data Sources

###  Configuration variables

```json
"ResourceGroupName": "jfk-cjg",  //name of resource group you want to deploy too
"ResourcePrefix": "jfk",  //a prefix added to all resources created
"SubscriptionId": "GUID",  //the azure subscription id
"AdminAzureClientId": "GUID", //the azure client id of the azure application
"AdminAzureClientSecret": "",  //the azure client secret of the azure application
"TenantId": "GUID",  //the AAD tenantid
"Region": "westus2",  //the region you want to deploy items too
"SearchServiceApiVersion": "2019-05-06-Preview",  //the search service application version
"DeployMain": false,  //whether to deploy the main ARM template
"DoDeployments": false,  //whether to deploy the Web, API and function zip files
"UseSampleData": "true", //whether to import the sample data
"BlobStorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=blah;AccountKey=blah;EndpointSuffix=core.windows.net",  //the storage account you want to point to for indexing
"StorageContainer": "documents",  //the container in the storage account to index
"ConfigFilePath": "https://raw.githubusercontent.com/solliancenet/Cognitive-search-JFK/knowledge-mining/00%20-%20Resource%20Deployment/KnowledgeMiningDeployer/KnowledgeMiningDeployer/configuration.development.json",//an external config file you might want to reference when running from ARM deployment
"CustomDataZip": "https://raw.githubusercontent.com/solliancenet/Cognitive-search-JFK/knowledge-mining/00%20-%20Resource%20Deployment/KnowledgeMiningDeployer/KnowledgeMiningDeployer/Deployment/CustomDocuments.zip", //your custom data set your want to pre-populate in your storage account and search index
"Username": "s2admin", //the username of the deployment VM and SQL server
"Password": "Password.1!!",  //the password of the deployment VM and SQL server
```

Follow these steps to create an Azure Client Id and Secret with the proper permissions:

1.  Open the [Azure Portal](https://portal.azure.com)
2.  Open the Azure Active Directory blad
3.  Click **App registrations**
4.  Click **+New registration**
5.  Type a name, then click **Register**
6.  In the azure portal, select **Subscriptions**
7.  Select your target subscription
8.  In the subscription blade, select **Access control (IAM)**
9.  Click **+Add**, then select **Add role assignment**
10.  Select the **owner** role
11.  Search for your new Azure Ad Application, select it, then click **Save**

### Azure Indexes and Indexers

These indexes will be built using the json configuration files for each of the respective nodes defined:

```json
"Indexes": [
    {
      "name": "base",
      "configuration": "base-index"
    },
    {
      "name": "extended",
      "configuration": "base-index"
    }
  ],
  "Indexers": [
    {
      "name": "base-indexer",
      "configuration": "base-indexer"
    },
    {
      "name": "extended-indexer",
      "configuration": "base-indexer"
    }
  ],
```

###  Azure Search Skillsets

These are the basic skills that are setup when you go through the Azure Portal UI to create a full cognitive skill set with knowledge store.

```json
"SkillSets": [
    {
      "name": "base",
      "description": "All Cognitive Skills",
      "cognitiveServices": {
      },
      "knowledgeStore": {
        "storageConnectionString": "DefaultEndpointsProtocol=https;AccountName=jfkstorage;AccountKey=UBmbvK7QjS4yVVzmc4it0nEmzGVyS1ksqt+JX8Mo4cID3wAQVa/7PVCviPwtz/JXGJAUEbIHdSVDUYRdGhGzlQ==;EndpointSuffix=core.windows.net",
        "projections": [
          {
            "tables": [
              {
                "tableName": "Document",
                "generatedKeyName": "SomeId",
                "source": "/document/content"
              },
              {
                "tableName": "KeyPhrases",
                "generatedKeyName": "KeyPhraseId",
                "source": "/document/keyphrases"
              },
              {
                "tableName": "Entities",
                "generatedKeyName": "EntityId",
                "source": "/document/entities"
              }
            ]
          }
        ]
      },
      "skills": [
        {
          "@odata.type": "#Microsoft.Skills.Text.EntityRecognitionSkill",
          "name": "EntityRecognitionSkill",
          "description": null,
          "context": "/document/merged_content",
          "categories": [ "Person", "Quantity", "Organization", "URL", "Email", "Location", "DateTime" ],
          "defaultLanguageCode": "en",
          "minimumPrecision": null,
          "includeTypelessEntities": null,
          "inputs": [
            {
              "name": "text",
              "source": "/document/merged_content",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "languageCode",
              "source": "/document/language",
              "sourceContext": null,
              "inputs": []
            }
          ],
          "outputs": [
            {
              "name": "persons",
              "targetName": "people"
            },
            {
              "name": "organizations",
              "targetName": "organizations"
            },
            {
              "name": "locations",
              "targetName": "locations"
            },
            {
              "name": "entities",
              "targetName": "entities"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Text.KeyPhraseExtractionSkill",
          "name": "KeyPhraseExtractionSkill",
          "description": null,
          "context": "/document/merged_content",
          "defaultLanguageCode": "en",
          "maxKeyPhraseCount": null,
          "inputs": [
            {
              "name": "text",
              "source": "/document/merged_content",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "languageCode",
              "source": "/document/language",
              "sourceContext": null,
              "inputs": []
            }
          ],
          "outputs": [
            {
              "name": "keyPhrases",
              "targetName": "keyphrases"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Text.LanguageDetectionSkill",
          "name": "LanguageDetectionSkill",
          "description": null,
          "context": "/document",
          "inputs": [
            {
              "name": "text",
              "source": "/document/merged_content",
              "sourceContext": null,
              "inputs": []
            }
          ],
          "outputs": [
            {
              "name": "languageCode",
              "targetName": "language"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Text.MergeSkill",
          "name": "MergeSkill",
          "description": "Create merged_text, which includes all the textual representation of each image inserted at the right location in the content field.",
          "context": "/document",
          "insertPreTag": " ",
          "insertPostTag": " ",
          "inputs": [
            {
              "name": "text",
              "source": "/document/content"
            },
            {
              "name": "itemsToInsert",
              "source": "/document/normalized_images/*/text"
            },
            {
              "name": "offsets",
              "source": "/document/normalized_images/*/contentOffset"
            }
          ],
          "outputs": [
            {
              "name": "mergedText",
              "targetName": "merged_content"
            }
          ]
        },
        {
          "description": "Extract text (plain and structured) from image.",
          "name": "OcrSkill",
          "@odata.type": "#Microsoft.Skills.Vision.OcrSkill",
          "context": "/document/normalized_images/*",
          "textExtractionAlgorithm": "printed",
          "lineEnding": "Space",
          "defaultLanguageCode": "en",
          "detectOrientation": true,
          "inputs": [
            {
              "name": "image",
              "source": "/document/normalized_images/*"
            }
          ],
          "outputs": [
            {
              "name": "text",
              "targetName": "text"
            },
            {
              "name": "layoutText",
              "targetName": "layoutText"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Util.ShaperSkill",
          "name": "#6",
          "description": null,
          "context": "/document/normalized_images/*",
          "inputs": [
            {
              "name": "image",
              "source": null,
              "sourceContext": "/document/normalized_images/*",
              "inputs": [
                {
                  "name": "tags",
                  "source": "/document/normalized_images/*/imageTags",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "description",
                  "source": "/document/normalized_images/*/imageCaption",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "celebrities",
                  "source": "/document/normalized_images/*/imageCelebrities/*/detail/celebrities/*/name",
                  "sourceContext": null,
                  "inputs": []
                }
              ]
            }
          ],
          "outputs": [
            {
              "name": "output",
              "targetName": "imageprojection"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Vision.ImageAnalysisSkill",
          "name": "#7",
          "description": null,
          "context": "/document/normalized_images/*",
          "defaultLanguageCode": "en",
          "visualFeatures": [
            "tags",
            "description"
          ],
          "details": [
            "celebrities"
          ],
          "inputs": [
            {
              "name": "image",
              "source": "/document/normalized_images/*",
              "sourceContext": null,
              "inputs": []
            }
          ],
          "outputs": [
            {
              "name": "tags",
              "targetName": "imageTags"
            },
            {
              "name": "description",
              "targetName": "imageCaption"
            },
            {
              "name": "categories",
              "targetName": "imageCelebrities"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Util.ShaperSkill",
          "name": "#8",
          "description": null,
          "context": "/document",
          "inputs": [
            {
              "name": "metadata_storage_content_type",
              "source": "/document/metadata_storage_content_type",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_size",
              "source": "/document/metadata_storage_size",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_last_modified",
              "source": "/document/metadata_storage_last_modified",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_content_md5",
              "source": "/document/metadata_storage_content_md5",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_name",
              "source": "/document/metadata_storage_name",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_path",
              "source": "/document/metadata_storage_path",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_content_type",
              "source": "/document/metadata_content_type",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_language",
              "source": "/document/metadata_language",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "merged_content",
              "source": "/document/merged_content",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "KeyPhrases",
              "source": "/document/merged_content/keyphrases/*",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "LanguageCode",
              "source": "/document/language",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "entities",
              "source": null,
              "sourceContext": "/document/merged_content",
              "inputs": [
                {
                  "name": "People",
                  "source": "/document/merged_content/people/*",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "Organizations",
                  "source": "/document/merged_content/organizations/*",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "Locations",
                  "source": "/document/merged_content/locations/*",
                  "sourceContext": null,
                  "inputs": []
                }
              ]
            },
            {
              "name": "Images",
              "source": null,
              "sourceContext": "/document/normalized_images/*",
              "inputs": [
                {
                  "name": "Images",
                  "source": "/document/normalized_images/*/imageprojection/image",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "layoutText",
                  "source": "/document/normalized_images/*/layoutText",
                  "sourceContext": null,
                  "inputs": []
                }
              ]
            }
          ],
          "outputs": [
            {
              "name": "output",
              "targetName": "objectprojection"
            }
          ]
        },
        {
          "@odata.type": "#Microsoft.Skills.Util.ShaperSkill",
          "name": "#9",
          "description": null,
          "context": "/document",
          "inputs": [
            {
              "name": "metadata_storage_content_type",
              "source": "/document/metadata_storage_content_type",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_size",
              "source": "/document/metadata_storage_size",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_last_modified",
              "source": "/document/metadata_storage_last_modified",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_content_md5",
              "source": "/document/metadata_storage_content_md5",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_name",
              "source": "/document/metadata_storage_name",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_storage_path",
              "source": "/document/metadata_storage_path",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_content_type",
              "source": "/document/metadata_content_type",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "metadata_language",
              "source": "/document/metadata_language",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "merged_content",
              "source": "/document/merged_content",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "KeyPhrases",
              "source": "/document/merged_content/keyphrases/*",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "LanguageCode",
              "source": "/document/language",
              "sourceContext": null,
              "inputs": []
            },
            {
              "name": "entities",
              "source": null,
              "sourceContext": "/document/merged_content",
              "inputs": [
                {
                  "name": "People",
                  "source": "/document/merged_content/people/*",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "Organizations",
                  "source": "/document/merged_content/organizations/*",
                  "sourceContext": null,
                  "inputs": []
                },
                {
                  "name": "Locations",
                  "source": "/document/merged_content/locations/*",
                  "sourceContext": null,
                  "inputs": []
                }
              ]
            },
            {
              "name": "Images",
              "source": "/document/normalized_images/*/imageprojection/image",
              "sourceContext": null,
              "inputs": []
            }
          ],
          "outputs": [
            {
              "name": "output",
              "targetName": "tableprojection"
            }
          ]
        }
      ]
    }
  ]
```

###  Data sources to import

You can create as many data sources as you want.  You simply set the type (cosmosbd, sqlserver, blob or table), the connection parameters and what index and indexer should be used.  This will allow you to point to many custom data sources and create the corresponding index and indexer in the deployment(s).

```json
"datasources": [
    {
      "indexer": "base-indexer-cosmos",
      "targetIndex": "base",
      "type": "cosmosdb",
      "name": "jfk-cosmosdb",
      "connectionString": "https://jfk-cosmosdb.documents.azure.com:443/",
      "containerId": "documents",
      "databaseId": "documents"
    },
    {
      "indexer": "base-indexer",
      "targetIndex": "base",
      "type": "sqlserver",
      "name": "jfk-sql",
      "database": "jfk-db",
      "tables": [ "documents", "tables" ]
    },
    {
      "indexer": "base-indexer",
      "targetIndex": "base",
      "type": "blob",
      "name": "blob",
      "AccountName": "blah",
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=blah;AccountKey=blah",
      "ContainerName": "documents",
      "SasToken": "?sv=2018-03-28&ss=bfqt&srt=sco&sp=rwdlacup&se=2020-06-27T04:32:21Z&st=2019-06-26T20:32:21Z&spr=https&sig=Kqv29O0G9rOWOBcE0g6h8wVjGhuacdhD2Qb%2BxdoqQCY%3D"
    },
    {
      "indexer": "base-indexer",
      "targetIndex": "base",
      "type": "table",
      "name": "table",
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=blah;AccountKey=blah",
      "containers": [ "documents" ]
    }
  ]

```

##  Installation

Once you have setup the configuration.{mode}.json file, you will execute the KnowledgeMiningDeployer.exe tool.  You can review the source code for this tool via this repo.

This tool will do the following:

-  Load the Configuration into a configuration object
-  Initialize the AzureHelper instance (a class that utlizes the Fluent APIs for resource creation)
-  Review the Azure instance and correpsonding resource group and build a set of configuration values to be utilize through out the program
-  Extracts various zip files if certain folders do not exist (this is only important during the one-click DSC Arm template deployment)
-  Upload sample and custom data to the targeted resource group storage account
-  Add a Azure SQL Server and database that has a predefined structure for Azure Search indexing (Partial)
-  Creates and configures the UI, API and Function apps
-  Future:  Creates and trains the ML skills
-  Creates the basic search index
-  Creates data sources based on the resources in the resource group
-  Creates a data source based on the blob storage connection string passed in
-  Creates all data sources in the configuration file
-  Deploys all the skills to the Azure Search instance
-  Future:  Sets up the form recognition skill (deploy, train)
-  Future:  Deploys the Power BI report to a personal workspace

##  Deploy PowerBI Report

A PowerBI report has been created that allows you to view the Cognitive Services skills enrichments.  These enrichments are sent to Azure Table Storage via the KnowledgeStore feature.  You simply need to download the PowerBI report, modify the data source and then refresh the data sources.  You will then be able to see all the enhancements your skill pipeline has added to your Azure Search index.

###  Modify the data source

-  Open PowerBI
-  Click **Recent Sources**, select **jfkstorage**
-  Right-click the **jfkstorage[1]** item, select **Edit**
-  In the ribbon, click **Data source settings**
-  Select the data source, then click **Change source**
-  Enter the name of the storage account
-  Click **OK**
-  Click **Edit permissions**
-  Click **Edit**
-  Update the account key with the primary or secondary key
-  Click **Save**
-  Close all windows and refresh your data sources