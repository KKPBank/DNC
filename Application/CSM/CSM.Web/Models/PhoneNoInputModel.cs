using CSM.Common.Resources;
using CSM.Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using static CSM.Common.Utilities.Constants;

namespace CSM.Web.Models
{
    public class PhoneNoInputModel
    {
        [Display(Name = ResourceName.Lbl_Telephone, ResourceType = typeof(Resource))]
        [LocalizedRegex(RegexFormat.Telephone, ResourceName.ValErr_NumericOnly)]
        [LocalizedStringLength(MaxLength.DoNotCallPhoneNo, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.PhoneNo, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; }
    }
}