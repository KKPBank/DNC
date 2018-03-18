using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static CSM.Common.Utilities.Constants;

namespace CSM.Web.Models
{
    public class DoNotCallFileUploadInputModel
    {
        [LocalizedRequired(ErrorMessageResourceName = ResourceName.ValErr_RequiredField, ErrorMessageResourceType = typeof(Resource))]
        public HttpPostedFileBase File { get; set; }
        public string AllowedFileType { get; set; }
        public int LimitFileSize { get; set; }

        public int LimitFileSizeInMB => LimitFileSize / KbPerMB;
    }

    public class DoNotCallUploadSearchResultViewModel
    {
        public DoNotCallUploadSearchResultViewModel()
        {
            Results = new List<DoNotCallFileUploadSearchResultModel>();
            Pager = new Pager
            {
                PageNo = 1,
                TotalRecords = 0,
                PageSize = 15
            };
        }

        public Pager Pager { get; set; }
        public List<DoNotCallFileUploadSearchResultModel> Results { get; set; }
    }
}