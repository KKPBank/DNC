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
    
    public partial class TB_T_SR_ACTIVITY
    {
        public TB_T_SR_ACTIVITY()
        {
            this.TB_T_SR_ATTACHMENT = new HashSet<TB_T_SR_ATTACHMENT>();
            this.TB_T_SR_PREPARE_EMAIL = new HashSet<TB_T_SR_PREPARE_EMAIL>();
        }
    
        public int SR_ACTIVITY_ID { get; set; }
        public int SR_ID { get; set; }
        public Nullable<int> OWNER_BRANCH_ID { get; set; }
        public Nullable<int> OWNER_USER_ID { get; set; }
        public Nullable<int> SR_STATUS_ID { get; set; }
        public Nullable<int> DELEGATE_BRANCH_ID { get; set; }
        public Nullable<int> DELEGATE_USER_ID { get; set; }
        public Nullable<int> OLD_OWNER_USER_ID { get; set; }
        public Nullable<int> OLD_DELEGATE_USER_ID { get; set; }
        public Nullable<int> OLD_SR_STATUS_ID { get; set; }
        public Nullable<bool> IS_SEND_DELEGATE_EMAIL { get; set; }
        public string SR_ACTIVITY_DESC { get; set; }
        public Nullable<int> SR_EMAIL_TEMPLATE_ID { get; set; }
        public string SR_ACTIVITY_EMAIL_SENDER { get; set; }
        public string SR_ACTIVITY_EMAIL_TO { get; set; }
        public string SR_ACTIVITY_EMAIL_CC { get; set; }
        public string SR_ACTIVITY_EMAIL_SUBJECT { get; set; }
        public string SR_ACTIVITY_EMAIL_BODY { get; set; }
        public string SR_ACTIVITY_EMAIL_ATTACHMENTS { get; set; }
        public Nullable<int> SR_ACTIVITY_TYPE_ID { get; set; }
        public Nullable<int> CREATE_BRANCH_ID { get; set; }
        public Nullable<int> CREATE_USER { get; set; }
        public Nullable<System.DateTime> CREATE_DATE { get; set; }
        public Nullable<short> ACTIVITY_CAR_SUBMIT_STATUS { get; set; }
        public Nullable<System.DateTime> ACTIVITY_CAR_SUBMIT_DATE { get; set; }
        public string ACTIVITY_CAR_SUBMIT_ERROR_CODE { get; set; }
        public string ACTIVITY_CAR_SUBMIT_ERROR_DESC { get; set; }
        public Nullable<short> ACTIVITY_HP_SUBMIT_STATUS { get; set; }
        public Nullable<System.DateTime> ACTIVITY_HP_SUBMIT_DATE { get; set; }
        public string ACTIVITY_HP_SUBMIT_ERROR_CODE { get; set; }
        public string ACTIVITY_HP_SUBMIT_ERROR_DESC { get; set; }
        public string CREATE_USERNAME { get; set; }
        public Nullable<System.DateTime> EXPORT_DATE { get; set; }
        public string SR_ACTIVITY_EMAIL_BCC { get; set; }
        public Nullable<bool> IS_SECRET { get; set; }
        public Nullable<bool> IS_SEND_CAR { get; set; }
        public Nullable<int> WORKING_MINUTE { get; set; }
    
        public virtual TB_C_SR_ACTIVITY_TYPE TB_C_SR_ACTIVITY_TYPE { get; set; }
        public virtual TB_C_SR_EMAIL_TEMPLATE TB_C_SR_EMAIL_TEMPLATE { get; set; }
        public virtual TB_C_SR_STATUS TB_C_SR_STATUS { get; set; }
        public virtual TB_C_SR_STATUS TB_C_SR_STATUS1 { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH1 { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH2 { get; set; }
        public virtual TB_R_USER TB_R_USER { get; set; }
        public virtual TB_R_USER TB_R_USER1 { get; set; }
        public virtual ICollection<TB_T_SR_ATTACHMENT> TB_T_SR_ATTACHMENT { get; set; }
        public virtual ICollection<TB_T_SR_PREPARE_EMAIL> TB_T_SR_PREPARE_EMAIL { get; set; }
        public virtual TB_T_SR TB_T_SR { get; set; }
    }
}
