using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using CSM.Service.Messages.Common;

namespace CSM.Service.Messages.DoNotCall
{
    public class ExportFileToTotRequest
    {
        [MessageHeader]
        public Header Header { get; set; }

        /// <summary>
        /// Format: "HH:mm"
        /// </summary>
        public string ExecuteTime { get; set; }
    }
}
