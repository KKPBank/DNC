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
    
    public partial class TB_T_DNC_EMAIL
    {
        public int DNC_EMAIL_ID { get; set; }
        public Nullable<int> DNC_TRANSACTION_ID { get; set; }
        public string EMAIL { get; set; }
        public string DELETE_STATUS { get; set; }
        public Nullable<System.DateTime> CREATE_DATE { get; set; }
        public Nullable<int> CREATE_USER { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public Nullable<int> UPDATE_USER { get; set; }
        public string CREATE_USERNAME { get; set; }
        public string UPDATE_USERNAME { get; set; }
    
        public virtual TB_T_DNC_TRANSACTION TB_T_DNC_TRANSACTION { get; set; }
    }
}
