using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using CSM.Service.Messages.Common;
using System.Runtime.Serialization;

namespace CSM.Service.Messages.DoNotCall
{
    [DataContract]
    public class InquireDoNotCallRequest
    {
        [DataMember(Order = 1, IsRequired = true)]
        public Header Header { get; set; }

        [DataMember(Order = 2)]
        public string CardNo { get; set; }
        [DataMember(Order = 3)]
        public string SubscriptionTypeCode { get; set; }
        [DataMember(Order = 4)]
        public string ProductCode { get; set; }
        [DataMember(Order = 5)]
        public int CisId { get; set; }
        [DataMember(Order = 6)]
        public string PhoneNo { get; set; }
        [DataMember(Order = 7)]
        public string Email { get; set; }
        [DataMember(Order = 8, IsRequired = true)]
        public int DataLimit { get; set; }
    }
}
