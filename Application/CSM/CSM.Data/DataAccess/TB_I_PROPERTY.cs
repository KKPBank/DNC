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
    
    public partial class TB_I_PROPERTY
    {
        public int PROPERTY_ID { get; set; }
        public Nullable<int> EMPLOYEE_ID { get; set; }
        public Nullable<int> ROW_ID { get; set; }
        public string IF_ROW_STAT { get; set; }
        public string IF_ROW_BATCH_NUM { get; set; }
        public string AST_ASSET_NUM { get; set; }
        public string AST_TYPE_CD { get; set; }
        public string AST_TRADEINTYPE_CD { get; set; }
        public string AST_STATUS_CD { get; set; }
        public string AST_DESC_TEXT { get; set; }
        public string AST_NAME { get; set; }
        public string AST_COMMENTS { get; set; }
        public string AST_REF_NUMBER_1 { get; set; }
        public string AST_LOT_NUM { get; set; }
        public string AST_PURCH_LOC_DESC { get; set; }
        public string SALE_NAME { get; set; }
        public string PHONE_NO { get; set; }
        public string MOBILE_NO { get; set; }
        public string EMAIL { get; set; }
        public string ERROR { get; set; }
    
        public virtual TB_R_USER TB_R_USER { get; set; }
    }
}
