using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSM.Web.Models
{
    public class AutoCompleteSearchProductInputModel
    {
        public AutoCompleteSearchProductInputModel()
        {
            ExceptProductIds = new List<int>();
        }

        public string Keyword { get; set; }
        public List<int> ExceptProductIds { get; set; }
    }
}