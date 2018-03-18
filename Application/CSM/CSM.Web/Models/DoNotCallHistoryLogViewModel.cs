using CSM.Entity;
using CSM.Entity.Common;
using System.Collections.Generic;

namespace CSM.Web.Models
{
    public class DoNotCallHistoryLogViewModel
    {
        public DoNotCallHistoryLogViewModel()
        {
            LogList = new List<DoNotCallHistoryEntity>();
            Pager = new Pager
            {
                PageNo = 1,
                TotalRecords = 0,
                PageSize = 10
            };
        }

        public Pager Pager { get; set; }
        public List<DoNotCallHistoryEntity> LogList { get; set; }
    }
}