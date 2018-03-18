using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CSM.Service.Messages.Common;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Service.Messages.DoNotCall
{
    public class ExportFileRequest
    {
        [MessageHeader]
        public Header Header { get; set; }
    }
}
