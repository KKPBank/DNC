using CSM.Common.Utilities;
using System.Collections.Generic;

namespace CSM.Service.Messages.Common
{
    public class StatusResponse
    {
        public string Description { get; set; }
        public string ErrorCode { get; set; }
        public string Status { get; set; }
        public List<string> BranchCodeNotFoundList { get; set; } 
    }

    public class BaseStatusResponse
    {
        public BaseStatusResponse()
        {
            ResponseCode = Constants.InterfaceResponseCode.UnknownError;
            ResponseMessage = Constants.UnknownError;
        }

        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}
