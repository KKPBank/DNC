using System.Collections.Generic;

namespace BatchSelectService.Message
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
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}
