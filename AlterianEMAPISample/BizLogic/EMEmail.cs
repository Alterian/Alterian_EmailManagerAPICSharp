using System.Collections.Generic;
using AlterianEMAPIClient;
using AlterianEMAPIClient.DMListManager;
using AlterianEMAPIClient.DMSendMessage;
using AlterianEMAPISample.Authenticate;
using AlterianEMAPISample.Properties;
using AlterianEMAPISample.Service;
using ArrayOfInt = AlterianEMAPIClient.DMListImport.ArrayOfInt;
using DMTemplateValue = AlterianEMAPIClient.DMSendMessage.DMTemplateValue;
using DMVariableValue = AlterianEMAPIClient.DMSendMessage.DMVariableValue;

namespace AlterianEMAPISample.BizLogic
{
    /// <summary>
    /// EMEmail class to send emails
    /// </summary>
    internal class EMEmail
    {
        /// <summary>
        /// Send an email to a recipient. e.g. Welcome email
        /// </summary>
        public void SendAnEmail()
        {
            var token = Auth.GetToken();

            using (var em = new EMWebServicesClientExtension(token, Settings.Default.EndPoint))
            {
                int primaryKey = em.GetField("email").ID;
                string value = "john@alterian.com";
                var fieldIDs = new AlterianEMAPIClient.DMListManager.ArrayOfInt();
                bool isCreateNew = true;
                int listID = 1192;
                int deploymentID = 2563;

                // Step #1 Looks up an existing recipient record or creates a new one.
                var recipient = em.ListManager.GetRecipientByPK(
                    token,
                    primaryKey,         // EM field id for the PK and it is assumed that the field will contain email address
                    value,              // value of the PK, e.g. 123@abc.com
                    fieldIDs,           // field ids to retrieve if the recipient already exist
                    isCreateNew,     // if true, will create a new record if there is no recipient
                    listID       // list needs to be ready in Email Manager
                    );
 
                // Step #2 Add recipient data into a list, e.g. First Name, Last Name, if needed.
                List<DMFieldValue> fvList = new List<DMFieldValue>();
                DMFieldValue email = new DMFieldValue
                {
                    FieldID = em.GetField("email").ID,  // field id of PK
                    Value = "john@alterian.com"
                };
                fvList.Add(email);
                DMFieldValue firstName = new DMFieldValue
                {
                    FieldID = em.GetField("firstname").ID, // field id of any. e.g. first name
                    Value = "John"
                };
                fvList.Add(firstName);
 
                em.ListManager.AddRecipientRecord(
                    token,
                    listID,
                    false, // set to false, if HTML email is preferred
                    false, // set to false, if the recipient is not un-subscribed 
                    fvList.ToArray()
                    );
 
                // Step #3 Send email to the recipient
                em.SendMessage.AddRecipientToDeployment(
                    token,
                    recipient.RecipientID,
                    deploymentID,   // Deployment ID need to be ready to use in Email Manager.
                    listID
                    );
            }
        }

        /// <summary>
        /// Send emails to multiple recipients by uploading a recipient list with a given deployment setup.
        /// </summary>
        public void SendMultipleEmailsWithGivenDeployment()
        {
            using (var em = new EMWebServicesClient(Auth.GetToken(), Settings.Default.EndPoint))
            {
                string Token = Auth.GetToken();
 
                // importSourceID, listID, deploymentID is prerequisite and can be set up in EM front-end UI.
                const int importSourceID = 21560;
                const int listID = 1192;
                const int deploymentID = 2563;
 
                // Put data to fit into field order as a CSV format.
                // For a welcome email scenario, it can be a simple email
                // In case high volume is expected, emails can be bundled in one import call.
                string dataToImport = string.Empty;
                dataToImport += "email,firstname";
                dataToImport += "\r\n";
                dataToImport += "john@alterian.com,john";
                dataToImport += "\r\n";
                dataToImport += "jane@alterian.com,jane";
                dataToImport += "\r\n";
 
                // Step 1 - Create import context
                long fileSize = dataToImport.Length;
                var arrayOfAddToList = new ArrayOfInt {listID};
                // To upload data without sending email, don't need to assign deploymentID.
                var arrayOfAddToDeployments = new ArrayOfInt {deploymentID};

                int importID = em.ListImport.CreateImport(
                    Token,
                    "ABC Automation",
                    importSourceID,
                    fileSize,
                    arrayOfAddToList,
                    arrayOfAddToDeployments
                    );
 
                // Step 2 - upload data
                const int chunknumber = 1;
                em.ListImport.ImportDataS(Token, importID, chunknumber, dataToImport);
 
                // Step 3 - Let EM know uploading is done from front-end side so that EM server to start importing from back-end
                em.ListImport.FinishImport(Token, importID);
 
                // Step 4 - Import status check
                // This is optional step to confirm the import process. Use only when import record size is more than a few thousand records and the use case is not for a real time scenario.
                var importStatus = em.ListImport.GetImportStatus(Token, importID);
            }
        }

        /// <summary>
        /// Send emails to multiple recipients with a given deployment setup.
        /// </summary>
        public void SendMultipleEmailsAfterCreatingDeployment()
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                const string creativeName = "Training_Basic";
                int creativeId = em.GetCreative(creativeName).ID;
 
                // Create Deployment
                const string deployName = "";
                int deploymentId = em.SendMessage.CreateDeployment(em.Token, creativeId, deployName);
    
                // Set Variables
                int recipientListId =  1192; // This is the list id that contains the recipients to send email to.
                var recipientLists = new AlterianEMAPIClient.DMSendMessage.ArrayOfInt
                {
                    recipientListId
                };

                DMTemplateValue templateValue = new DMTemplateValue
                {
                    HtmlTemplate = em.GetTemplate(creativeId, "Email 1_HTML").ID
                };

                List<DMVariableMap> variableMaps = new List<DMVariableMap>
                {
                    SetVariableValue(creativeId, "Sender Email", "apac-uat@e.alterian.net"),
                    SetVariableValue(creativeId, "Recipient Email", "", "email"),
                    SetVariableValue(creativeId, "email 2", "", "email")
                };

                em.SendMessage.SetDeploymentVariables(em.Token, deploymentId, templateValue, recipientLists, null, variableMaps.ToArray(), false);
 
                // Execute Deployment
                const bool sendNow = true;
                em.SendMessage.FinalizeDeployment(em.Token, deploymentId, sendNow);
            }
        }

        private DMVariableMap SetVariableValue(int creativeId, string variableName, string variableValue, string fieldName = "")
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                DMVariableMap variableMap = new DMVariableMap
                {
                    VariableID = em.GetCreativeVariable(creativeId, variableName).ID,
                    Value = new DMVariableValue {Value = variableValue},
                    FieldID = 0
                };

                // use field name to map a variable to a Email Manager list field
                if (!string.IsNullOrWhiteSpace(fieldName))
                    variableMap.FieldID = em.GetField(fieldName)?.ID ?? 0;

                return variableMap; 
            }
        }
    }
}
