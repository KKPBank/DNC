using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using static CSM.Common.Utilities.Constants;

namespace CSM.Entity
{
    public class DoNotCallEntity
    {
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string CardNo { get; set; }
        public DoNotCallUserModel CreateBy { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BlockInformationEmail { get; set; }
        public string BlockInformationSms { get; set; }
        public string BlockInformationTelephone { get; set; }
        public string BlockSalesEmail { get; set; }
        public string BlockSalesSms { get; set; }
        public string BlockSalesTelephone { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string SubscriptionType { get; set; }
        public string CreateByUsername { get; set; }
        public int? CreateByUserId { get; set; }
        public int? SubscriptionTypeId { get; set; }
    }

    public class DoNotCallTransactionInfo
    {
        public DoNotCallTransactionInfo()
        {
            SalesProducts = new List<ActivityProductEntity>();
            InfoProducts = new List<ActivityProductEntity>();
            Emails = new List<DoNotCallEmailModel>();
            Telephones = new List<DoNotCallTelephoneModel>();
        }
        public int TransactionId { get; set; }
        public List<ActivityProductEntity> SalesProducts { get; set; }
        public List<ActivityProductEntity> InfoProducts { get; set; }
        public List<DoNotCallEmailModel> Emails { get; set; }
        public List<DoNotCallTelephoneModel> Telephones { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsBlockAllSalesProduct { get; set; }
        public bool IsBlockAllInfoProduct { get; set; }
    }

    public class DoNotCallExcelModel
    {
        public int No { get; set; }
        [DisplayName("Customer/Telephone")]
        public string TransactionType { get; set; }
        [DisplayName("เลขบัตร")]
        public string CardNo { get; set; }
        [DisplayName("ประเภทบัตร")]
        public string SubscriptionTypeName { get; set; }
        [DisplayName("ชื่อลูกค้า")]
        public string FirstName { get; set; }
        [DisplayName("นามสกุลลูกค้า")]
        public string LastName { get; set; }
        [DisplayName("โทรศัพท์ (มือถือ,บ้าน,ที่ทำงาน ,อื่นๆ)")]
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        [DisplayName("Expiry Date")]
        public string ExpireDate { get; set; }
        [DisplayName("Do Not Call List Status")]
        public string Status { get; set; }
        [DisplayName("Remark")]
        public string Remark { get; set; }
        [DisplayName("Created Date")]
        public string CreateDate { get; set; }
        [DisplayName("Created By")]
        public string CreateBy { get; set; }
        [DisplayName("Created Branch")]
        public string CreateBranch { get; set; }
        [DisplayName("Update Date")]
        public string UpdateDate { get; set; }
        [DisplayName("Update By")]
        public string UpdateBy { get; set; }
        [DisplayName("Update Branch")]
        public string UpdateBranch { get; set; }
        [DisplayName("System")]
        public string System { get; set; }
        [DisplayName("Sales: Block Email")]
        public string BlockSalesEmail { get; set; }
        [DisplayName("Sales: Block SMS")]
        public string BlockSalesSms { get; set; }
        [DisplayName("Sales: Block Telephone")]
        public string BlockSalesTelephone { get; set; }
        [DisplayName("Sales: All Product")]
        public string BlockSalesAllProduct { get; set; }
        [DisplayName("Sales: Product")]
        public string SalesProducts { get; set; }
        [DisplayName("Sales: Created Date")]
        public string SalesCreateDate => CreateDate;
        [DisplayName("Sales: Created By")]
        public string SalesCreateBy => CreateBy;
        [DisplayName("Sales: Created Branch")]
        public string SalesCreateBranch => CreateBranch;
        [DisplayName("Sales: Update Date")]
        public string SalesUpdateDate => UpdateDate;
        [DisplayName("Sales: Update By")]
        public string SalesUpdateBy => UpdateBy;
        [DisplayName("Sales: Update Branch")]
        public string SalesUpdateBranch => UpdateBranch;
        [DisplayName("Sales: System")]
        public string SalesSystem => System;
        [DisplayName("Information: Block Email")]
        public string BlockInformationEmail { get; set; }
        [DisplayName("Information: Block SMS")]
        public string BlockInformationSms { get; set; }
        [DisplayName("Information: Block Telephone")]
        public string BlockInformationTelephone { get; set; }
        [DisplayName("Information: All Product")]
        public string BlockInfoAllProduct { get; set; }
        [DisplayName("Information: Product")]
        public string InfoProducts { get; set; }
        [DisplayName("Information: Created Date")]
        public string InfoCreateDate => CreateDate;
        [DisplayName("Information: Created By")]
        public string InfoCreateBy => CreateBy;
        [DisplayName("Information: Created Branch")]
        public string InfoCreateBranch => CreateBranch;
        [DisplayName("Information: Update Date")]
        public string InfoUpdateDate => UpdateDate;
        [DisplayName("Information: Update By")]
        public string InfoUpdateBy => UpdateBy;
        [DisplayName("Information: Update Branch")]
        public string InfoUpdateBranch => UpdateBranch;
        [DisplayName("Information: System")]
        public string InfoSystem => System;
    }

    public class DoNotCallTransactionEntity
    {
        public DoNotCallTransactionEntity()
        {
            BasicInfo = new DoNotCallBasicInfoModel();
            BlockInformation = new DoNotCallBlockInformationModel();
            BlockSales = new DoNotCallBlockSalesModel();
            ContactDetail = new DoNotCallContactModel();
        }

        public int CurrentUserId { get; set; }
        public string CurrentUsername { get; set; }
        public string CurrentUserIpAddress { get; set; }
        public DoNotCallCardInfoModel CardInfo { get; set; }
        public DoNotCallBasicInfoModel BasicInfo { get; set; }
        public DoNotCallBlockInformationModel BlockInformation { get; set; }
        public DoNotCallBlockSalesModel BlockSales { get; set; }
        public DoNotCallContactModel ContactDetail { get; set; }

        public bool IsNewCustomer => BasicInfo.TransactionId == 0;

        [Range(typeof(bool), "true", "true", ErrorMessage = "**ต้องเลือก Block อย่างน้อย 1 รายการ")]
        public bool HasBlockItems
        {
            get
            {
                bool isBlockInformation = BlockInformation.IsBlockInfoEmail || BlockInformation.IsBlockInfoSms || BlockInformation.IsBlockInfoTelephone;
                bool isBlockSales = BlockSales.IsBlockSalesEmail || BlockSales.IsBlockSalesSms || BlockSales.IsBlockSalesTelephone;
                return isBlockInformation || isBlockSales;
            }
        }

    }

    public class DoNotCallTransactionModel
    {
        public int Id { get; set; }
        [Display(Name = "Transaction Date")]
        public DateTime CreateDate { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CardNo { get; set; }
        [Display(Name = "Telephone")]
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public DoNotCallUserModel CreateBy { get; set; }

        public string CreateByName => CreateBy.DisplayName;

        public string DisplayCreateDate => CreateDate.FormatDateTime("dd/MM/yyyy");
        [Display(Name = "Status")]
        public string DisplayStatus => IsActive ? Resource.Lbl_Active : Resource.Lbl_Inactive;

        public string CreateUserName { get; set; }
        public int? CreateByUserId { get; set; }
    }

    public class DoNotCallUserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PositionCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => UserId > 0 ? $"{PositionCode} - {FirstName} {LastName}": Username;
    }

    public class DoNotCallEmail
    {
        public DoNotCallEmail()
        {
            EmailList = new List<DoNotCallEmailModel>();
        }

        [Display(Name = ResourceName.Lbl_Email, ResourceType = typeof(Resource))]
        public List<DoNotCallEmailModel> EmailList { get; set; }

        [Display(Name = ResourceName.Lbl_Email, ResourceType = typeof(Resource))]
        [LocalizedStringLength(MaxLength.Email, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        public string InputText { get; set; }
    }

    public class DoNotCallTelephone
    {
        public DoNotCallTelephone()
        {
            TelephoneList = new List<DoNotCallTelephoneModel>();
        }

        [Display(Name = ResourceName.Lbl_Telephone, ResourceType = typeof(Resource))]
        public List<DoNotCallTelephoneModel> TelephoneList { get; set; }

        [Display(Name = ResourceName.Lbl_Telephone, ResourceType = typeof(Resource))]
        [LocalizedRegex(RegexFormat.Telephone, ResourceName.ValErr_NumericOnly)]
        [LocalizedStringLength(MaxLength.DoNotCallPhoneNo, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.PhoneNo, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType =typeof(Resource))]
        public string InputText { get; set; }
    }

    public class DoNotCallEmailModel
    {
        public DoNotCallEmailModel()
        {
            IsDeleted = false;
        }

        [Display(Name = ResourceName.Lbl_Email, ResourceType = typeof(Resource))]
        [EmailAddress(ErrorMessage = ErrorMessage.InvalidEmailFormat)]
        [LocalizedStringLength(MaxLength.Email, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        public string Email { get; set; }

        public int Id { get; set; }
        public DoNotCallUserModel UpdateBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsDeleted { get; set; }

        public bool CanDelete => !IsDeleted;
        public string Status => IsDeleted ? "Deleted" : "Active";
        public string LastUpdateDateStr => LastUpdateDate.FormatDateTime(DateTimeFormat.DefaultFullDateTime);

        public int LastUpdateUserId { get; set; }
        public string CreateUsername { get; set; }
    }

    public class DoNotCallTelephoneModel
    {
        public DoNotCallTelephoneModel()
        {
            IsDeleted = false;
        }

        [Display(Name = ResourceName.Lbl_Telephone, ResourceType = typeof(Resource))]
        [LocalizedRegex(RegexFormat.Telephone, ResourceName.ValErr_NumericOnly)]
        [LocalizedStringLength(MaxLength.DoNotCallPhoneNo, ErrorMessageResourceName = ResourceName.ValErr_StringLength, ErrorMessageResourceType = typeof(Resource))]
        [LocalizedMinLength(MinLenght.PhoneNo, ErrorMessageResourceName = ResourceName.ValErr_MinLength, ErrorMessageResourceType =typeof(Resource))]
        public string PhoneNo { get; set; }

        public int Id { get; set; }
        public DoNotCallUserModel UpdateBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsDeleted { get; set; }

        public bool CanDelete => !IsDeleted;
        public string Status => IsDeleted ? "Deleted" : "Active";
        public string LastUpdateDateStr => LastUpdateDate.FormatDateTime(DateTimeFormat.DefaultFullDateTime);

        public int LastUpdateUserId { get; set; }
        public string CreateUsername { get; set; }
    }

    public class DoNotCallContactModel
    {
        public DoNotCallContactModel()
        {
            Telephone = new DoNotCallTelephone();
            Email = new DoNotCallEmail();
        }

        public DoNotCallTelephone Telephone { get; set; }
        public DoNotCallEmail Email { get; set; }

        public bool HasActiveEmail => Email.EmailList.Any(e => !e.IsDeleted);
        public bool HasActiveTelephone => Telephone.TelephoneList.Any(e => !e.IsDeleted);
    }

    public class DoNotCallBlockInformationModel
    {
        public DoNotCallBlockInformationModel()
        {
            BlockInfoProductList = new List<DoNotCallProductModel>();
            IsBlockInfoEmail = false;
            IsBlockInfoTelephone = false;
            IsBlockInfoSms = false;
            IsBlockAllInfoProducts = false;
        }

        [Display(Name = "Block Email")]
        public bool IsBlockInfoEmail { get; set; }
        [Display(Name = "Block Telephone")]
        public bool IsBlockInfoTelephone { get; set; }
        [Display(Name = "Block SMS")]
        public bool IsBlockInfoSms { get; set; }
        [Display(Name = "All Product")]
        public bool IsBlockAllInfoProducts { get; set; }
        [Display(Name = ResourceName.Lbl_Product, ResourceType = typeof(Resource))]
        public List<DoNotCallProductModel> BlockInfoProductList { get; set; }
    }

    public class DoNotCallBlockSalesModel
    {
        public DoNotCallBlockSalesModel()
        {
            BlockSalesProductList = new List<DoNotCallProductModel>();
            IsBlockSalesEmail = false;
            IsBlockSalesTelephone = true;
            IsBlockSalesSms = true;
            IsBlockAllSalesProducts = true;
        }

        [Display(Name = "Block Email")]
        public bool IsBlockSalesEmail { get; set; }
        [Display(Name = "Block Telephone")]
        public bool IsBlockSalesTelephone { get; set; }
        [Display(Name = "Block SMS")]
        public bool IsBlockSalesSms { get; set; }
        [Display(Name = "All Product")]
        public bool IsBlockAllSalesProducts { get; set; }
        [Display(Name = ResourceName.Lbl_Product, ResourceType = typeof(Resource))]
        public List<DoNotCallProductModel> BlockSalesProductList { get; set; }
    }

    public class DoNotCallProductModel
    {
        public DoNotCallProductModel()
        {
            IsDeleted = false;
        }

        public int Id { get; set; }
        [Display(Name = ResourceName.Lbl_Product, ResourceType = typeof(Resource))]
        public string Name { get; set; }
        [Display(Name = ResourceName.Lbl_Date, ResourceType = typeof(Resource))]
        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
        public DoNotCallUserModel UpdateBy { get; set; }
        public string ActivityProductType { get; set; }
        public int ActivityTypeId { get; set; }
        public int ActivityProductId { get; set; }
        public bool IsDeleted { get; set; }

        public string Status => IsDeleted ? "Deleted" : "Active";
        public string DisplayUpdateDate => UpdateDate.FormatDateTime(DateTimeFormat.DefaultFullDateTime);

        public int ProductId { get; set; }
        public string CreateByUsername { get; set; }
        public int? CreateByUserId { get; set; }
    }

    public class DoNotCallCardInfoModel 
    {
        [Display(Name = ResourceName.Lbl_CardId, ResourceType = typeof(Resource))]
        public string CardNo { get; set; }
        [Display(Name = ResourceName.Lbl_SubscriptionType, ResourceType = typeof(Resource))]
        public int? SubscriptionTypeId { get; set; }
    }

    public class DoNotCallBasicInfoModel : IValidatableObject
    {
        public DoNotCallBasicInfoModel()
        {
            IsActive = true;
            DateTime now = DateTime.Now;
            IsNeverExpire = false;
            FromSystem = SystemName.DoNotCall;
            ExpireDate = now.Date.AddYears(1);
            EffectiveDate = now.Date;
            CreateDate = now;
            UpdateDate = now;
        }
        public int TransactionId { get; set; }
        [Display(Name = ResourceName.Lbl_FirstNameEnglish, ResourceType = typeof(Resource))]
        [MaxLength(MaxLength.FirstName)]
        public string FirstName { get; set; }
        [Display(Name = ResourceName.Lbl_LastNameEnglish, ResourceType = typeof(Resource))]
        [MaxLength(MaxLength.LastName)]
        public string LastName { get; set; }
        [Display(Name = ResourceName.Lbl_ExpiryDateEng, ResourceType = typeof(Resource))]
        public DateTime ExpireDate { get; set; }
        [Display(Name = ResourceName.Lbl_EffectiveDateEN, ResourceType = typeof(Resource))]
        public DateTime EffectiveDate { get; set; }
        [Display(Name = ResourceName.Lbl_Status, ResourceType = typeof(Resource))]
        public string DisplayStatus => IsActive ? Resource.Lbl_Active : Resource.Lbl_Inactive;
        public bool IsActive { get; set; }
        [Display(Name = ResourceName.Lbl_System, ResourceType = typeof(Resource))]
        public string FromSystem { get; set; }
        [Display(Name = ResourceName.Lbl_CreateBy, ResourceType = typeof(Resource))]
        public string CreateByName => CreateBy?.DisplayName;
        [Display(Name = ResourceName.Lbl_LastUpdateUser, ResourceType = typeof(Resource))]
        public string UpdateByName => UpdateBy?.DisplayName;
        [Display(Name = ResourceName.Lbl_CreateDate, ResourceType = typeof(Resource))]
        public DateTime CreateDate { get; set; }
        [Display(Name = ResourceName.Lbl_LastUpdateDate, ResourceType = typeof(Resource))]
        public DateTime UpdateDate { get; set; }
        [Display(Name = ResourceName.Lbl_Remark, ResourceType = typeof(Resource))]
        public string Remark { get; set; }
        [Display(Name = "ไม่มีวันหมดอายุ")]
        public bool IsNeverExpire { get; set; }

        public DoNotCallUserModel CreateBy { get; set; }
        public DoNotCallUserModel UpdateBy { get; set; }

        public string DisplayExpireDateAndStatus {
            get {
                string display = ExpireDate.FormatDateTime("dd/MM/yyyy");

                if (IsNeverExpire)
                    display += " (ไม่มีวันหมดอายุ)";

                return display;
            }
        }

        public string DisplayCreateDate => CreateDate.FormatDateTime("dd/MM/yyyy");
        public string DisplayEffectiveDate => EffectiveDate.FormatDateTime("dd/MM/yyyy");
        public string DisplayUpdateDate => UpdateDate.FormatDateTime("dd/MM/yyyy");

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                yield return new ValidationResult(string.Format(Resource.ValErr_RequiredField, Resource.Lbl_FirstNameEnglish), new[] { nameof(FirstName) });
            }

            if (EffectiveDate > ExpireDate)
            {
                yield return new ValidationResult(Resource.ValErr_InvalidDateRange, new[] { nameof(ExpireDate) });
            }
        }
    }
}
