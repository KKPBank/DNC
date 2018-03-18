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
    public class DoNotCallNewTelephoneModel : IValidatableObject
    {
        [Display(Name = ResourceName.Lbl_Telephone, ResourceType = typeof(Resource))]
        [LocalizedRegex(RegexFormat.Telephone, ResourceName.ValErr_NumericOnly)]
        [LocalizedStringLength(MaxLength.DoNotCallPhoneNo, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.PhoneNo, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string PhoneNo { get; set; }

        [Display(Name = ResourceName.Lbl_Email, ResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessage = ErrorMessage.InvalidEmailFormat)]
        [LocalizedStringLength(MaxLength.Email, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(PhoneNo) && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(string.Format(Resource.ValErr_RequiredField, "Email"), new[] { nameof(Email) });
                yield return new ValidationResult(string.Format(Resource.ValErr_RequiredField, "Telephone"), new[] { nameof(PhoneNo) });
            }
        }
    }

    public class DoNotCallByPhoneSearchResultViewModel
    {
        public DoNotCallByPhoneSearchResultViewModel()
        {
            Transactions = new List<DoNotCallTransactionModel>();
            Pager = new Pager
            {
                PageNo = 1,
                TotalRecords = 0,
                PageSize = 10
            };
        }

        public Pager Pager { get; set; }
        public List<DoNotCallTransactionModel> Transactions { get; set; }
    }
}