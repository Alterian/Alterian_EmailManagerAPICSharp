using AlterianEMAPIClient.DMListManager;
using AlterianEMAPISample.Authenticate;
using AlterianEMAPISample.Properties;
using AlterianEMAPISample.Service;

namespace AlterianEMAPISample.BizLogic
{
    internal class EMRecipient
    {
        /// <summary>
        /// Unsubscribe a recipient based on primary key
        /// </summary>
        public void Unsubscribe()
        {
            using (var em = new EMWebServicesClientExtension(Auth.GetToken(), Settings.Default.EndPoint))
            {
                int PKfieldId = em.GetField("email").ID;
                string PKvalue = "jane@alterian.com";
                DMRecipientRecord recipient = em.ListManager.GetRecipientByPK(em.Token, PKfieldId, PKvalue, new ArrayOfInt(), false, 0);

                em.ListManager.UpdateRecipient(em.Token, 0, recipient.RecipientID, recipient.PrefersText, recipient.RSSOnly, Unsubscribed: true, FieldValues: null);
            }
        }
    }
}