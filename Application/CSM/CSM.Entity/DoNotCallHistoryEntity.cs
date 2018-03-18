using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Entity
{
    public class DoNotCallHistoryEntity
    {
        public int LogId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int VersionNo { get; set; }
        public DoNotCallUserModel EditBy { get; set; }

        public string EditByName => EditBy.DisplayName;
        public string TransactionDateTimeString => TransactionDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

        public int? EditByUserId { get; set; }
        public string CreateUsername { get; set; }
    }

    public class DoNotCallHistorySearchFilter
    {
        public DoNotCallHistorySearchFilter()
        {
            Pager = new Pager
            {
                PageNo = 1,
                PageSize = 10,
                TotalRecords = 0
            };
        }

        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int CustomerId { get; set; }
        public Pager Pager { get; set; }
    }

    public class DoNotCallTransactionHistoryEntity
    {
        public int VersionNo { get; set; }
        public string TransactionType { get; set; }
        public string CardNo { get; set; }
        public string CardTypeName { get; set; }
        public DoNotCallBasicInfoModel BaseInfo { get; set; }
        public List<DoNotCallEmailModel> Emails { get; set; }
        public List<DoNotCallTelephoneModel> Telephones { get; set; }
        public DoNotCallBlockInformationModel BlockInfoModel { get; set; }
        public DoNotCallBlockSalesModel BlockSalesModel { get; set; }
        public string DisplayTransactionType
        {
            get
            {
                switch (TransactionType)
                {
                    case Constants.DNC.TransactionTypeTelephone:
                        return Resource.Lbl_Telephone;
                    case Constants.DNC.TransactionTypeCustomer:
                        return Resource.Lbl_Customer;
                }

                return "Unknown";
            }
        }
    }
}
