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
    
    public partial class TB_T_SR
    {
        public TB_T_SR()
        {
            this.TB_L_SR_LOGGING = new HashSet<TB_L_SR_LOGGING>();
            this.TB_T_SR_ACTIVITY = new HashSet<TB_T_SR_ACTIVITY>();
            this.TB_T_SR_ATTACHMENT = new HashSet<TB_T_SR_ATTACHMENT>();
            this.TB_T_SR_VERIFY_RESULT_GROUP = new HashSet<TB_T_SR_VERIFY_RESULT_GROUP>();
            this.TB_T_SR_REPLY_EMAIL = new HashSet<TB_T_SR_REPLY_EMAIL>();
        }
    
        public int SR_ID { get; set; }
        public string SR_NO { get; set; }
        public string SR_CALL_ID { get; set; }
        public string SR_ANO { get; set; }
        public Nullable<int> CUSTOMER_ID { get; set; }
        public Nullable<int> ACCOUNT_ID { get; set; }
        public Nullable<int> CONTACT_ID { get; set; }
        public string CONTACT_ACCOUNT_NO { get; set; }
        public Nullable<int> CONTACT_RELATIONSHIP_ID { get; set; }
        public Nullable<int> PRODUCTGROUP_ID { get; set; }
        public Nullable<int> PRODUCT_ID { get; set; }
        public Nullable<int> CAMPAIGNSERVICE_ID { get; set; }
        public Nullable<int> AREA_ID { get; set; }
        public Nullable<int> SUBAREA_ID { get; set; }
        public Nullable<int> TYPE_ID { get; set; }
        public Nullable<int> MAP_PRODUCT_ID { get; set; }
        public Nullable<int> CHANNEL_ID { get; set; }
        public Nullable<int> MEDIA_SOURCE_ID { get; set; }
        public string SR_SUBJECT { get; set; }
        public string SR_REMARK { get; set; }
        public Nullable<int> OWNER_BRANCH_ID { get; set; }
        public Nullable<int> OWNER_USER_ID { get; set; }
        public Nullable<int> DELEGATE_BRANCH_ID { get; set; }
        public Nullable<int> DELEGATE_USER_ID { get; set; }
        public Nullable<int> OLD_OWNER_USER_ID { get; set; }
        public Nullable<int> OLD_DELEGATE_USER_ID { get; set; }
        public Nullable<int> SR_PAGE_ID { get; set; }
        public Nullable<bool> SR_IS_VERIFY { get; set; }
        public string SR_IS_VERIFY_PASS { get; set; }
        public Nullable<int> SR_STATUS_ID { get; set; }
        public Nullable<int> OLD_SR_STATUS_ID { get; set; }
        public Nullable<int> SR_DEF_ACCOUNT_ADDRESS_ID { get; set; }
        public string SR_DEF_ADDRESS_HOUSE_NO { get; set; }
        public string SR_DEF_ADDRESS_VILLAGE { get; set; }
        public string SR_DEF_ADDRESS_BUILDING { get; set; }
        public string SR_DEF_ADDRESS_FLOOR_NO { get; set; }
        public string SR_DEF_ADDRESS_ROOM_NO { get; set; }
        public string SR_DEF_ADDRESS_MOO { get; set; }
        public string SR_DEF_ADDRESS_SOI { get; set; }
        public string SR_DEF_ADDRESS_STREET { get; set; }
        public string SR_DEF_ADDRESS_TAMBOL { get; set; }
        public string SR_DEF_ADDRESS_AMPHUR { get; set; }
        public string SR_DEF_ADDRESS_PROVINCE { get; set; }
        public string SR_DEF_ADDRESS_ZIPCODE { get; set; }
        public Nullable<int> SR_AFS_ASSET_ID { get; set; }
        public string SR_AFS_ASSET_NO { get; set; }
        public string SR_AFS_ASSET_DESC { get; set; }
        public Nullable<System.DateTime> SR_NCB_CUSTOMER_BIRTHDATE { get; set; }
        public string SR_NCB_CHECK_STATUS { get; set; }
        public Nullable<int> SR_NCB_MARKETING_USER_ID { get; set; }
        public string SR_NCB_MARKETING_FULL_NAME { get; set; }
        public Nullable<int> SR_NCB_MARKETING_BRANCH_ID { get; set; }
        public string SR_NCB_MARKETING_BRANCH_NAME { get; set; }
        public Nullable<int> SR_NCB_MARKETING_BRANCH_UPPER_1_ID { get; set; }
        public string SR_NCB_MARKETING_BRANCH_UPPER_1_NAME { get; set; }
        public Nullable<int> SR_NCB_MARKETING_BRANCH_UPPER_2_ID { get; set; }
        public string SR_NCB_MARKETING_BRANCH_UPPER_2_NAME { get; set; }
        public Nullable<int> CREATE_BRANCH_ID { get; set; }
        public Nullable<int> CREATE_USER { get; set; }
        public Nullable<int> UPDATE_USER { get; set; }
        public Nullable<System.DateTime> CREATE_DATE { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public Nullable<System.DateTime> CLOSE_DATE { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE_BY_OWNER { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE_BY_DELEGATE { get; set; }
        public string RULE_DELEGATE_FLAG { get; set; }
        public Nullable<int> RULE_DELEGATE_BRANCH_ID { get; set; }
        public Nullable<int> RULE_THIS_ALERT { get; set; }
        public Nullable<int> RULE_TOTAL_ALERT { get; set; }
        public Nullable<int> RULE_THIS_WORK { get; set; }
        public Nullable<int> RULE_TOTAL_WORK { get; set; }
        public Nullable<System.DateTime> RULE_NEXT_SLA { get; set; }
        public string RULE_ASSIGN_FLAG { get; set; }
        public string RULE_EMAIL_FLAG { get; set; }
        public Nullable<System.DateTime> RULE_ASSIGN_DATE { get; set; }
        public Nullable<System.DateTime> RULE_STATUS_DATE { get; set; }
        public Nullable<System.DateTime> RULE_DELEGATE_DATE { get; set; }
        public Nullable<System.DateTime> RULE_CURRENT_SLA { get; set; }
        public Nullable<int> DRAFT_SR_EMAIL_TEMPLATE_ID { get; set; }
        public string DRAFT_MAIL_SENDER { get; set; }
        public string DRAFT_MAIL_TO { get; set; }
        public string DRAFT_MAIL_CC { get; set; }
        public string DRAFT_MAIL_SUBJECT { get; set; }
        public string DRAFT_MAIL_BODY { get; set; }
        public Nullable<int> DRAFT_ACTIVITY_TYPE_ID { get; set; }
        public string DRAFT_ACTIVITY_DESC { get; set; }
        public string DRAFT_ACCOUNT_ADDRESS_TEXT { get; set; }
        public Nullable<bool> DRAFT_IS_SEND_EMAIL_FOR_DELEGATE { get; set; }
        public Nullable<bool> DRAFT_IS_CLOSE { get; set; }
        public string DRAFT_ATTACHMENT_JSON { get; set; }
        public string DRAFT_VERIFY_ANSWER_JSON { get; set; }
        public Nullable<System.DateTime> EXPORT_DATE { get; set; }
        public Nullable<int> CPN_PRODUCT_GROUP_ID { get; set; }
        public Nullable<int> CPN_PRODUCT_ID { get; set; }
        public Nullable<int> CPN_CAMPAIGNSERVICE_ID { get; set; }
        public Nullable<int> CPN_SUBJECT_ID { get; set; }
        public Nullable<int> CPN_TYPE_ID { get; set; }
        public Nullable<int> CPN_ROOT_CAUSE_ID { get; set; }
        public Nullable<int> CPN_ISSUES_ID { get; set; }
        public Nullable<int> CPN_MAPPING_ID { get; set; }
        public Nullable<int> CPN_BU_GROUP_ID { get; set; }
        public Nullable<int> CPN_CAUSE_SUMMARY_ID { get; set; }
        public Nullable<int> CPN_SUMMARY_ID { get; set; }
        public Nullable<bool> CPN_SECRET { get; set; }
        public Nullable<bool> CPN_CAR { get; set; }
        public Nullable<bool> CPN_HPLog100 { get; set; }
        public Nullable<bool> CPN_SUMMARY { get; set; }
        public Nullable<bool> CPN_CAUSE_CUSTOMER { get; set; }
        public Nullable<bool> CPN_CAUSE_STAFF { get; set; }
        public Nullable<bool> CPN_CAUSE_SYSTEM { get; set; }
        public Nullable<bool> CPN_CAUSE_PROCESS { get; set; }
        public string CPN_CAUSE_CUSTOMER_DETAIL { get; set; }
        public string CPN_CAUSE_STAFF_DETAIL { get; set; }
        public string CPN_CAUSE_SYSTEM_DETAIL { get; set; }
        public string CPN_CAUSE_PROCESS_DETAIL { get; set; }
        public string CPN_FIXED_DETAIL { get; set; }
        public string DRAFT_MAIL_BCC { get; set; }
        public Nullable<int> CLOSE_USER { get; set; }
        public string CLOSE_USERNAME { get; set; }
        public string CPN_BU1_CODE { get; set; }
        public string CPN_BU2_CODE { get; set; }
        public string CPN_BU3_CODE { get; set; }
        public Nullable<int> CPN_MSHBranchId { get; set; }
        public string UPDATE_USERNAME { get; set; }
    
        public virtual TB_C_SR_ACTIVITY_TYPE TB_C_SR_ACTIVITY_TYPE { get; set; }
        public virtual TB_C_SR_EMAIL_TEMPLATE TB_C_SR_EMAIL_TEMPLATE { get; set; }
        public virtual TB_C_SR_PAGE TB_C_SR_PAGE { get; set; }
        public virtual TB_C_SR_STATUS TB_C_SR_STATUS { get; set; }
        public virtual ICollection<TB_L_SR_LOGGING> TB_L_SR_LOGGING { get; set; }
        public virtual TB_M_ACCOUNT TB_M_ACCOUNT { get; set; }
        public virtual TB_M_ACCOUNT_ADDRESS TB_M_ACCOUNT_ADDRESS { get; set; }
        public virtual TB_M_AFS_ASSET TB_M_AFS_ASSET { get; set; }
        public virtual TB_M_AREA TB_M_AREA { get; set; }
        public virtual TB_M_CONTACT TB_M_CONTACT { get; set; }
        public virtual TB_M_CUSTOMER TB_M_CUSTOMER { get; set; }
        public virtual TB_M_MAP_PRODUCT TB_M_MAP_PRODUCT { get; set; }
        public virtual TB_M_MEDIA_SOURCE TB_M_MEDIA_SOURCE { get; set; }
        public virtual TB_M_RELATIONSHIP TB_M_RELATIONSHIP { get; set; }
        public virtual TB_M_SUBAREA TB_M_SUBAREA { get; set; }
        public virtual TB_M_TYPE TB_M_TYPE { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH1 { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH2 { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH3 { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH4 { get; set; }
        public virtual TB_R_BRANCH TB_R_BRANCH5 { get; set; }
        public virtual TB_R_CAMPAIGNSERVICE TB_R_CAMPAIGNSERVICE { get; set; }
        public virtual TB_R_CHANNEL TB_R_CHANNEL { get; set; }
        public virtual TB_R_PRODUCT TB_R_PRODUCT { get; set; }
        public virtual TB_R_PRODUCTGROUP TB_R_PRODUCTGROUP { get; set; }
        public virtual TB_R_USER TB_R_USER { get; set; }
        public virtual TB_R_USER TB_R_USER1 { get; set; }
        public virtual TB_R_USER TB_R_USER2 { get; set; }
        public virtual TB_R_USER TB_R_USER3 { get; set; }
        public virtual TB_R_USER TB_R_USER4 { get; set; }
        public virtual TB_R_USER TB_R_USER5 { get; set; }
        public virtual TB_R_USER TB_R_USER6 { get; set; }
        public virtual ICollection<TB_T_SR_ACTIVITY> TB_T_SR_ACTIVITY { get; set; }
        public virtual ICollection<TB_T_SR_ATTACHMENT> TB_T_SR_ATTACHMENT { get; set; }
        public virtual ICollection<TB_T_SR_VERIFY_RESULT_GROUP> TB_T_SR_VERIFY_RESULT_GROUP { get; set; }
        public virtual ICollection<TB_T_SR_REPLY_EMAIL> TB_T_SR_REPLY_EMAIL { get; set; }
    }
}
