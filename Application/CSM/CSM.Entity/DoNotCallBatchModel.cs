using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Entity
{
    public class DoNotCallBatchModel
    {
    }

    public class DoNotCallUpdatePhoneNoModel
    {
        public string PhoneNo { get; set; }
        /// <summary>
        /// "Block" or "Unblock"
        /// </summary>
        public string Status { get; set; }
    }

    public class DoNotCallUpdateExpireResultModel
    {
        public int TransactionId { get; set; }
        public string PhoneNo { get; set; }
    }
}
