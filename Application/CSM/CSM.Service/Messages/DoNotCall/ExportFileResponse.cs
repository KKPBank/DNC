using CSM.Common.Utilities;
using CSM.Service.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Service.Messages.DoNotCall
{
    public class ExportFileResponse
    {
        public ExportFileResponse()
        {
            ResultStatus = Constants.StatusResponse.Failed;
            Error = Constants.UnknownError;
        }

        [MessageHeader]
        public Header Header { get; set; }

        /// <summary>
        /// Succes/Failed
        /// </summary>
        public string ResultStatus { get; set; }
        public string Description { get; set; }
        public string Error { get; set; }
        public int ExportDataCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeSpan ElapsedTime => EndTime - StartTime;
    }
}
