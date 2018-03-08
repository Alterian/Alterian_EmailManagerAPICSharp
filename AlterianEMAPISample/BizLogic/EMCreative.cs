using System;
using System.IO;
using System.Linq;
using AlterianEMAPIClient;
using AlterianEMAPIClient.EM;
using AlterianEMAPISample.Authenticate;
using AlterianEMAPISample.Properties;

namespace AlterianEMAPISample.BizLogic
{
    /// <summary>
    /// EMCreative class to create a creative
    /// </summary>
    internal class EMCreative
    {
        /// <summary>
        /// Create email creative with HTML and TEXT template
        /// </summary>
        public void Create()
        {
            using (var em = new EMWebServicesClient(Auth.GetToken(), Settings.Default.EndPoint))
            {
                //Find or create a folder called "Test" to store the campaign.
                var folders = em.EmService.GetFolders(em.Token).ToList();
                int parentFolderID;
                if (folders.Exists(f => f.Name.Equals("Test")))
                {
                    parentFolderID = folders.Find(f => f.Name.Equals("Test")).ID;
                }
                else
                {
                    parentFolderID = em.EmService.CreateFolder(em.Token, 0, "Test");
                }
 
                //Create blank campaign in specified folder
                string campaignName = "API Demo Test Campaign";
                string campaignDescription = "This is test campaign";

                // Create campaign if not exist.
                int campaignID = em.EmService.GetDocumentsByName(em.Token, campaignName).FirstOrDefault()?.ID ?? 0;
                if (campaignID == 0)
                    campaignID = em.EmService.CreateCampaign(em.Token, campaignName, campaignDescription, parentFolderID,
                    false, false, null, null);
 
                //Wait until the campaign can be saved - i.e. if it is currently being edited
                bool canSave = false;
                while (!canSave)
                {
                    canSave = em.EmService.CanSaveCampaign(em.Token, campaignID);
                }
 
                //Begin edit of the campaign
                int campaignSaveID = em.EmService.BeginCampaignSave(em.Token, campaignID);
 
                //Create a simple email content, from HTML and text stored in a file
                String creativeName = "Email";
                String description = "";
                EMDocumentType type = EMDocumentType.EMDT_Email;
 
                EMContent htmlContent = new EMContent
                {
                    Content = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data\\template.html")),
                    Encoding = "",
                    Type = EMContentType.EMCT_HTML
                };
 
                EMContent textContent = new EMContent
                {
                    Content = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data\\template.txt")),
                    Encoding = "",
                    Type = EMContentType.EMCT_Text
                };
 
                em.EmService.QueueCampaignCreativeCreation(em.Token, campaignSaveID, creativeName,
                    description, type, false, false, htmlContent, textContent, true);
 
                em.EmService.CommitCampaignSave(em.Token, campaignSaveID);
            }
        }
    }
}
