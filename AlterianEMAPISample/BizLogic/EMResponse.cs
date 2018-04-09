using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using AlterianEMAPIClient.DMPlus;
using AlterianEMAPISample.Authenticate;
using AlterianEMAPISample.Properties;
using AlterianEMAPISample.Service;

namespace AlterianEMAPISample.BizLogic
{
    internal class EMResponse
    {
        /// <summary>
        /// Export Email Manager response data
        /// </summary>
        public void Export()
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                //date range
                DateTime startDate = DateTime.Now.AddDays(-30);
                DateTime endDate = DateTime.Now;
 
                // define export filters
                List<DMEventExportFilter> filters = new List<DMEventExportFilter>();
 
                // event id can be found at http://em.help.alterian.com/#Dynamic_Reporting/Actions.htm?Highlight=Actions
                // for example, 1: submitted, 2: Email Open, 54: Any Click, 55: Any Bounce, 56: Any Unsubscribe
                filters.Add(GetFilter("1,2,54,55,56", DMEventExportFilterType.DMEF_INCLUDE_EVENTS));
 
                // Explicitly specify deployments to include only these deployments in event extraction, if needed
                // filters.Add(GetFilter("2563,2569", DMEventExportFilterType.DMEF_INCLUDE_DEPLOYMENTS));

                // Explicitly specify deployments to exclude in event extraction, if needed.
                filters.Add(GetFilter("100,200", DMEventExportFilterType.DMEF_EXCLUDE_DEPLOYMENTS));
 
                // define export columns
                var eventColumns = GetEventColumns();
 
                // create export
                int count;
                int pageSize = 1000; // keep this below 5,000 is the best practice
                var exportId = em.DmPlus.CreateExportByDate(em.Token, pageSize, startDate, endDate, filters.ToArray(), out count);

                // write pages of export to file
                var pageNumber = 1;
                const int maxRetryCount = 5;
                var retryAttempt = 0;
                var fileOutputLocation = $"Export{DateTime.Now:yyyyMMdd_hhmmss}.csv";
                while (true)
                {
                    try
                    {
                        // read response data
                        var exportData = em.DmPlus.ExportEventlogDataByPage(em.Token, exportId, pageNumber, startDate, endDate, filters.ToArray(), eventColumns.ToArray(), DMExportDataFormat.DMDF_CSV, CompressionMethod.CM_GZIP);
                        
                        // write stream to file
                        var gZipStream = new GZipStream(exportData, CompressionMode.Decompress, false);
                        using (var exportReader = new StreamReader(gZipStream))
                        {
                            using (var writer = File.AppendText(fileOutputLocation))
                            {
                                var content = exportReader.ReadToEnd();
                                if (content.Length == 0)
                                    break;
                                writer.Write(content);
                            }
                        }

                        pageNumber++;
                    }
                    catch (Exception e)
                    {
                        if (retryAttempt < maxRetryCount)
                        {
                            retryAttempt++;
                            Console.WriteLine("Failed on chunk {0} - retry number {1}, will retry after a pause. Error: {2}", pageNumber, retryAttempt, e);
                            Thread.Sleep(1000 * retryAttempt);
                        }
                        else
                        {
                            Console.WriteLine("Failed on chunk {0}, no more retry attempts. Error: {1}", pageNumber, e);
                            throw;
                        }
                    }
                }

                em.DmPlus.DeleteExport(em.Token, exportId);
            }
        }

        /// define event types to export
        private DMEventExportFilter GetFilter(string idString, DMEventExportFilterType filterType)
        {
            DMEventExportFilter filter = new DMEventExportFilter();
 
            filter.FilterType = filterType;
            string[] ids = idString.Split(new char[] { ',' });
            filter.Values = new int[ids.Length];
 
            for (int i = 0; i < ids.Length; i++)
                filter.Values[i] = Convert.ToInt32(ids[i]);
 
            return filter;
        }
 
        /// define columns to export
        private List<DMEventColumn> GetEventColumns()
        {
            var eventColumns = new List<DMEventColumn>();
            DMEventColumn column = new DMEventColumn();
            column.ColumnType = DMEventColumnType.DMEC_EVENT_ID;
            eventColumns.Add(column);
 
            column = new DMEventColumn();
            column.ColumnType = DMEventColumnType.DMEC_EVENT_TIME;
            eventColumns.Add(column);
 
            column = new DMEventColumn();
            column.ColumnType = DMEventColumnType.DMEC_EVENT_NAME;
            eventColumns.Add(column);
     
            column = new DMEventColumn();
            column.ColumnType = DMEventColumnType.DMEC_DEPLOYMENT_ID;
            eventColumns.Add(column);
 
            column = new DMEventColumn();
            column.ColumnType = DMEventColumnType.DMEC_FIELD_VALUE;
            column.FieldID = 1;   // field id to include in report
            column.ListID = 1192;    // list id of the field id is belong to
            eventColumns.Add(column);
 
            column = new DMEventColumn();
            column.ColumnType = DMEventColumnType.DMEC_FIELD_VALUE;
            column.FieldID = 2;   // field id to include in report
            column.ListID = 1192;    // list id of the field id is belong to
            eventColumns.Add(column);
 
            // more columns can be added here
 
            return eventColumns;
        }
    }
}
