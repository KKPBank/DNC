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
    
    public partial class TB_R_CHANNEL
    {
        public TB_R_CHANNEL()
        {
            this.TB_M_SLA = new HashSet<TB_M_SLA>();
            this.TB_R_BRANCH = new HashSet<TB_R_BRANCH>();
            this.TB_T_JOB = new HashSet<TB_T_JOB>();
            this.TB_T_SR = new HashSet<TB_T_SR>();
        }
    
        public int CHANNEL_ID { get; set; }
        public string CHANNEL_NAME { get; set; }
        public Nullable<short> STATUS { get; set; }
        public string CHANNEL_CODE { get; set; }
        public string EMAIL { get; set; }
    
        public virtual ICollection<TB_M_SLA> TB_M_SLA { get; set; }
        public virtual ICollection<TB_R_BRANCH> TB_R_BRANCH { get; set; }
        public virtual ICollection<TB_T_JOB> TB_T_JOB { get; set; }
        public virtual ICollection<TB_T_SR> TB_T_SR { get; set; }
    }
}
