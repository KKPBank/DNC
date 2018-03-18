using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Entity.Common;
using MvcPaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using static CSM.Common.Utilities.Constants;

namespace CSM.Web.Models
{
    public class DoNotCallViewModel
    {
        public DoNotCallViewModel()
        {
            DoNotCallList = new List<DoNotCallSearchResultViewModel>();
            SearchFilter = new DoNotCallListSearchFilter();
        }

        public List<DoNotCallSearchResultViewModel> DoNotCallList { get; set; }
        public DoNotCallListSearchFilter SearchFilter { get; set; }
    }

    public class DoNotCallSearchResultViewModel
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string IsBlockSalesTelephone { get; set; }
        public string IsBlockInformationTelephone { get; set; }
        public string IsBlockSalesSms { get; set; }
        public string IsBlockInformationSms { get; set; }
        public string IsBlockSalesEmail { get; set; }
        public string IsBlockInformationEmail { get; set; }
        public string CreateByName { get; set; }
        public string CardNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string TransactionType { get; set; }
        public string SubscriptionType { get; set; }

        public string DisplayStatus { get; set; }
        public string TransactionDateString => TransactionDate.FormatDateTime(DateTimeFormat.DefaultFullDateTime);
    }

    public class DoNotCallExcelViewModel
    {
        [DisplayName("No.")]
        public int Id { get; set; }
        [DisplayName("Transaction Date")]
        public DateTime TransactionDate { get; set; }
        [DisplayName("Do Not Call List Status")]
        public string Status { get; set; }
        [DisplayName("Block Sales (Telephone)")]
        public string IsBlockSalesTelephone { get; set; }
        [DisplayName("Block Information (Telephone)")]
        public string IsBlockInformationTelephone { get; set; }
        [DisplayName("Block Sales (SMS)")]
        public string IsBlockSalesSms { get; set; }
        [DisplayName("Block Information (SMS)")]
        public string IsBlockInformationSms { get; set; }
        [DisplayName("Block Sales (Email)")]
        public string IsBlockSalesEmail { get; set; }
        [DisplayName("Block Information (Email)")]
        public string IsBlockInformationEmail { get; set; }
        [DisplayName("Create By")]
        public string CreateByName { get; set; }
        [DisplayName("Card ID")]
        public string CardNo { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }

}