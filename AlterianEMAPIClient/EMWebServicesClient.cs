using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using AlterianEMAPIClient.DMAuthenticate;
using AlterianEMAPIClient.DMCreativeBuilder;
using AlterianEMAPIClient.DMListImport;
using AlterianEMAPIClient.DMListManager;
using AlterianEMAPIClient.DMPlus;
using AlterianEMAPIClient.DMReporting;
using AlterianEMAPIClient.DMSendMessage;
using AlterianEMAPIClient.EM;

namespace AlterianEMAPIClient
{
    /// <summary>
    ///     DMWebServicesClient encapsulates common functions of an application connecting to the Alterian Email Manager API.
    /// </summary>
    public class EMWebServicesClient : IDisposable
    {
        private readonly SecurityProtocolType _securityType;
        private string _endpointUrl;
        private bool _isKeepAlive;
        private bool _disposed;
        private DMAuthenticateSoapClient _authenticate;
        private DMCreativeBuilderSoapClient _creativeBuilder;
        private DMPlusClient _dmPlus;
        private EMClient _emService;
        private DMListImportSoapClient _listImport;
        private DMListManagerSoapClient _listManager;
        private DMReportingSoapClient _reporting;
        private DMSendMessageSoapClient _sendMessage;

        /// <summary>
        /// Token to access Alterian Email Manager API
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// URL to access Alterian API
        /// </summary>
        private string EndpointUrl
        {
            get { return _endpointUrl; }
            set
            {
                if (value.StartsWith("https"))
                {
                    ServicePointManager.SecurityProtocol = _securityType;
                    ServicePointManager.Expect100Continue = true;
                }

                _endpointUrl = value;
            }
        }

        /// <summary>
        /// Initialize the new instance of EMWebServicesClient
        /// </summary>
        /// <param name="token">Alterian Email Manager API access key</param>
        /// <param name="endpointUrl">Alterian Email Manager API URL e.g. https://nasa.e.alterian.net</param>
        /// <param name="isKeepAlive">If set to true, use KeepAliveEnalbed mode in HttpTransportBinding</param>
        /// <param name="securityType">Security protocol type when connecting through HTTPS. Default is TLS1.2</param>
        public EMWebServicesClient(string token, string endpointUrl, bool isKeepAlive = false, SecurityProtocolType securityType = SecurityProtocolType.Tls12)
        {
            // security type must be assigned before the endpointUrl is assigned.
            _securityType = securityType;
            Token = token;
            EndpointUrl = endpointUrl;
            _isKeepAlive = isKeepAlive;
        }

        /// <summary>
        /// Authenticate SOAP client to get a token for  Alterian Email Manager API.
        /// </summary>
        public DMAuthenticateSoapClient Authenticator => _authenticate ?? (_authenticate = new DMAuthenticateSoapClient(
                                                             GetBinding(5, 5), 
                                                             new EndpointAddress(EndpointUrl + "/authenticate.asmx")));

        /// <summary>
        /// CreativeBuilder SOAP client to manage email creative.
        /// </summary>
        public DMCreativeBuilderSoapClient CreativeBuilder => _creativeBuilder ?? (_creativeBuilder = new DMCreativeBuilderSoapClient(
                                                                  GetBinding(receiveTimeout:10), 
                                                                  new EndpointAddress(EndpointUrl + "/creativebuilder.asmx")));

        /// <summary>
        /// ListImport SOAP client to add data into Alterian Email Manager and send emails.
        /// </summary>
        public DMListImportSoapClient ListImport => _listImport ?? (_listImport = new DMListImportSoapClient(
                                                        GetBinding(30, 10), 
                                                        new EndpointAddress(EndpointUrl + "/ListImport.asmx")));

        /// <summary>
        /// ListManager SOAP client to manage lists in Email Manager.
        /// </summary>
        public DMListManagerSoapClient ListManager => _listManager ?? (_listManager = new DMListManagerSoapClient(
                                                          GetBinding(receiveTimeout:30, sendTimeout:10), 
                                                          new EndpointAddress(EndpointUrl + "/listmanager.asmx")));

        /// <summary>
        /// Reporting SOAP client to get reporting on email response, such as open and click.
        /// </summary>
        public DMReportingSoapClient Reporting => _reporting ?? (_reporting = new DMReportingSoapClient(
                                                      GetBinding(receiveTimeout:30, sendTimeout:10), 
                                                      new EndpointAddress(EndpointUrl + "/reporting.asmx")));

        /// <summary>
        /// SendMessage SOAP client to send emails.
        /// </summary>
        public DMSendMessageSoapClient SendMessage => _sendMessage ?? (_sendMessage = new DMSendMessageSoapClient(
                                                          GetBinding(receiveTimeout:10, sendTimeout:10), 
                                                          new EndpointAddress(EndpointUrl + "/sendmessage.asmx")));

        /// <summary>
        /// EMService SOAP client replacing most Creative Builder API.
        /// </summary>
        public EMClient EmService => _emService ?? (_emService = new EMClient(
                                         GetBinding(10, 3, true), 
                                         new EndpointAddress(EndpointUrl + "/EM.svc/soap")));

        /// <summary>
        /// DMPlus SOAP client to extract email response log data, such as open and click.
        /// </summary>
        public DMPlusClient DmPlus => _dmPlus ?? (_dmPlus = new DMPlusClient(
                                          GetBinding(30, 10, true), 
                                          new EndpointAddress(EndpointUrl + "/DMPlus.svc")));

        private CustomBinding GetBinding(int receiveTimeout, int sendTimeout = 3, bool mtom = false)
        {
            // set binding
            var customBinding = new CustomBinding
            {
                ReceiveTimeout = new TimeSpan(0, receiveTimeout, 0),
                SendTimeout = new TimeSpan(0, sendTimeout, 0)
            };

            if (mtom)
                customBinding.Elements.Add(new MtomMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8));
            else
                customBinding.Elements.Add(new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8));

            HttpTransportBindingElement bindingElement;

            // Security mode
            if (_endpointUrl.StartsWith("https"))
                bindingElement = new HttpsTransportBindingElement();
            else
                bindingElement = new HttpTransportBindingElement();

            bindingElement.KeepAliveEnabled = _isKeepAlive;
            bindingElement.MaxReceivedMessageSize = int.MaxValue;

            customBinding.Elements.Add(bindingElement);

            return customBinding;
        }

        /// <summary>
        /// Dispose the resource in this class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other manage object here.
            }

            // Free any unmanaged object here.
            if (_authenticate != null)
            {
                try
                {
                    _authenticate.Close();
                }
                catch (Exception)
                {
                    _authenticate.Abort();
                    throw;
                }

                _authenticate = null;
            }

            if (_creativeBuilder != null)
            {
                try
                {
                    _creativeBuilder.Close();
                }
                catch (Exception)
                {
                    _creativeBuilder.Abort();
                    throw;
                }

                _creativeBuilder = null;
            }

            if (_dmPlus != null)
            {
                try
                {
                    _dmPlus.Close();
                }
                catch (Exception)
                {
                    _dmPlus.Abort();
                    throw;
                }
                _dmPlus = null;
            }

            if (_emService != null)
            {
                try
                {
                    _emService.Close();
                }
                catch (Exception)
                {
                    _emService.Abort();
                    throw;
                }
                _emService = null;
            }

            if (_listImport != null)
            {
                try
                {
                    _listImport.Close();
                }
                catch (Exception)
                {
                    _listImport.Abort();
                    throw;
                }
                _listImport = null;
            }

            if (_listManager != null)
            {
                try
                {
                    _listManager.Close();
                }
                catch (Exception)
                {
                    _listManager.Abort();
                    throw;
                }
                _listManager = null;
            }

            if (_reporting != null)
            {
                try
                {
                    _reporting.Close();
                }
                catch (Exception)
                {
                    _reporting.Abort();
                    throw;
                }
                _reporting = null;
            }

            if (_sendMessage != null)
            {
                try
                {
                    _sendMessage.Close();
                }
                catch (Exception)
                {
                    _sendMessage.Abort();
                    throw;
                }
                _sendMessage = null;
            }

            _disposed = true;
        }

        /// <summary>
        /// Destructor of the class
        /// </summary>
        ~EMWebServicesClient()
        {
            Dispose(false);
        }
    }
}
