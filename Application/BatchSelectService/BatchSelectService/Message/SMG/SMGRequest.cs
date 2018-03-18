using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BatchSelectService.Message.SMG
{
    public class SMGRequest
    {
        public string jobCode { get; set; }
        public SMGParam[] Params { get; set; }
    }
}