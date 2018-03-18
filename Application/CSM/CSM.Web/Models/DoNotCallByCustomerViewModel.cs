using CSM.Entity;
using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSM.Web.Models
{
    public class DoNotCallByCustomerSearchInputModel
    {
        public string CardNo { get; set; }
        public int SubscriptionTypeId { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }

    public class DoNotCallByCustomerSearchResultViewModel
    {
        public DoNotCallByCustomerSearchResultViewModel()
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