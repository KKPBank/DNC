using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Entity
{
    public class ActivityProductEntity
    {
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public int ActivityProductId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
