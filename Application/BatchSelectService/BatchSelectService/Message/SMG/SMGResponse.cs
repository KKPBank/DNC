using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BatchSelectService.Message.SMG
{
    public class SMGResponse
    {
        public string jobCode { get; set; }
        public SMGResponseStatus responseStatus { get; set; }
    }
}