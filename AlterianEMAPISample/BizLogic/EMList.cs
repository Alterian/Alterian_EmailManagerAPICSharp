using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlterianEMAPIClient;
using AlterianEMAPIClient.DMListManager;
using AlterianEMAPISample.Authenticate;
using AlterianEMAPISample.Properties;
using AlterianEMAPISample.Service;

namespace AlterianEMAPISample.BizLogic
{
    internal class EMList
    {
        /// <summary>
        /// Download the records of a Email Manager list
        /// </summary>
        public void Download()
        {
            var token = Auth.GetToken();

            using (var em = new EMWebServicesClient(token, Settings.Default.EndPoint))
            {
                // Set Variable
                int listID = 1192; // List id can be loaded from app.config, if already know it.
                int fieldPrimaryKey;

                // Get list fields (columns)
                DMListField[] fields = em.ListManager.GetListFields(token, listID, out fieldPrimaryKey);

                // Get list records
                DMCursor dmCursor = new DMCursor();
                DMRecipientRecord[] dmRecords = em.ListManager.GetListRecords(token, listID, 999999, ref dmCursor, DMPreviewDirection.DMPD_FIRST);

                // Write the CSV file with the retrieve data
                WriteToCSVFile(fields, dmRecords);
            }
        }

        /// <summary>
        /// Upload CSV file to an EM list. This process is not thread safe. Use DMListImport for concurrent uploading.
        /// </summary>
        public void Upload(int listId)
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                // Upload data into a list
                string fileNameFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\data.csv");

                // set EM field id order to be identical to CSV file column order
                ArrayOfInt fieldOrder = new ArrayOfInt
                {
                    em.GetField("email").ID,
                    em.GetField("firstname").ID
                };

                em.ListManager.InitializeListImport(em.Token, listId, fieldOrder, false);

                // read and import CSV file
                // a header row is not required in CSV file
                using (var tStream = new StreamReader(fileNameFull))
                {
                    string data = tStream.ReadToEnd();
                    em.ListManager.UploadDataCSV(em.Token, ",", null, data);
                }
                
                em.ListManager.InsertListData(em.Token);
                em.ListManager.FinalizeListImport(em.Token);
            }
        }

        /// <summary>
        /// Query on EM list
        /// </summary>
        public string Query()
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                DMListCriteria criteria = new DMListCriteria();

                // Set lists to query against. The lists needs to share the same primary key.
                int listID = 1192;
                var listIds = new ArrayOfInt
                {
                    listID
                };
                // Add more lists as needed
                // listIds.Add(2);
                // listIds.Add(3);
                criteria.IncludeLists = listIds;

                // Set conditions
                var dmFieldCriteriaList = new List<DMFieldCriteria>();

                // e.g. condition for 'subscribed' recipients
                DMFieldCriteria dmFieldCriteria = new DMFieldCriteria
                {
                    QueryType = DMFieldQueryType.DMQT_ENABLED,
                    Parameter = 1,
                    Operator = DMSQLOperator.DMSQ_EQUAL,
                    Combine = DMCombine.DMCM_AND
                };
                dmFieldCriteriaList.Add(dmFieldCriteria);

                // e.g. condition for 'created' 7 days ago.
                dmFieldCriteria = new DMFieldCriteria
                {
                    QueryType = DMFieldQueryType.DMQT_MODIFIED,
                    Parameter = 0,
                    Operator = DMSQLOperator.DMSQ_GREATERTHAN,
                    Combine = DMCombine.DMCM_NONE,
                    Date1 = DateTime.Now.AddDays(-365 * 10)
                    // dmFieldCriteria.Date2 = DateTime.Today.AddDays(1);  // Not need for now, but will be needed when used with data range option.
                };

                dmFieldCriteriaList.Add(dmFieldCriteria);
                criteria.FieldCriteria = dmFieldCriteriaList.ToArray();

                // Run query
                int countRecipients = em.ListManager.ExecuteListQuery(em.Token, criteria);

                // Set field IDs to download
                var fields = new ArrayOfInt
                {
                    em.GetField("email").ID,
                    em.GetField("firstname").ID
                };

                // Download list
                int startIndex = 0;
                int endIndex = 999999999;
                int count;
                var result = em.ListManager.GetQueryResultsCSV(em.Token, fields, startIndex, endIndex, false, out count);

                return result;
            }
        }

        public int Create()
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                // Prepare fields to add to an EM list.
                DMSourceColumn[] sourceColumns = new DMSourceColumn[2];
                sourceColumns[0] = new DMSourceColumn {Name = "email"};
                sourceColumns[1] = new DMSourceColumn {Name = "firstname"};
                int defaultKey;
                var allColumns = em.ListManager.GetColumnMappingAll(em.Token, ref sourceColumns, out defaultKey);

                // assume that all fields exist in Email Manager
                DMListField[] listFields = new DMListField[2];
                listFields[0] = MapToListField(allColumns.SingleOrDefault(column => column.Name == sourceColumns[0].Name), true);
                listFields[1] = MapToListField(allColumns.SingleOrDefault(column => column.Name == sourceColumns[1].Name), false);

                // Create a list
                int listId = em.ListManager.CreateRecipientList(
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

                return listId;
            }
        }

        private static DMListField MapToListField(DMListField columnEmail, bool isPrimayKey)
        {
            return new DMListField
            {
                ID = columnEmail.ID,
                FieldType = columnEmail.FieldType,
                Name = columnEmail.Name,
                PrimaryKey = isPrimayKey,
                UserAccess = columnEmail.UserAccess,
                AllowDupes = columnEmail.AllowDupes,
                ListField = columnEmail.ListField,
                Created = columnEmail.Created,
                Modified = columnEmail.Modified,
                StorageType = columnEmail.StorageType,
                UserOptOut = columnEmail.UserOptOut
            };
        }

        private void WriteToCSVFile(DMListField[] fields, DMRecipientRecord[] dmRecords)
        {
            // do something
        }
    }
}