using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CSM.Service.Messages.Common;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Service.Messages.DoNotCall
{
    public class InquireDoNotCallResponse
    {
        public InquireDoNotCallResponse()
        {
            ResponseStatusInfo = new InquireResponsStatusInfo();
            TransactionList = new List<TransactionInfo>();
        }

        [MessageHeader]
        public Header Header { get; set; }

        [MessageBodyMember]
        public InquireResponsStatusInfo ResponseStatusInfo { get; set; }
        [MessageBodyMember]
        public List<TransactionInfo> TransactionList { get; set; }
    }

    public class TransactionInfo
    {
        public TransactionInfo()
        {
            SalesBlockProductList = new List<ProductInfo>();
            InformationBlockProductList = new List<ProductInfo>();
            TelephoneList = new List<TelelphoneInfo>();
            EmailList = new List<EmailInfo>();
        }

        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? SubscriptTypeId { get; set; }
        public string SubscriptTypeName { get; set; }
        public string CardNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string SystemCode { get; set; }
        public decimal? CisId { get; set; }
        public string BlockLevelCode { get; set; }
        public string BlockLevelDesc { get; set; }
        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }
        public string SalesBlockTelephone { get; set; }
        public string SalesBlockSMS { get; set; }
        public string SalesBlockEmail { get; set; }
        public string SalesBlockAllProduct { get; set; }
        public List<ProductInfo> SalesBlockProductList { get; set; }
        public string InformationBlockTelephone { get; set; }
        public string InformationBlockSMS { get; set; }
        public string InformationBlockEmail { get; set; }
        public string InformationBlockAllProduct { get; set; }
        public List<ProductInfo> InformationBlockProductList { get; set; }
        public List<TelelphoneInfo> TelephoneList { get; set; }
        public List<EmailInfo> EmailList { get; set; }
    }

    public class ProductInfo
    {
        public int ActivityProductId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ActiveStatus { get; set; }
    }

    public class EmailInfo
    {
        public string Email { get; set; }
        public string ActiveStatus { get; set; }
    }

    public class TelelphoneInfo
    {
        public string PhoneNo { get; set; }
        public string ActiveStatus { get; set; }
    }

    public class InquireResponsStatusInfo: BaseStatusResponse
    {
        public int RecordFound { get; set; }
    }
}
