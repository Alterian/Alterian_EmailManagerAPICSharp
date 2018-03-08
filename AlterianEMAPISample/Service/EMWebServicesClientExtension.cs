using System.Linq;
using AlterianEMAPIClient;
using AlterianEMAPIClient.DMCreativeBuilder;
using AlterianEMAPIClient.DMListManager;
using AlterianEMAPIClient.DMSendMessage;

namespace AlterianEMAPISample.Service
{
    /// <summary>
    /// Extension class of EMWebServicesClient
    /// </summary>
    public class EMWebServicesClientExtension : EMWebServicesClient
    {
        /// <summary>
        /// Initialize EMWebServicesClientExtension instance
        /// </summary>
        /// <param name="token">Alterian Email Manager API access key</param>
        /// <param name="endpointUrl">Alterian Email Manager API URL e.g. https://nasa.e.alterian.net</param>
        public EMWebServicesClientExtension(string token, string endpointUrl) 
            : base(token, endpointUrl)
        {
        }

        /// <summary>
        /// Get creative by creative name. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DMCreative GetCreative(string name)
        {
            return SendMessage
                .GetAllCreatives(Token)
                .SingleOrDefault(a => a.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Get creative variable by variable name
        /// </summary>
        /// <param name="creativeId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public DMVariable GetCreativeVariable(int creativeId, string name)
        {
            return CreativeBuilder
                .GetCreativeVariables(Token, creativeId)
                .SingleOrDefault(a => a.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Get list field by field name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DMField GetField(string name)
        {
            return ListManager
                .GetFields(Token)
                .SingleOrDefault(a => a.Name == name);
        }

        /// <summary>
        /// Get template by its name in a given creative
        /// </summary>
        /// <param name="creativeId"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public DMTemplate GetTemplate(int creativeId, string templateName)
        {
            return CreativeBuilder
                .GetCreativeTemplates(Token, creativeId)
                .SingleOrDefault(a => a.Name == templateName);
        }
    }
}
