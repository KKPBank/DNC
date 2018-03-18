using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSM.Service.Messages.Common;
using System.ServiceModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSM.Common.Utilities;
using CSM.Common.Resources;

namespace CSM.Service.Messages.DoNotCall
{
    class InsertOrUpdateDoNotCallCustomer
    {
    }

    public class InsertOrUpdateDoNotCallCustomerRequest : DoNotCallTransactionInput, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();
            // Required fields
            if (string.IsNullOrWhiteSpace(UpdateUser))
                result.Add(GetRequiredError(nameof(UpdateUser)));
            if (!UpdateDate.HasValue)
                result.Add(GetRequiredError(nameof(UpdateDate)));
            if (string.IsNullOrWhiteSpace(SubscriptTypeCode))
                result.Add(GetRequiredError(nameof(SubscriptTypeCode)));
            // Card No
            if (string.IsNullOrWhiteSpace(CardNo))
                result.Add(GetRequiredError(nameof(CardNo)));
            else if (!IsValidCardFormat())
                result.Add(new ValidationResult("Invalid Personal card information."));
            // FirstName
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                result.Add(GetRequiredError(nameof(FirstName)));
            }
            else if (FirstName.Length > Constants.MaxLength.FirstName)
            {
                result.Add(GetTooLongError(nameof(FirstName), Constants.MaxLength.FirstName));
            }
            // LastName
            if (!string.IsNullOrWhiteSpace(LastName) && LastName.Length > Constants.MaxLength.LastName)
            {
                result.Add(GetTooLongError(nameof(LastName), Constants.MaxLength.LastName));
            }

            bool? isActive;
            // ExpireDate
            if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Today)
                result.Add(new ValidationResult("ExpiryDate is before today"));
            if (!IsValidTrueFalseDigit(Status, out isActive))
                result.Add(GetInvalidTrueFalseDigitFormat(nameof(Status)));
            // System Code
            if (SystemCode.GetCleanString().Length > Constants.MaxLength.SystemCode)
                result.Add(GetTooLongError(nameof(SystemCode), Constants.MaxLength.SystemCode));
            // Remark
            if (Remark.GetCleanString().Length > Constants.MaxLength.RemarkDoNotCall)
                result.Add(GetTooLongError(nameof(SystemCode), Constants.MaxLength.RemarkDoNotCall));

            bool validBlockInfo = true;
            bool? isBlockSalesEmail = null;
            bool? isBlockSalesSms = null;
            bool? isBlockSalesTelephone = null;
            bool? isBlockInfoEmail = null;
            bool? isBlockInfoSms = null;
            bool? isBlockInfoTelephone = null;
            // Sales
            if (SalesBlockInfo == null)
            {
                validBlockInfo = false;
                result.Add(GetRequiredError(nameof(SalesBlockInfo)));
            }
            else
            {
                if (!IsValidTrueFalseDigit(SalesBlockInfo.BlockEmail, out isBlockSalesEmail))
                {
                    validBlockInfo = false;
                    result.Add(GetInvalidTrueFalseDigitFormat(nameof(SalesBlockInfo.BlockEmail)));
                }
                if (!IsValidTrueFalseDigit(SalesBlockInfo.BlockSMS, out isBlockSalesSms))
                {
                    validBlockInfo = false;
                    result.Add(GetInvalidTrueFalseDigitFormat(nameof(SalesBlockInfo.BlockSMS)));
                }
                if (!IsValidTrueFalseDigit(SalesBlockInfo.BlockTelephone, out isBlockSalesTelephone))
                {
                    validBlockInfo = false;
                    result.Add(GetInvalidTrueFalseDigitFormat(nameof(SalesBlockInfo.BlockTelephone)));
                }
                else
                {
                    var productList = SalesBlockInfo.BlockProducts;
                    if(productList != null && productList.Count > 0)
                    {
                        List<string> duplicatedItems = productList.Where(x => x.IsActive == Constants.DigitTrue)
                                                                  .GroupBy(x => x.ProductCode.GetCleanString())
                                                                  .Where(group => group.Count() > 1)
                                                                  .Select(x => x.Key).ToList();

                        if (duplicatedItems != null && duplicatedItems.Count > 0)
                        {
                            result.Add(new ValidationResult($"Duplicated product code: {string.Join(",", duplicatedItems)}"));
                        }
                        else
                        {
                            foreach (var product in productList)
                            {
                                ValidateProductListItem(result, product);
                            }
                        }
                    }
                }
            }
            // Information
            if (InformationBlockInfo == null)
            {
                validBlockInfo = false;
                result.Add(GetRequiredError(nameof(InformationBlockInfo)));
            }
            else
            {
                if (!IsValidTrueFalseDigit(InformationBlockInfo.BlockEmail, out isBlockInfoEmail))
                {
                    validBlockInfo = false;
                    result.Add(GetInvalidTrueFalseDigitFormat(nameof(InformationBlockInfo.BlockEmail)));
                }
                if (!IsValidTrueFalseDigit(InformationBlockInfo.BlockSMS, out isBlockInfoSms))
                {
                    validBlockInfo = false;
                    result.Add(GetInvalidTrueFalseDigitFormat(nameof(InformationBlockInfo.BlockSMS)));
                }
                if (!IsValidTrueFalseDigit(InformationBlockInfo.BlockTelephone, out isBlockInfoTelephone))
                {
                    validBlockInfo = false;
                    result.Add(GetInvalidTrueFalseDigitFormat(nameof(InformationBlockInfo.BlockTelephone)));
                }
                else
                {
                    var productList = InformationBlockInfo.BlockProducts;
                    if(productList != null && productList.Count > 0)
                    {
                        List<string> duplicatedItems = productList.Where(x => x.IsActive == Constants.DigitTrue)
                                                                  .GroupBy(x => x.ProductCode.GetCleanString())
                                                                  .Where(group => group.Count() > 1)
                                                                  .Select(x => x.Key).ToList();

                        if (duplicatedItems != null && duplicatedItems.Count > 0)
                        {
                            result.Add(new ValidationResult($"Duplicated product code: {string.Join(",", duplicatedItems)}"));
                        }
                        else
                        {
                            foreach (var product in productList)
                            {
                                ValidateProductListItem(result, product);
                            }
                        }
                    }
                }
            }

            if (validBlockInfo) // every block sales/information item is valid
            {
                //Telephone List
                if (isBlockSalesTelephone.Value || isBlockInfoTelephone.Value || isBlockSalesSms.Value || isBlockInfoSms.Value)
                {
                    if (TelephoneList.Count == 0)
                    {
                        result.Add(GetRequiredError(nameof(TelephoneList)));
                    }
                    else
                    {
                        List<string> duplicatedItems = TelephoneList.GroupBy(x => x.PhoneNo.GetCleanString()).Where(group => group.Count() > 1).Select(x => x.Key).ToList();
                        if (duplicatedItems != null && duplicatedItems.Count > 0)
                        {
                            result.Add(new ValidationResult($"Duplicated Telephone: {string.Join(",", duplicatedItems)}"));
                        }
                        else
                        {
                            foreach (var phone in TelephoneList)
                            {
                                string phoneNo = phone.PhoneNo;
                                bool hasPhoneNo = !string.IsNullOrWhiteSpace(phoneNo);
                                if (hasPhoneNo)
                                {
                                    // validate format
                                    string cleanPhoneNo = phoneNo.GetCleanString();
                                    int phoneNoLength = cleanPhoneNo.Length;
                                    int maxPhoneLength = Constants.MaxLength.DoNotCallPhoneNo;
                                    int minPhoneLength = Constants.MinLenght.PhoneNo;

                                    if (phoneNoLength < minPhoneLength || phoneNoLength > maxPhoneLength || cleanPhoneNo.Any(c => !char.IsDigit(c)))
                                    {
                                        result.Add(new ValidationResult($"Telephone ต้องระบุเป็นตัวเลข {minPhoneLength}-{maxPhoneLength} หลักเท่านั้น"));
                                        break;
                                    }
                                }
                                else
                                {
                                    result.Add(new ValidationResult("มี Telephone เป็นค่าว่าง"));
                                    break;
                                }
                                bool? isActivePhone;
                                string status = phone.IsActive;
                                if (string.IsNullOrWhiteSpace(status) || !IsValidYesNoString(status, out isActivePhone))
                                    result.Add(GetInvalidTrueFalseDigitFormat("PhoneNoStatus"));
                            }
                        }
                    }
                }

                //Email List
                if (isBlockInfoEmail.Value || isBlockSalesEmail.Value)
                {
                    if (EmailList.Count == 0)
                    {
                        result.Add(GetRequiredError(nameof(EmailList)));
                    }
                    else
                    {
                        List<string> duplicatedItems = EmailList.GroupBy(x => x.Email.GetCleanString()).Where(group => group.Count() > 1).Select(x => x.Key).ToList();
                        if (duplicatedItems != null && duplicatedItems.Count > 0)
                        {
                            result.Add(new ValidationResult($"Duplicated Email: {string.Join(",", duplicatedItems)}"));
                        }
                        else
                        {
                            foreach (var item in EmailList)
                            {
                                var email = item.Email;
                                bool hasEmail = !string.IsNullOrWhiteSpace(email);
                                if (hasEmail)
                                {
                                    // validate format
                                    string cleanEmail = email.GetCleanString();
                                    var emailChecker = new EmailAddressAttribute();
                                    bool isValid = emailChecker.IsValid(cleanEmail);
                                    if (!isValid)
                                    {
                                        result.Add(new ValidationResult("รูปแบบ email ไม่ถูกต้อง"));
                                        break;
                                    }
                                    else if (email.Length > Constants.MaxLength.Email)
                                    {
                                        result.Add(new ValidationResult($"ความยาว Email ต้องไม่เกิน {Constants.MaxLength.Email} ตัวอักษร"));
                                        break;
                                    }
                                }
                                else
                                {
                                    result.Add(new ValidationResult("มี Email เป็นค่าว่าง"));
                                    break;
                                }
                                bool? isActiveEmail;
                                string status = item.IsActive;
                                if (string.IsNullOrWhiteSpace(status) || !IsValidYesNoString(status, out isActiveEmail))
                                    result.Add(GetInvalidTrueFalseDigitFormat("EmailStatus"));
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    public class InsertOrUpdateDoNotCallCustomerResponse
    {
        public InsertOrUpdateDoNotCallCustomerResponse()
        {
            ResponseStatusInfo = new InsertResponseStatusInfo();
        }

        [MessageHeader]
        public Header Header { get; set; }

        [MessageBodyMember]
        public InsertResponseStatusInfo ResponseStatusInfo { get; set; }
    }
}
