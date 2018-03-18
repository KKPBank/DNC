using CSM.Service.Messages.DoNotCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DoNotCallWCFService
{
    //TODO: add contract namespace
    [ServiceContract(Namespace = "")]
    public interface IDoNotCallBatchProcess: IDisposable
    {
        [OperationContract]
        ExportFileResponse ExportFile(ExportFileRequest requet);

        [OperationContract]
        ExportFileResponse ExportFileToTOT(ExportFileToTotRequest request);
    }
}
