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
    
    public partial class TB_M_AREA_SUBAREA
    {
        public int AREA_SUBAREA_ID { get; set; }
        public int AREA_ID { get; set; }
        public int SUBAREA_ID { get; set; }
        public Nullable<int> CREATE_USER { get; set; }
        public Nullable<System.DateTime> CREATE_DATE { get; set; }
    
        public virtual TB_M_AREA TB_M_AREA { get; set; }
        public virtual TB_M_SUBAREA TB_M_SUBAREA { get; set; }
        public virtual TB_R_USER TB_R_USER { get; set; }
    }
}
