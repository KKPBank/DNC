//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSM.Data.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class TB_T_DEFAULT_SEARCH
    {
        public int USER_SEARCH_ID { get; set; }
        public string SEARCH_PAGE { get; set; }
        public int USER_ID { get; set; }
        public bool IS_SELECTED { get; set; }
    
        public virtual TB_R_USER TB_R_USER { get; set; }
    }
}
