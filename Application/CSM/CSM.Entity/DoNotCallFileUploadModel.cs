using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Entity
{
    public class DoNotCallFileUploadModel
    {
    }

    public class DoNotCallFileUploadDetailModel
    {
        public DoNotCallFileUploadDetailModel()
        {
            DataList = new List<DoNotCallFileUploadDataModel>();
            Pager = new Pager
            {
                PageSize = 15,
                TotalRecords = 0,
                SortField = nameof(DoNotCallFileUploadDataModel.FirstName)
            };
        }

        public Pager Pager { get; set; }

        public int FileUploadId { get; set; }
        public string FileName { get; set; }
        public bool IsSuccessUpload { get; set; }
        public DateTime CreateDate { get; set; }
        public List<DoNotCallFileUploadDataModel> DataList { get; set; }

        public string UploadResult => IsSuccessUpload ? Resource.Ddl_Status_Success : Resource.Ddl_Status_Fail;
        public string CrateDateDisplay => CreateDate.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);
    }

    public class DoNotCallFileUploadSortFilter : Pager
    {
        public int Id { get; set; }
    }

    public class DoNotCallFileUploadDataModel
    {
        public int RowNum { get; set; }
        public string TransactionType { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlockSalesTelephone { get; set; }
        public bool IsBlockInfoTelephone { get; set; }
        public bool IsBlockSalesSMS { get; set; }
        public bool IsBlockInfoSMS { get; set; }
        public bool IsBlockSalesEmail { get; set; }
        public bool IsBlockInfoEmail { get; set; }
        public string CardNo { get; set; }
        public int? CardTypeId { get; set; }
        public string CardTypeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }

        public string TransactionTypeName
        {
            get
            {
                switch (TransactionType)
                {
                    case Constants.DNC.TransactionTypeCustomer:
                        return Resource.Lbl_Customer;
                    case Constants.DNC.TransactionTypeTelephone:
                        return Resource.Lbl_Telephone;
                    default:
                        return "Unknown";
                }
            }
        }

        public string Status => IsActive ? Constants.DNC.Active : Constants.DNC.Inactive;
        public string BlockSalesTelephone => GetBlockDisplayName(IsBlockSalesTelephone);
        public string BlockInfoTelephone => GetBlockDisplayName(IsBlockInfoTelephone);
        public string BlockSalesSMS => GetBlockDisplayName(IsBlockSalesSMS);
        public string BlockInfoSMS => GetBlockDisplayName(IsBlockInfoSMS);
        public string BlockSalesEmail => GetBlockDisplayName(IsBlockSalesEmail);
        public string BlockInfoEmail => GetBlockDisplayName(IsBlockInfoEmail);

        public static string GetBlockDisplayName(bool isBlock)
        {
            return isBlock ? Constants.Yes : Constants.No;
        }
    }

    public class DoNotCallFileUploadSearchResultModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int RecordCount { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DoNotCallUserModel LastUpdateBy { get; set; }

        public string RecourdCountDisplay => RecordCount.FormatNumber();
        public string DisplayUpdateDate => UpdateDate.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);
        public string DisplayUploadDate => UploadDate.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);

        public int? UpdateByUserId { get; set; }
    }

    public class DoNotCallFileUploadResultModel
    {
        public DoNotCallFileUploadResultModel()
        {
            ErrorResults = new List<RowErrorResult>();
        }

        public string FileName { get; set; }
        public List<RowErrorResult> ErrorResults { get; set; }
        public bool IsValidColumnHeaders { get; set; }
        public int DataRowCount { get; set; }
        public string SystemError { get; set; }
        public bool Success { get; set; }

        public string ErrorDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SystemError))
                {
                    return ErrorResults.Count >= 100 ? "100+" : ErrorResults.Count.ToString();
                }
                else
                {
                    return SystemError;
                }
            }
        }
    }

    public class RowErrorResult
    {
        public RowErrorResult()
        {
            CellErrorDict = new Dictionary<string, string>();
        }
        public int RowNum { get; set; }
        public Dictionary<string, string> CellErrorDict { get; set; }

        public string DescriptionDisplay
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Join(", ", CellErrorDict.Keys));
                sb.Append(": ");
                sb.Append(string.Join(", ", CellErrorDict.Values));
                return sb.ToString();
            }
        }
    }
}
