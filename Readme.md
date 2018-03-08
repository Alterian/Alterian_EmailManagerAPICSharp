# Alterian Email Manager API

# Overview

Alterian Email Manager is built around a flexible and versatile Web Services API. This API allows anything that can be done in the client software, to be done via external scripting. Functions such as uploading list, creating email creative, triggering email deployments, download campaign response data, can all be realized through the Web Services API.

# Prerequisites

- .Net version 4.5.2 and higher
- Alterian Email Manager user account or access token
- A Client specific Web service URL

# Install Library

You can install Alterian Email Manager API C# client library either [download]() it from GitHub or install it using NuGet package manager
```
PM> Install-Package AlterianEmailManagerAPI
```

# Use Cases
These Email Manager API Code Samples will show steps and basic code to achieve most of basic email manager functions through Email Manager API. 

## Authenticate
```csharp
em.Authenticator.Authenticate(id, pass, newPassword, true, DateTime.Now, out _token);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Create a list
```csharp
em.ListManager.GetColumnMappingAll(em.Token, ref sourceColumns, out defaultKey);
em.ListManager.CreateRecipientList(
                    em.Token,
                    "API Created List " + DateTime.Now.ToString("yyyyMMdd_hhmmss"),
                    "Description Text",
                    309,
                    false,
                    false,
                    ref listFields,
                    null,
                    false
                );
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Upload a list

This sceanrio is to upload large and infrequent list upload. Note: this steps are not thread safe. For the use cases, such as Welcome email or Confirm email, i.e. frequent and small emails, use Send an email or Send multiple emails using predefined email variable mappiings. 

```csharp
em.ListManager.InitializeListImport(em.Token, listId, fieldOrder, false);
em.ListManager.UploadDataCSV(em.Token, ",", null, data);
em.ListManager.InsertListData(em.Token);
em.ListManager.FinalizeListImport(em.Token);
``` 
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Download a list
```csharp
em.ListManager.GetListFields(token, listID, out fieldPrimaryKey);
em.ListManager.GetListRecords(token, listID, 999999, ref dmCursor, DMPreviewDirection.DMPD_FIRST);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Query on the list (e.g. filtering a target)
```csharp
em.ListManager.ExecuteListQuery(em.Token, criteria);
em.ListManager.GetQueryResultsCSV(em.Token, fields, startIndex, endIndex, false, out count);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Create a creative
```csharp
em.EmService.CreateCampaign(em.Token, campaignName, campaignDescription, parentFolderID, false, false, null, null);
em.EmService.BeginCampaignSave(em.Token, campaignID);
em.EmService.QueueCampaignCreativeCreation(em.Token, campaignSaveID, creativeName, description, type, false, false, htmlContent, textContent, true);
em.EmService.CommitCampaignSave(em.Token, campaignSaveID);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Send an email (e.g. Welcome email)

The sample code below is useful for the scenario of Welcome email for an newly regisetered users or Confirm email for a subscription or purchase.  The code below is to send a single email each time. 

```csharp
em.ListManager.GetRecipientByPK(
    token,
    primaryKey,         // EM field id for the PK and it is assumed that the field will contain email address
    value,              // value of the PK, e.g. 123@abc.com
    fieldIDs,           // field ids to retrieve if the recipient already exist
    isCreateNew,        // if true, will create a new record if there is no recipient
    listID              // list needs to be ready in Email Manager
    );
em.ListManager.AddRecipientRecord(
    token,
    listID,
    false, // set to false, if HTML email is preferred
    false, // set to false, if the recipient is not un-subscribed 
    fvList.ToArray()
    );
 em.SendMessage.AddRecipientToDeployment(
    token,
    recipient.RecipientID,
    deploymentID,   // Deployment ID need to be ready to use in Email Manager.
    listID
    );                    
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Send multiple emails using predefined email variable mappiings (e.g Transactional Emails)

The sample code below is useful for the scenario of Welcome email for an newly regisetered users or Confirm email for a subscription or purchase.  The code below is to send a single email or multiple emails at once. When large volume of emails are estimated, emails can be bundled into one API call, so this sample code will be especially useful when a large volume of transactional emails are expected. The process below can be callled concurrently. 

```csharp
em.ListImport.CreateImport(
    Token,
    "ABC Automation",
    importSourceID,
    fileSize,
    arrayOfAddToList,
    arrayOfAddToDeployments
    );
em.ListImport.ImportDataS(Token, importID, chunknumber, dataToImport);
em.ListImport.FinishImport(Token, importID);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Send multiple emails dynamically setting up an email varialbes (e.g Transactional Emails)

The sample code below is useful for the scenario that 1) variable mapping in email creative needs to be done dynamically when sending an email, 2) each emails sent needs to be separated into a deployment in Email Manager, or 3) a list of emails to be sent is relatively large and frequency is low. Note: This sample is not for frequent sends, use Send an email or Send multiple emails using predefined email variable mappiings instead for frequent trigger. 

```csharp
em.GetCreative(creativeName).ID;
em.SendMessage.CreateDeployment(em.Token, creativeId, deployName);
em.SendMessage.SetDeploymentVariables(em.Token, deploymentId, templateValue, recipientLists, null, variableMaps.ToArray(), false);
em.SendMessage.FinalizeDeployment(em.Token, deploymentId, sendNow);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Export response data such as open and click
```csharp
em.DmPlus.ExportEventlogData(em.Token, 0, startDate, endDate, filters.ToArray(), eventColumns.ToArray(), DMExportDataFormat.DMDF_CSV, CompressionMethod.CM_GZIP);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

## Unsubscribe a recipient
```csharp
em.ListManager.UpdateRecipient(em.Token, 0, recipient.RecipientID, recipient.PrefersText, recipient.RSSOnly, Unsubscribed: true, FieldValues: null);
```
You can download full example code from [here](https://github.com/AlterianTechnology/EmailManagerAPICSharp)

# Note

Ths example code here is not production ready. It's more to show the basic steps to follow for each scenarios, please apply necessary exception handling, logging, alerting, or what is needed before using it for production. 