using CSM.Common.Resources;
using CSM.Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static CSM.Common.Utilities.Constants;

namespace CSM.Web.Models
{
    public class EmailInputModel
    {
        [Display(Name = ResourceName.Lbl_Email, ResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessage = ErrorMessage.InvalidEmailFormat)]
        [LocalizedStringLength(MaxLength.Email, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }
    }
}