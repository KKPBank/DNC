using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Script.Serialization;
using BatchSelectService.Message;
using BatchSelectService.Message.SMG;
using BatchSelectService.Common;
using log4net;
using RestSharp;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Reflection;

namespace BatchSelectService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DoNotCallBatchSelectService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select DoNotCallBatchSelectService.svc or DoNotCallBatchSelectService.svc.cs at the Solution Explorer and start debugging.
    public class DoNotCallBatchSelectService : IDoNotCallBatchSelectService
    {
        private readonly ILog _logger;
        public DoNotCallBatchSelectService()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                // Set logfile name and application name variables
                GlobalContext.Properties["ApplicationCode"] = "DoNotCallBatchSelectService";
                GlobalContext.Properties["ServerName"] = Environment.MachineName;
                _logger = LogManager.GetLogger(typeof(DoNotCallBatchSelectService));
            }
            catch (Exception ex)
            {
                _logger.Error("Exception occur:\n", ex);
            }
        }

        public DoNotCallBatchSelectServiceResponse ExecuteBatchSelectService()
        {
            ThreadContext.Properties["EventClass"] = GetCurrentMethod(1);
            ThreadContext.Properties["RemoteAddress"] = GetClientIP();

            _logger.Debug("-- Execute Batch --:--Execute ExecuteBatchSelectService--");
            DoNotCallBatchSelectServiceResponse ret = new DoNotCallBatchSelectServiceResponse();
            try
            {
                ret.SMGStatus = CallSMGSelectService();
                ret.SLMStatus = CallSLMSelectService();
            }
            catch (Exception ex)
            {
                _logger.Error("Exception occur:\n", ex);
            }

            return ret;
        }
        private SMGResponseStatus CallSMGSelectService()
        {
            _logger.Info("I:--START--:--CallSMGSelectService--");
            SMGResponseStatus ret = new SMGResponseStatus();

            string endpoint = ConfigurationManager.AppSettings["SMGWebServiceURL"].ToString(); // "http://10.202.104.21/BPSJobTrigger/Rest/BPSJobTriggerService.svc/TriggerJob ";
            var client = new RestClient(endpoint);
            var request = new RestRequest(Method.POST);
            var req = new SMGRequest();

            // Set Parameters here
            req.jobCode = ConfigurationManager.AppSettings["SMGJobCode"].ToString(); // "SMGDNC001";
            req.Params = new SMGParam[] { new SMGParam() { key = "value", value = DateTime.Now.ToString("dd/MM/yyyy", new System.Globalization.CultureInfo("en-US")) } };

            string content = "";
            var jsonReq = JsonConvert.SerializeObject(req, new JsonSerializerSettings { ContractResolver = CustomDataContractResolver.Instance }); //request.JsonSerializer.Serialize(req);
            request.AddParameter("application/json; charset=utf-8", jsonReq, ParameterType.RequestBody);
            _logger.DebugFormat("-- XMLRequest --\n{0}", request.XmlNamespace);

            var response = client.Execute(request);
            System.Net.HttpStatusCode statusCode = response.StatusCode;
            int numericStatusCode = (int)statusCode;

            if (numericStatusCode == 200)
            {
                content = response.Content;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                SMGResponse wsResponse = json_serializer.Deserialize<SMGResponse>(content);
                //case call success
                //Do something
                //Console.WriteLine("Response Code : " + wsResponse.responseStatus.responseCode);
                //Console.WriteLine("Response Message : " + wsResponse.responseStatus.responseMsg);
                ret = wsResponse.responseStatus;
                _logger.Debug("I:--SUCCESS--:--CallSMGSelectService--");
            }
            else
            {
                //Do something
                //Console.WriteLine("Fail");
                ret.responseCode = response.StatusCode.ToString();
                ret.responseMsg = response.ErrorMessage;
                _logger.ErrorFormat("O:--FAILED--:Error Message/{0}", response.StatusCode + " " + response.ErrorMessage);
            }

            return ret;
        }

        private SLMMasterService.ResponseStatus CallSLMSelectService() {
            _logger.Info("I:--START--:--CallSLMSelectService--");

            SLMMasterService.ResponseStatus ret = new SLMMasterService.ResponseStatus();
            try
            {
                SLMMasterService.Header header = new SLMMasterService.Header()
                {
                    SystemCode = ConfigurationManager.AppSettings["SLMHeader_SystemCode"].ToString(),
                    ServiceName = ConfigurationManager.AppSettings["SLMHeader_ServiceName"].ToString(),
                    Username = ConfigurationManager.AppSettings["SLMHeader_Username"].ToString(),
                    Password = ConfigurationManager.AppSettings["SLMHeader_Password"].ToString(),
                    TransactionDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TransactionID = DateTime.Now.ToString("yyyyMMddHHmmssfff")
                };

                SLMMasterService.CallSlmBatchRequestBody body = new SLMMasterService.CallSlmBatchRequestBody()
                {
                    BatchCode = ConfigurationManager.AppSettings["SLMBody_BatchCode"].ToString()
                };
                SLMMasterService.MasterServiceClient client = new SLMMasterService.MasterServiceClient();
                ret = client.CallSlmBatch(ref header, body);

                if (ret.ResponseCode != "OBT_I_1000")
                {
                    _logger.ErrorFormat("O:--FAILED--:Error Message/{0}", ret.ResponseCode + " " + ret.ResponseMessage);
                }
                else {
                    _logger.Debug("I:--SUCCESS--:--CallSLMSelectService--");
                }
                client.Close();
            }
            catch (Exception ex)
            {
                _logger.Error("Exception occur:\n", ex);
            }
            return ret;
        }



        private string GetClientIP()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                HttpRequest request = context.Request;
                if (request != null)
                {
                    try
                    {
                        string userHostAddress = request.UserHostAddress;

                        // Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
                        // Could use TryParse instead, but I wanted to catch all exceptions
                        IPAddress.Parse(userHostAddress);

                        string xForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];

                        if (string.IsNullOrEmpty(xForwardedFor))
                            return userHostAddress;

                        // Get a list of public ip addresses in the X_FORWARDED_FOR variable
                        List<string> publicForwardingIps =
                            xForwardedFor.Split(',').Where(ip => !IsPrivateIpAddress(ip)).ToList();

                        // If we found any, return the last one, otherwise return the user host address
                        return publicForwardingIps.Any() ? publicForwardingIps.Last() : userHostAddress;
                    }
                    catch (Exception)
                    {
                        // Always return all zeroes for any failure (my calling code expects it)
                        //return "0.0.0.0";
                        return "0.0.0.0";
                    }
                }
            }

            //return "0.0.0.0";
            return "0.0.0.0";
        }

        private bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            IPAddress ip = IPAddress.Parse(ipAddress);
            byte[] octets = ip.GetAddressBytes();

            bool is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            bool is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            bool is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            bool isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }

        private string GetCurrentMethod(Int32 index = 2)
        {
            var st = new StackTrace();
            MethodBase method = st.GetFrame(index).GetMethod();
            string methodName = method.Name;
            //string className = method.ReflectedType.Name;
            //string fullMethodName = string.Format("{0}.{1}", className, methodName);
            //return fullMethodName;
            return methodName;
        }

        #region "IDisposable"
        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    //if (_doNotCallFacade != null) { _doNotCallFacade.Dispose(); }
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
