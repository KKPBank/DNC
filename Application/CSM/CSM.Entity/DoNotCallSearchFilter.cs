using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSM.Common.Utilities.Constants;

namespace CSM.Entity
{
    public class DoNotCallSearchFilter
    {
    }

    public class DoNotCallLoadListSearchFilter : Pager, IValidatableObject
    {
        public DoNotCallLoadListSearchFilter()
        {
            SortField = "TransactionDate";
        }
        [Display(Name = ResourceName.Lbl_UploadDate, ResourceType = typeof(Resource))]
        public DateTime? FromDate { get; set; }
        [Display(Name = ResourceName.Lbl_To, ResourceType = typeof(Resource))]
        public DateTime? ToDate { get; set; }
        [Display(Name = ResourceName.Lbl_FileName, ResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.SearchTerm, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string FileName { get; set; }
        [Display(Name = ResourceName.Lbl_FileStatus, ResourceType = typeof(Resource))]
        public string FileStatusId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
            {
                yield return new ValidationResult("ช่วงวันที่ไม่ถูกต้อง", new string[] { nameof(FromDate), nameof(ToDate) });
            }
        }
    }

    public class DoNotCallListSearchFilter : Pager, IValidatableObject
    {
        [Display(Name = ResourceName.Lbl_TransactionDate, ResourceType = typeof(Resource))]
        public DateTime? FromDate { get; set; }
        [Display(Name = ResourceName.Lbl_To, ResourceType = typeof(Resource))]
        public DateTime? ToDate { get; set; }
        [Display(Name = ResourceName.Lbl_SubscriptionType, ResourceType = typeof(Resource))]
        public int? SubscriptionTypeId { get; set; }
        [Display(Name = ResourceName.Lbl_CardId, ResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.SearchTerm, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string CardNo { get; set; }
        [Display(Name = ResourceName.Lbl_FirstNameEnglish, ResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.SearchTerm, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string FirstName { get; set; }
        [Display(Name = ResourceName.Lbl_LastNameEnglish, ResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.SearchTerm, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string LastName { get; set; }
        [Display(Name = ResourceName.Lbl_Telephone, ResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.SearchTerm, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string Telephone { get; set; }
        [Display(Name = ResourceName.Lbl_Email, ResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.SearchTerm, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }
        [Display(Name = ResourceName.Lbl_DoNotCallListStatus, ResourceType = typeof(Resource))]
        public int? DoNotCallListStatusId { get; set; }
        [Display(Name = ResourceName.Lbl_Type, ResourceType = typeof(Resource))]
        public string TransactionType { get; set; }
        [Display(Name = ResourceName.Lbl_UpdateBranch, ResourceType = typeof(Resource))]
        public int? UpdateBranchId { get; set; }
        [Display(Name = ResourceName.Lbl_UpdateUser, ResourceType = typeof(Resource))]
        public int? UpdateUser { get; set; }
        [Display(Name = ResourceName.Lbl_CisId, ResourceType = typeof(Resource))]
        public int? CisId { get; set; }
        public string ProductCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
            {
                yield return new ValidationResult("ช่วงวันที่ไม่ถูกต้อง", new string[] { nameof(FromDate), nameof(ToDate) });
            }
        }
    }
}
