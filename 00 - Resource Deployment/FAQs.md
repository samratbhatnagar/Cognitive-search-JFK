# Frequently Asked Questions

##  What are the levels of configuration / deployment?

You can create the following items via the configuration file.  The remaining items are done via ARM template deployment or via Fluent API code.

-  Indexes
-  Skillsets
-  Data sources

##	Cognitive Services Account is created? Why is isn’t in the config file or ask for key?

The original design of this deployment tool was to provide a one-click experience.  Therefore the resources that are needed to support the end state are deployed and automatically configured via code.

##	Not clear how to change from prod to test and dev config files. When you choose one, the others will be ignored? What is the difference between them?

You can modify the configuration file that is used by changing the app.config file for the deployment tool aka the KnowledgeMiningDeployment.exe.config file.

##	Why are the cosmos and SQL data sources commented out and/or empty?

The intent of the tool was to show the flexiablity of Azure Search with cognitive services.  This would include the ability to deploy CosmoDB and SQL server resources and the respective data that would be in them.  However, this also means we would have to create some kind of layer of commonality to allow Cosmos and Azure SQL to be indexed and that in of itself would be a project.

##	How do I add more data sources to import?

Data sources can be added through the Azure Seach Portal, or via the configuration file in the deployment tool.

##  How do I add more data after deployment?

There are two ways to add more data to a knowledge mining deployment:

-  Add data to your data source and re-index manual via the Azure Search Portal blade OR
-  Utilize the Web UI to add more data to the backend storage account and then have the Web UI automatically execute a re-index for you

##	What is the minimum Azure Search API version required?

The API calls must be 2019-05-06-Preview or later.

##  My data is already in a blob storage account, how do I add that?

Utilize the configuration file to add more blob storage account data sources to the deployment using your account name and key(s).

##	What is the VM used for and can it be deleted?

The VM is used for the one-click deployment via DSC scripting.  Once the deployment is successful, you can safely delete the VM.  Note however that it will return if you re-run the deployment ARM template.

##	Operation returned an invalid status code 'Unauthorized'. What could the issue be?

In most cases, you likely typed or copied the wrong key for an Azure resource (such as CosmosDB, SQL Server, or Azure Storage).

##	Do I need to add a connection string for the knowledgestore? Or will the deployment create it?

If you type an improper connection string, the deployment tool will test the connection string before executing the indexer creation.  It will fallback to the storage account that is created as part of the ARM template deployment.
