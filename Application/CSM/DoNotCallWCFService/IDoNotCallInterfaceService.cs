using CSM.Service.Messages.DoNotCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DoNotCallWCFService
{
    //TODO: add contract namespace
    [ServiceContract(Namespace = "")]
    public interface IDoNotCallInterfaceService
    {
        [OperationContract]
        InquireDoNotCallResponse InquiryDoNotCallList(InquireDoNotCallRequest requet);

        [OperationContract]
        InsertOrUpdateDoNotCallCustomerResponse InsertOrUpdateDoNotCallCustomer(InsertOrUpdateDoNotCallCustomerRequest request);

        [OperationContract]
        InsertOrUpdateDoNotCallPhoneResponse InsertOrUpdateDoNotCallPhone(InsertOrUpdateDoNotCallPhoneRequest request);
    }
}
