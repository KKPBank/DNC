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
    
    public partial class TB_T_DNC_LOAD_LIST
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TB_T_DNC_LOAD_LIST()
        {
            this.TB_T_DNC_LOAD_LIST_DATA = new HashSet<TB_T_DNC_LOAD_LIST_DATA>();
        }
    
        public int DNC_LOAD_LIST_ID { get; set; }
        public string FILE_NAME { get; set; }
        public string CONTENT_TYPE { get; set; }
        public string DOCUMENT_NAME { get; set; }
        public string DOCUMENT_DESC { get; set; }
        public string FILE_URL { get; set; }
        public System.DateTime UPLOAD_DATE { get; set; }
        public string STATUS { get; set; }
        public Nullable<System.DateTime> CREATE_DATE { get; set; }
        public Nullable<int> CREATE_USER { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public Nullable<int> UPDATE_USER { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TB_T_DNC_LOAD_LIST_DATA> TB_T_DNC_LOAD_LIST_DATA { get; set; }
    }
}
