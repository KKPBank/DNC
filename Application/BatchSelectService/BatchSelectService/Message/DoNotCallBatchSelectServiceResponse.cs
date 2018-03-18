using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BatchSelectService.Message.SMG;

namespace BatchSelectService.Message
{
    public class DoNotCallBatchSelectServiceResponse : BaseStatusResponse
    {
        public DoNotCallBatchSelectServiceResponse()
        {
            SMGStatus = new SMGResponseStatus();
            SLMStatus = new SLMMasterService.ResponseStatus();
        }
        public SMGResponseStatus SMGStatus { get; set; }
        public SLMMasterService.ResponseStatus SLMStatus { get; set; }
    }
}