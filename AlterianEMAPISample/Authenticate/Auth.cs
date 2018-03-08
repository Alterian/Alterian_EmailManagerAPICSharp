using System;
using AlterianEMAPIClient;
using AlterianEMAPIClient.DMAuthenticate;
using AlterianEMAPISample.Properties;

namespace AlterianEMAPISample.Authenticate
{
    /// <summary>
    /// Auth class to authenticate to API
    /// </summary>
    internal static class Auth
    {
        private static string _token = "";

        /// <summary>
        /// Login to Email Manager and get token
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pass"></param>
        public static string Login(string id, string pass)
        {
            using (var em = new EMWebServicesClient("", Settings.Default.EndPoint))
            {
                string newPassword = "";
                    
                var LoginResult = em.Authenticator.Authenticate(id, pass, newPassword, true, DateTime.Now, out _token);
 
                switch (LoginResult)
                {
                    case DMLoginResult.DMLR_SUCCESS:
                        return "Login successful.";
                    case DMLoginResult.DMLR_LOGINEXPIRED:
                        break;
                    case DMLoginResult.DMLR_LOGININVALID:
                        return "Invalid id or password";
                    case DMLoginResult.DMLR_LOGINDISABLED:
                        break;
                    case DMLoginResult.DMLR_MAXATTEMPTEXCEEDED:
                        break;
                    case DMLoginResult.DMLR_CLIENTDISABLED:
                        break;
                    case DMLoginResult.DMLR_LOGININUSE:
                        break;
                    case DMLoginResult.DMLR_SYSTEMDISABLED:
                        break;
                    case DMLoginResult.DMLR_NEWPWREQUIRED:
                        break;
                    case DMLoginResult.DMLR_NEWPWINVALID:
                        break;
                    case DMLoginResult.DMLR_UNKNOWN:
                        break;
                    default:
                        return "Login was not successful.";
                }

                return "Login was not successful.";
            }
        }

        /// <summary>
        /// Get token if exist, if not return empty string
        /// </summary>
        /// <returns></returns>
        public static string GetToken()
        {
            if (_token == "")
            {
                Console.WriteLine("Please log in");
                return "";
            }
            else
            {
                return _token;
            }
        }
    }
}
