using CSM.Common.Resources;
using CSM.Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel;
using CSM.Service.Messages.Common;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Service.Messages.DoNotCall
{
    public class DoNotCallInterfaceInputModel
    {

    }

    public class DoNotCallActivityProductInput
    {
        public string ProductCode { get; set; }
        public int ActivityProductId { get; set; }
        public int? ProductId { get; set; }
        /// <summary>
        /// Information: "I", Sales: "S"
        /// </summary>
        public string BlockType { get; set; }
        /// <summary>
        /// True: "Y", False: "N"
        /// </summary>
        public string IsActive { get; set; }

        public bool IsDeleted => IsActive == Constants.FlagN;
    }

    public class DoNotCallInterfaceValidateResult
    {
        public DoNotCallInterfaceValidateResult()
        {
            ActivityProducts = new List<DoNotCallActivityProductInput>();
            IsValid = false;
            IsBlockAllInfoProducts = false;
            IsBlockAllSalesProducts = false;
        }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public List<DoNotCallActivityProductInput> ActivityProducts { get; set; }
        public int? TransactionId { get; set; }
        public int? SubscriptionTypeId { get; set; }
        public bool IsBlockAllSalesProducts { get; set; }
        public bool IsBlockAllInfoProducts { get; set; }
    }

    public class InformationBlockInfoModel
    {
        public string BlockTelephone { get; set; }
        public string BlockSMS { get; set; }
        public string BlockEmail { get; set; }
        public List<BlockProduct> BlockProducts { get; set; }
    }

    public class SalesBlockInfoModel
    {
        public string BlockTelephone { get; set; }
        public string BlockSMS { get; set; }
        public string BlockEmail { get; set; }
        public List<BlockProduct> BlockProducts { get; set; }
    }

    public class BlockPhoneNo
    {
        public string PhoneNo { get; set; }
        /// <summary>
        /// True: "Y", False: "N"
        /// </summary>
        public string IsActive { get; set; }
        public bool IsDeleted => IsActive == Constants.FlagN;

        public int? Id { get; set; }
    }

    public class BlockEmail
    {
        public string Email { get; set; }
        public int? Id { get; set; }
        /// <summary>
        /// True: "Y", False: "N"
        /// </summary>
        public string IsActive { get; set; }
        public bool IsDeleted => IsActive == Constants.FlagN;
    }

    public class BlockProduct
    {
        public string ProductCode { get; set; }
        /// <summary>
        /// True: "Y", False: "N"
        /// </summary>
        public string IsActive { get; set; }
        public bool IsDeleted => IsActive == Constants.FlagN;
    }

    public class InsertResponseStatusInfo : BaseStatusResponse
    {
        public int TransactionId { get; set; }
    }

    public class DoNotCallTransactionInput
    {
        [MessageHeader]
        public Header Header { get; set; }

        [MessageBodyMember]
        public string CardNo { get; set; }
        [MessageBodyMember]
        public string SubscriptTypeCode { get; set; }
        [MessageBodyMember]
        public string FirstName { get; set; }
        [MessageBodyMember]
        public string LastName { get; set; }
        [MessageBodyMember]
        public DateTime? ExpiryDate { get; set; }
        [MessageBodyMember]
        public string Status { get; set; }
        [MessageBodyMember]
        public string SystemCode { get; set; }
        [MessageBodyMember]
        public int? CisId { get; set; }
        [MessageBodyMember]
        public string Remark { get; set; }
        [MessageBodyMember]
        public string UpdateUser { get; set; } // Username
        [MessageBodyMember]
        public DateTime? UpdateDate { get; set; }
        [MessageBodyMember]
        public SalesBlockInfoModel SalesBlockInfo { get; set; }
        [MessageBodyMember]
        public InformationBlockInfoModel InformationBlockInfo { get; set; }
        [MessageBodyMember]
        public List<BlockPhoneNo> TelephoneList { get; set; }
        [MessageBodyMember]
        public List<BlockEmail> EmailList { get; set; }

        
        internal void ValidateProductListItem(List<ValidationResult> result, BlockProduct product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductCode))
                result.Add(GetRequiredError("ProductCode"));

            bool? isActiveProduct;
            string productStatus = product.IsActive;
            if (string.IsNullOrWhiteSpace(productStatus) || !IsValidYesNoString(product.IsActive, out isActiveProduct))
                result.Add(GetInvalidTrueFalseDigitFormat("ProductStatus"));
        }

        internal bool IsValidCardFormat()
        {
            bool isPersonCardType = SubscriptTypeCode == Constants.SubscriptTypeCode.Personal;
            bool isValidPersonCardNo = string.IsNullOrWhiteSpace(CardNo) || ApplicationHelpers.ValidateCardNo(CardNo);
            bool valid = !isPersonCardType || isValidPersonCardNo;
            return valid;
        }

        internal bool IsValidYesNoString(string input, out bool? isTrue)
        {
            string cleanInput = input.GetCleanString();

            bool isTrueResult = cleanInput.Equals(Constants.FlagY, StringComparison.InvariantCultureIgnoreCase);
            bool isFalseResult = cleanInput.Equals(Constants.FlagN, StringComparison.InvariantCultureIgnoreCase);
            var valid = isTrueResult || isFalseResult;
            if (valid)
                isTrue = isTrueResult;
            else
                isTrue = null;

            return valid;
        }

        internal bool IsValidTrueFalseDigit(string digit, out bool? isTrue)
        {
            string cleanDigit = digit.GetCleanString();
            bool isTrueResult = digit == Constants.DigitTrue;
            bool isFalseResult = digit == Constants.DigitFalse;
            var valid = isTrueResult || isFalseResult;
            if (valid)
                isTrue = isTrueResult;
            else
                isTrue = null;

            return valid;
        }

        internal ValidationResult GetRequiredError(string fieldName)
        {
            return new ValidationResult($"Missing {fieldName}");
        }

        internal ValidationResult GetTooLongError(string fieldName, int maxLength)
        {
            return new ValidationResult($"{fieldName} exceeded {maxLength} characters");
        }

        internal ValidationResult GetInvalidTrueFalseDigitFormat(string fieldName)
        {
            return new ValidationResult($"Invalid {fieldName} data");
        }
    }

}
