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
    
    public partial class TB_C_ROLE_SR_PAGE
    {
        public int ROLE_SR_PAGE_ID { get; set; }
        public int ROLE_ID { get; set; }
        public int SR_PAGE_ID { get; set; }
    
        public virtual TB_C_ROLE TB_C_ROLE { get; set; }
        public virtual TB_C_SR_PAGE TB_C_SR_PAGE { get; set; }
    }
}
