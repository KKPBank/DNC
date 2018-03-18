using CSM.Business;
using CSM.Business.Interfaces;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Service.Messages.DoNotCall;
using log4net;
using System;
using System.Collections.Generic;
using CSM.Service.Messages.Common;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace DoNotCallWCFService
{
    public class DoNotCallInterfaceService : IDoNotCallInterfaceService
    {
        private readonly ILog _logger;
        private IDoNotCallFacade _doNotCallFacade;
        private ICommonFacade _commonFacade;
        private IUserFacade _userFacade;
        private IProductFacade _productFacade;
        private LogMessageBuilder _logMsg = new LogMessageBuilder();

        public DoNotCallInterfaceService()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                // Set logfile name and application name variables
                GlobalContext.Properties["ApplicationCode"] = "DNCWS";
                GlobalContext.Properties["ServerName"] = Environment.MachineName;
                _logger = LogManager.GetLogger(typeof(DoNotCallInterfaceService));
            }
            catch (Exception ex)
            {
                _logger.Error("Exception occur:\n", ex);
            }
        }

        // Search
        public InquireDoNotCallResponse InquiryDoNotCallList(InquireDoNotCallRequest request)
        {
            string methodName = $"{nameof(DoNotCallInterfaceService)}.{nameof(InquiryDoNotCallList)}";
            var response = new InquireDoNotCallResponse();
            try
            {
                // Validate input data
                string errorMessage;
                bool validInput = ValidateInquireDoNotCallRequest(request, out errorMessage);
                if (!validInput)
                {
                    response.ResponseStatusInfo = GetInvalidInputResponse<InquireResponsStatusInfo>(errorMessage);
                    return response;
                }

                response.Header = GetResponseHeader(request.Header, methodName);

                // Authenticate user
                if (!ValidateServiceRequest(request.Header))
                {
                    response.ResponseStatusInfo = GetInvalidLoginResponse<InquireResponsStatusInfo>();
                    return response;
                }
                _logger.DebugFormat("-- XMLRequest --\n{0}", request.SerializeObject());

                // Get data
                response.TransactionList = SearchDoNotCallTransaction(request);
                response.ResponseStatusInfo = GetSuccessResponseStatusInfo<InquireResponsStatusInfo>();
                response.ResponseStatusInfo.RecordFound = response.TransactionList.Count;

                return response;
            }
            catch (Exception ex)
            {
                response.ResponseStatusInfo = GetErrorResponse<InquireResponsStatusInfo>(ex);
                return response;
            }
        }

        public InsertOrUpdateDoNotCallPhoneResponse InsertOrUpdateDoNotCallPhone(InsertOrUpdateDoNotCallPhoneRequest request)
        {
            string methodName = $"{nameof(DoNotCallInterfaceService)}.{nameof(InsertOrUpdateDoNotCallPhone)}";
            var response = new InsertOrUpdateDoNotCallPhoneResponse();
            bool success = false;
            string detail = string.Empty;

            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                // Validate input data
                DoNotCallInterfaceValidateResult validateResult = ValidateInsertUpdatePhoneRequest(request);
                if (!validateResult.IsValid)
                {
                    response.ResponseStatusInfo = GetInvalidInputResponse<InsertResponseStatusInfo>(validateResult.ErrorMessage);
                    _logger.DebugFormat("-- XMLResponse --\n{0}", response.SerializeObject());
                    detail = validateResult.ErrorMessage;
                    return response;
                }

                response.Header = GetResponseHeader(request.Header, methodName);

                // Authenticate user
                if (!ValidateServiceRequest(request.Header))
                {
                    response.ResponseStatusInfo = GetInvalidLoginResponse<InsertResponseStatusInfo>();
                    detail = "Invalid login";
                    return response;
                }
                _logger.DebugFormat("-- XMLRequest --\n{0}", request.SerializeObject());

                // Map model to insert
                DoNotCallByTelephoneEntity entity = MapTelephoneEntity(request, validateResult);
                // Insert data

                int transactionId = _doNotCallFacade.SavePhone(entity);
                if (transactionId > 0)
                {
                    response.ResponseStatusInfo = GetSuccessResponseStatusInfo<InsertResponseStatusInfo>();
                    response.ResponseStatusInfo.TransactionId = transactionId;
                    success = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.ResponseStatusInfo = GetErrorResponse<InsertResponseStatusInfo>(ex);
                detail = ex.Message;
                return response;
            }
            finally
            {
                InsertAuditLog(methodName, success, detail ?? response.ResponseStatusInfo.ResponseMessage);
            }
        }

        // Insert or Update
        public InsertOrUpdateDoNotCallCustomerResponse InsertOrUpdateDoNotCallCustomer(InsertOrUpdateDoNotCallCustomerRequest request)
        {
            string methodName = $"{nameof(DoNotCallInterfaceService)}.{nameof(InsertOrUpdateDoNotCallCustomer)}";
            var response = new InsertOrUpdateDoNotCallCustomerResponse();
            bool success = false;
            string detail = string.Empty;

            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                // Validate input data
                DoNotCallInterfaceValidateResult validateResult = ValidateInsertUpdateCustomerRequest(request);
                if (!validateResult.IsValid)
                {
                    response.ResponseStatusInfo = GetInvalidInputResponse<InsertResponseStatusInfo>(validateResult.ErrorMessage);
                    _logger.DebugFormat("-- XMLResponse --\n{0}", response.SerializeObject());
                    detail = validateResult.ErrorMessage;
                    return response;
                }

                response.Header = GetResponseHeader(request.Header, methodName);

                // Authenticate user
                if (!ValidateServiceRequest(request.Header))
                {
                    response.ResponseStatusInfo = GetInvalidLoginResponse<InsertResponseStatusInfo>();
                    detail = "Invalid login";
                    return response;
                }
                _logger.DebugFormat("-- XMLRequest --\n{0}", request.SerializeObject());

                // Map model to insert
                DoNotCallByCustomerEntity entity = MapCustomerEntity(request, validateResult);
                // Insert data

                int transactionId = _doNotCallFacade.SaveCustomerFromInterface(entity);
                if (transactionId > 0)
                {
                    response.ResponseStatusInfo = GetSuccessResponseStatusInfo<InsertResponseStatusInfo>();
                    response.ResponseStatusInfo.TransactionId = transactionId;
                    success = true;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.ResponseStatusInfo = GetErrorResponse<InsertResponseStatusInfo>(ex);
                detail = ex.Message;
                return response;
            }
            finally
            {
                InsertAuditLog(methodName, success, detail ?? response.ResponseStatusInfo.ResponseMessage);
            }
        }

        #region Private

        private void InsertAuditLog(string action, bool success, string detail = null)
        {
            var auditLog = new AuditLogEntity
            {
                Module = "DoNotCall",
                Action = action,
                IpAddress = ApplicationHelpers.GetClientIP(),
                Status = success ? LogStatus.Success : LogStatus.Fail,
                Detail = detail,
            };
            AppLog.AuditLog(auditLog);
        }

        private DoNotCallByTelephoneEntity MapTelephoneEntity<T>(T request, DoNotCallInterfaceValidateResult validateResult)
            where T : DoNotCallTransactionInput
        {
            DateTime now = DateTime.Now;

            _commonFacade = new CommonFacade();
            string neverExpireDate = _commonFacade.GetCacheParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime nonExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDate, "yyyy-MM-dd").Value;
            bool isActive = request.Status == Constants.DigitTrue;
            DateTime expireDate = isActive ? request.ExpiryDate.HasValue ? request.ExpiryDate.Value : now.Date.AddYears(1).Date : now.Date; // if inactive set expire date to today
            bool isBlockAllSalesProducts = validateResult.IsBlockAllSalesProducts;
            bool isBlockAllInfoProducts = validateResult.IsBlockAllInfoProducts;
            List<DoNotCallActivityProductInput> blockSalesProducts = isBlockAllSalesProducts ? new List<DoNotCallActivityProductInput>()
                                                                           : validateResult.ActivityProducts
                                                                             .Where(x => x.BlockType == Constants.ActivityProductTypeSales)
                                                                             .ToList();
            List<DoNotCallActivityProductInput> blockInfoProducts = isBlockAllInfoProducts ? new List<DoNotCallActivityProductInput>()
                                                                          : validateResult.ActivityProducts
                                                                            .Where(x => x.BlockType == Constants.ActivityProductTypeInformation)
                                                                            .ToList();

            // check user exists?
            _userFacade = new UserFacade();
            UserEntity user = _userFacade.GetUserByLogin(request.UpdateUser) ?? new UserEntity
            {
                UserId = 0,
                Username = request.UpdateUser
            };

            var entity = new DoNotCallByTelephoneEntity
            {
                CurrentUserId = user.UserId,
                CurrentUsername = user.Username,
                CurrentUserIpAddress = ApplicationHelpers.GetClientIP(),
                BasicInfo = new DoNotCallBasicInfoModel
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    TransactionId = validateResult.TransactionId ?? 0,
                    EffectiveDate = now.Date,
                    CreateDate = now,
                    UpdateDate = now,
                    IsNeverExpire = expireDate.Date == nonExpireDate.Date,
                    ExpireDate = expireDate,
                    FromSystem = request.SystemCode,
                    IsActive = isActive,
                    Remark = request.Remark
                },
                CardInfo = new DoNotCallCardInfoModel
                {
                    CardNo = request.CardNo.GetCleanString(defaultWhenNull: null),
                    SubscriptionTypeId = validateResult.SubscriptionTypeId
                },
                ContactDetail = new DoNotCallContactModel
                {
                    Email = new DoNotCallEmail
                    {
                        EmailList = request.EmailList?.Select(email => new DoNotCallEmailModel
                        {
                            Email = email.Email,
                            IsDeleted = email.IsActive == Constants.FlagN,
                            LastUpdateDate = now
                        }).ToList() ?? new List<DoNotCallEmailModel>()
                    },
                    Telephone = new DoNotCallTelephone
                    {
                        TelephoneList = request.TelephoneList?.Select(phoneNo => new DoNotCallTelephoneModel
                        {
                            PhoneNo = phoneNo.PhoneNo,
                            IsDeleted = phoneNo.IsActive == Constants.FlagN,
                            LastUpdateDate = now
                        }).ToList() ?? new List<DoNotCallTelephoneModel>()
                    }
                },
                BlockSales = new DoNotCallBlockSalesModel
                {
                    IsBlockAllSalesProducts = isBlockAllSalesProducts,
                    IsBlockSalesEmail = request.SalesBlockInfo.BlockEmail == Constants.DigitTrue,
                    IsBlockSalesSms = request.SalesBlockInfo.BlockSMS == Constants.DigitTrue,
                    IsBlockSalesTelephone = request.SalesBlockInfo.BlockTelephone == Constants.DigitTrue,
                    BlockSalesProductList = blockSalesProducts.Select(product => new DoNotCallProductModel
                    {
                        ActivityProductType = Constants.ActivityProductTypeSales,
                        CreateDate = now,
                        IsDeleted = product.IsActive == Constants.FlagN,
                        Id = product.ProductId.Value,
                        ActivityProductId = product.ActivityProductId,
                        UpdateDate = now
                    }).ToList() ?? new List<DoNotCallProductModel>()
                },
                BlockInformation = new DoNotCallBlockInformationModel
                {
                    IsBlockAllInfoProducts = isBlockAllInfoProducts,
                    IsBlockInfoEmail = request.InformationBlockInfo.BlockEmail == Constants.DigitTrue,
                    IsBlockInfoSms = request.InformationBlockInfo.BlockSMS == Constants.DigitTrue,
                    IsBlockInfoTelephone = request.InformationBlockInfo.BlockTelephone == Constants.DigitTrue,
                    BlockInfoProductList = blockInfoProducts.Select(product => new DoNotCallProductModel
                    {
                        ActivityProductType = Constants.ActivityProductTypeInformation,
                        CreateDate = now,
                        IsDeleted = product.IsActive == Constants.FlagN,
                        Id = product.ProductId.Value,
                        ActivityProductId = product.ActivityProductId,
                        UpdateDate = now
                    }).ToList() ?? new List<DoNotCallProductModel>()
                }
            };
            return entity;
        }

        private DoNotCallByCustomerEntity MapCustomerEntity<T>(T request, DoNotCallInterfaceValidateResult validateResult)
            where T : DoNotCallTransactionInput
        {
            DateTime now = DateTime.Now;
            _commonFacade = new CommonFacade();
            string neverExpireDate = _commonFacade.GetCacheParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime nonExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDate, "yyyy-MM-dd").Value;
            bool isActive = request.Status == Constants.DigitTrue;
            DateTime expireDate = isActive ? request.ExpiryDate.HasValue ? request.ExpiryDate.Value : now.Date.AddYears(1).Date : now.Date; // if inactive set expire date to today
            bool isBlockAllSalesProducts = validateResult.IsBlockAllSalesProducts;
            bool isBlockAllInfoProducts = validateResult.IsBlockAllInfoProducts;
            List<DoNotCallActivityProductInput> blockSalesProducts = isBlockAllSalesProducts ? new List<DoNotCallActivityProductInput>()
                                                                           : validateResult.ActivityProducts
                                                                             .Where(x => x.BlockType == Constants.ActivityProductTypeSales)
                                                                             .ToList();
            List<DoNotCallActivityProductInput> blockInfoProducts = isBlockAllInfoProducts ? new List<DoNotCallActivityProductInput>()
                                                                          : validateResult.ActivityProducts
                                                                            .Where(x => x.BlockType == Constants.ActivityProductTypeInformation)
                                                                            .ToList();

            // check user exists?
            _userFacade = new UserFacade();
            UserEntity user = _userFacade.GetUserByLogin(request.UpdateUser) ?? new UserEntity
            {
                UserId = 0,
                Username = request.UpdateUser
            };

            var entity = new DoNotCallByCustomerEntity
            {
                CurrentUserId = user.UserId,
                CurrentUsername = user.Username,
                CurrentUserIpAddress = ApplicationHelpers.GetClientIP(),
                BasicInfo = new DoNotCallBasicInfoModel
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    TransactionId = validateResult.TransactionId ?? 0,
                    EffectiveDate = now.Date,
                    CreateDate = now,
                    UpdateDate = now,
                    IsNeverExpire = expireDate.Date == nonExpireDate.Date,
                    ExpireDate = expireDate,
                    FromSystem = request.SystemCode,
                    IsActive = isActive,
                    Remark = request.Remark
                },
                CardInfo = new DoNotCallCardInfoModel
                {
                    CardNo = request.CardNo,
                    SubscriptionTypeId = validateResult.SubscriptionTypeId.Value
                },
                ContactDetail = new DoNotCallContactModel
                {
                    Email = new DoNotCallEmail
                    {
                        EmailList = request.EmailList?.Select(email => new DoNotCallEmailModel
                        {
                            Email = email.Email,
                            Id = email.Id.HasValue? email.Id.Value: 0,
                            IsDeleted = email.IsActive == Constants.FlagN,
                            LastUpdateDate = now
                        }).ToList() ?? new List<DoNotCallEmailModel>()
                    },
                    Telephone = new DoNotCallTelephone
                    {
                        TelephoneList = request.TelephoneList?.Select(phoneNo => new DoNotCallTelephoneModel
                        {
                            PhoneNo = phoneNo.PhoneNo,
                            Id = phoneNo.Id.HasValue? phoneNo.Id.Value: 0,
                            IsDeleted = phoneNo.IsActive == Constants.FlagN,
                            LastUpdateDate = now
                        }).ToList() ?? new List<DoNotCallTelephoneModel>()
                    }
                },
                BlockSales = new DoNotCallBlockSalesModel
                {
                    IsBlockAllSalesProducts = isBlockAllSalesProducts,
                    IsBlockSalesEmail = request.SalesBlockInfo.BlockEmail == Constants.DigitTrue,
                    IsBlockSalesSms = request.SalesBlockInfo.BlockSMS == Constants.DigitTrue,
                    IsBlockSalesTelephone = request.SalesBlockInfo.BlockTelephone == Constants.DigitTrue,
                    BlockSalesProductList = blockSalesProducts.Select(product => new DoNotCallProductModel
                    {
                        ActivityProductType = Constants.ActivityProductTypeSales,
                        CreateDate = now,
                        IsDeleted = product.IsActive == Constants.FlagN,
                        Id = product.ProductId.Value,
                        ActivityProductId = product.ActivityProductId,
                        ProductId = product.ProductId.HasValue ? product.ProductId.Value: 0,
                        UpdateDate = now
                    }).ToList() ?? new List<DoNotCallProductModel>()
                },
                BlockInformation = new DoNotCallBlockInformationModel
                {
                    IsBlockAllInfoProducts = isBlockAllInfoProducts,
                    IsBlockInfoEmail = request.InformationBlockInfo.BlockEmail == Constants.DigitTrue,
                    IsBlockInfoSms = request.InformationBlockInfo.BlockSMS == Constants.DigitTrue,
                    IsBlockInfoTelephone = request.InformationBlockInfo.BlockTelephone == Constants.DigitTrue,
                    BlockInfoProductList = blockInfoProducts.Select(product => new DoNotCallProductModel
                    {
                        ActivityProductType = Constants.ActivityProductTypeInformation,
                        CreateDate = now,
                        IsDeleted = product.IsActive == Constants.FlagN,
                        Id = product.ProductId.Value,
                        ActivityProductId = product.ActivityProductId,
                        ProductId = product.ProductId.HasValue ? product.ProductId.Value: 0,
                        UpdateDate = now
                    }).ToList() ?? new List<DoNotCallProductModel>()
                }
            };
            return entity;
        }

        private T GetInvalidInputResponse<T>(string errorMessage) where T : BaseStatusResponse, new()
        {
            return new T
            {
                ResponseMessage = errorMessage,
                ResponseCode = Constants.InterfaceResponseCode.InvalidInput
            };
        }

        private T GetErrorResponse<T>(Exception ex) where T : BaseStatusResponse, new()
        {
            _logger.InfoFormat($"O:--FAILED--:Error Message/{ex.Message}");
            _logger.Error("Exception occur:\n", ex);
            return new T
            {
                ResponseMessage = ex.Message
            };
        }

        private Header GetResponseHeader(Header requestHeader, string methodName)
        {
            ThreadContext.Properties["EventClass"] = ApplicationHelpers.GetCurrentMethod(1);
            ThreadContext.Properties["RemoteAddress"] = ApplicationHelpers.GetClientIP();

            ThreadContext.Properties["UserID"] = requestHeader.user_name;

            _logger.Info(_logMsg.Clear().SetPrefixMsg($"Call {methodName}").ToInputLogString());
            _logger.Debug($"I:--START--:--{methodName}--");

            return new Header
            {
                reference_no = requestHeader.reference_no,
                service_name = requestHeader.service_name,
                system_code = requestHeader.system_code,
                transaction_date = requestHeader.transaction_date
            };
        }

        private T GetInvalidLoginResponse<T>() where T : BaseStatusResponse, new()
        {
            string errorMessage = "Bad Request, the header is not valid";
            _logger.Info($"O:--LOGIN--:Error Message/{errorMessage}");
            return new T
            {
                ResponseCode = Constants.InterfaceResponseCode.BadRequest,
                ResponseMessage = errorMessage
            };
        }

        private bool ValidateServiceRequest(Header requestHeader)
        {
            _commonFacade = new CommonFacade();
            string doNotCallProfilePath = _commonFacade.GetProfileXml("DoNotCallProfile");
            bool valid = _commonFacade.VerifyServiceRequest<Header>(requestHeader, doNotCallProfilePath);
            return valid;
        }

        private static T GetSuccessResponseStatusInfo<T>() where T : BaseStatusResponse, new()
        {
            return new T
            {
                ResponseCode = Constants.InterfaceResponseCode.Success,
                ResponseMessage = Constants.InterfaceResponseMessage.Success,
            };
        }

        private List<TransactionInfo> SearchDoNotCallTransaction(InquireDoNotCallRequest request)
        {
            _doNotCallFacade = new DoNotCallFacade();
            var cardNo = request.CardNo;

            var searchFilter = new DoNotCallListSearchFilter
            {
                CardNo = cardNo,
                Telephone = request.PhoneNo,
                Email = request.Email,
                PageNo = 1,
                PageSize = request.DataLimit,
                ProductCode = request.ProductCode
            };

            if (!string.IsNullOrWhiteSpace(cardNo))
            {
                string code = request.SubscriptionTypeCode;
                int? subscriptionTypeId = _commonFacade.GetSubscriptTypeByCode(code)?.SubscriptTypeId;
                if (!subscriptionTypeId.HasValue)
                    throw new NullReferenceException($"Subscription Type Code: {code} not found.");
                else
                    searchFilter.SubscriptionTypeId = subscriptionTypeId.Value;
            }

            List<TransactionInfo> result = _doNotCallFacade.SearchExactDoNotCallTransaction(searchFilter);
            return result;
        }

        private DoNotCallInterfaceValidateResult ValidateInsertUpdatePhoneRequest(InsertOrUpdateDoNotCallPhoneRequest request)
        {
            var result = new DoNotCallInterfaceValidateResult();
            bool valid = ValidateRequestModel(request, result);
            if (valid) // validate with database
            {
                // check item alreayd exist (get id and update date)
                if (request.TransactionId != 0)
                {
                    int transactionId = request.TransactionId;
                    DoNotCallTransactionInfo info = _doNotCallFacade.GetTelephoneTransactionById(transactionId);
                    if (info == null)
                    {
                        result.ErrorMessage = $"Transaction ID: {transactionId} not found.";
                    }
                    else if (info.UpdateDate > request.UpdateDate)
                    {
                        result.ErrorMessage = $"Transaction already updated (Transaction ID: {info.TransactionId})";
                    }
                    else if (info.IsBlockAllInfoProduct && request.InformationBlockInfo.BlockProducts.Count > 0)
                    {
                        result.ErrorMessage = $"Cannot add block product to Information block product list. The current setting is block ALL products.";
                    }
                    else if (info.IsBlockAllSalesProduct && request.SalesBlockInfo.BlockProducts.Count > 0)
                    {
                        result.ErrorMessage = $"Cannot add block product to Sales block product list. The current setting is block ALL products.";
                    }
                    else
                    {
                        result.IsBlockAllInfoProducts = info.IsBlockAllInfoProduct;
                        result.IsBlockAllSalesProducts = info.IsBlockAllSalesProduct;
                        // remove duplicated email
                        if (request.EmailList != null && request.EmailList.Count > 0 && info.Emails.Count > 0)
                        {
                            request.EmailList.RemoveAll(email => info.Emails
                                                                     .Any(y => y.Email.Equals(email.Email, StringComparison.InvariantCultureIgnoreCase)
                                                                            && y.IsDeleted == email.IsDeleted));
                        }
                        // remove duplicated phone no
                        if (request.TelephoneList != null && request.TelephoneList.Count > 0 && info.Telephones.Count > 0)
                        {
                            request.TelephoneList.RemoveAll(phoneNo => info.Telephones
                                                                            .Any(y => y.PhoneNo.Equals(phoneNo.PhoneNo, StringComparison.InvariantCultureIgnoreCase)
                                                                                   && y.IsDeleted == phoneNo.IsDeleted));
                        }

                        result.TransactionId = info.TransactionId;
                        // check subscription type code exisst 
                        if (!string.IsNullOrWhiteSpace(request.SubscriptTypeCode))
                        {
                            _commonFacade = new CommonFacade();
                            string subscriptTypeCode = request.SubscriptTypeCode;
                            int? subTypeId = _commonFacade.GetSubscriptTypeByCode(subscriptTypeCode)?.SubscriptTypeId;
                            if (!subTypeId.HasValue)
                            {
                                result.ErrorMessage = $"Subscript type code {subscriptTypeCode} not found";
                            }
                            else
                            {
                                // check product exists 
                                result.SubscriptionTypeId = subTypeId.Value;
                            }
                        }

                        ValidateProductCodes(request, result);

                        if (result.IsValid)
                        {
                            // check duplicated sales product
                            result.ActivityProducts.RemoveAll(x => x.BlockType == Constants.ActivityProductTypeSales
                                                                && info.SalesProducts.Any(y => y.ProductId == x.ProductId && x.IsDeleted == y.IsDeleted));
                            // check duplicated info product
                            result.ActivityProducts.RemoveAll(x => x.BlockType == Constants.ActivityProductTypeInformation
                                                                && info.InfoProducts.Any(y => y.ProductId == x.ProductId && x.IsDeleted == y.IsDeleted));
                        }
                    }
                }
                else
                {
                    // check subscription type code exisst 
                    if (!string.IsNullOrWhiteSpace(request.SubscriptTypeCode))
                    {
                        _commonFacade = new CommonFacade();
                        string subscriptTypeCode = request.SubscriptTypeCode;
                        int? subTypeId = _commonFacade.GetSubscriptTypeByCode(subscriptTypeCode)?.SubscriptTypeId;
                        if (!subTypeId.HasValue)
                        {
                            result.ErrorMessage = $"Subscript type code {subscriptTypeCode} not found";
                        }
                        else
                        {
                            // check product exists 
                            result.SubscriptionTypeId = subTypeId.Value;
                        }
                    }

                    ValidateProductCodes(request, result);
                }
            }
            result.IsValid = string.IsNullOrWhiteSpace(result.ErrorMessage);

            return result;
        }

        private DoNotCallInterfaceValidateResult ValidateInsertUpdateCustomerRequest(InsertOrUpdateDoNotCallCustomerRequest request)
        {
            var result = new DoNotCallInterfaceValidateResult();
            bool valid = ValidateRequestModel(request, result);

            if (valid) // validate with database
            {
                // check item alreayd exist (get id and update date)
                DoNotCallTransactionInfo info = _doNotCallFacade.GetCustomerTransaction(request.CardNo, request.SubscriptTypeCode);
                bool isExistingTransaction = info != null && info.TransactionId > 0;
                if (isExistingTransaction && info.UpdateDate > request.UpdateDate)
                {
                    result.ErrorMessage = $"Transaction already exists (transaction ID: {info.TransactionId})";
                }
                else if (isExistingTransaction && info.IsBlockAllInfoProduct && request.InformationBlockInfo.BlockProducts.Count > 0)
                {
                    result.ErrorMessage = $"Cannot add block product to Information block product list. The current setting is block ALL products.";
                }
                else if (isExistingTransaction && info.IsBlockAllSalesProduct && request.SalesBlockInfo.BlockProducts.Count > 0)
                {
                    result.ErrorMessage = $"Cannot add block product to Sales block product list. The current setting is block ALL products.";
                }
                else
                {
                    if (isExistingTransaction) // For Update
                    {
                        result.IsBlockAllInfoProducts = info.IsBlockAllInfoProduct;
                        result.IsBlockAllSalesProducts = info.IsBlockAllSalesProduct;
                        // remove duplicated email
                        if (request.EmailList != null && request.EmailList.Count > 0 && info.Emails.Count > 0)
                        {
                            var removeItemList = new List<BlockEmail>();
                            foreach (var inputItem in request.EmailList)
                            {
                                var existingItems = info.Emails.Where(x => x.Email.Equals(inputItem.Email, StringComparison.InvariantCultureIgnoreCase)).ToList();
                                bool itemAlreadyExists = existingItems != null && existingItems.Count > 0;

                                if (itemAlreadyExists)
                                {
                                    bool inputItemIsActive = inputItem.IsActive == Constants.FlagY;
                                    var activeItem = existingItems.FirstOrDefault(x => !x.IsDeleted);
                                    bool haveExistingActiveItem = activeItem != null;
                                    bool haveExistingDeletedItem = existingItems.Any(x => x.IsDeleted);

                                    bool itemAlreadyActive = haveExistingActiveItem && inputItemIsActive;
                                    bool itemAlreadyDeleted = !haveExistingActiveItem && haveExistingDeletedItem && !inputItemIsActive;
                                    if (itemAlreadyActive || itemAlreadyDeleted)
                                    {
                                        removeItemList.Add(inputItem);
                                    }
                                    else if (haveExistingActiveItem && !inputItemIsActive) // have item to delete
                                    {
                                        inputItem.Id = activeItem.Id;
                                    }
                                }
                            }

                            // remove items
                            request.EmailList = request.EmailList.Except(removeItemList).ToList();
                        }
                        // remove duplicated phone no
                        if (request.TelephoneList != null && request.TelephoneList.Count > 0 && info.Telephones.Count > 0)
                        {
                            var removeItemList = new List<BlockPhoneNo>();
                            foreach (var inputItem in request.TelephoneList)
                            {
                                var existingItems = info.Telephones.Where(x => x.PhoneNo.Equals(inputItem.PhoneNo, StringComparison.InvariantCultureIgnoreCase)).ToList();
                                bool itemAlreadyExists = existingItems != null && existingItems.Count > 0;

                                if (itemAlreadyExists)
                                {
                                    bool inputItemIsActive = inputItem.IsActive == Constants.FlagY;
                                    var activeItem = existingItems.FirstOrDefault(x => !x.IsDeleted);
                                    bool haveExistingActiveItem = activeItem != null;
                                    bool haveExistingDeletedItem = existingItems.Any(x => x.IsDeleted);

                                    bool itemAlreadyActive = haveExistingActiveItem && inputItemIsActive;
                                    bool itemAlreadyDeleted = !haveExistingActiveItem && haveExistingDeletedItem && !inputItemIsActive;
                                    if (itemAlreadyActive || itemAlreadyDeleted)
                                    {
                                        removeItemList.Add(inputItem);
                                    }
                                    else if (haveExistingActiveItem && !inputItemIsActive) // have item to delete
                                    {
                                        inputItem.Id = activeItem.Id;
                                    }
                                }
                            }

                            // remove items
                            request.TelephoneList = request.TelephoneList.Except(removeItemList).ToList();
                        }
                    }

                    result.TransactionId = info?.TransactionId;
                    // check subscription type code exisst 
                    _commonFacade = new CommonFacade();
                    string subscriptTypeCode = request.SubscriptTypeCode;
                    int? subTypeId = _commonFacade.GetSubscriptTypeByCode(subscriptTypeCode)?.SubscriptTypeId;
                    if (!subTypeId.HasValue)
                    {
                        result.ErrorMessage = $"Subscript type code {subscriptTypeCode} not found";
                    }
                    else
                    {
                        // check product exists 
                        result.SubscriptionTypeId = subTypeId.Value;
                        ValidateProductCodes(request, result);
                        if (result.IsValid && isExistingTransaction)
                        {
                            // Sales
                            var removeSalesProductList = new List<DoNotCallActivityProductInput>();
                            foreach (var inputItem in result.ActivityProducts)
                            {
                                var existingItems = info.SalesProducts.Where(x => x.ProductId == inputItem.ProductId).ToList();
                                bool itemAlreadyExists = existingItems != null && existingItems.Count > 0;

                                if (itemAlreadyExists)
                                {
                                    bool inputItemIsActive = inputItem.IsActive == Constants.FlagY;
                                    var activeItem = existingItems.FirstOrDefault(x => !x.IsDeleted);
                                    bool haveExistingActiveItem = activeItem != null;
                                    bool haveExistingDeletedItem = existingItems.Any(x => x.IsDeleted);

                                    bool itemAlreadyActive = haveExistingActiveItem && inputItemIsActive;
                                    bool itemAlreadyDeleted = !haveExistingActiveItem && haveExistingDeletedItem && !inputItemIsActive;
                                    if (itemAlreadyActive || itemAlreadyDeleted)
                                    {
                                        removeSalesProductList.Add(inputItem);
                                    }
                                    else if (haveExistingActiveItem && !inputItemIsActive) // have item to delete
                                    {
                                        inputItem.ProductId = activeItem.ProductId;
                                    }
                                }
                            }
                            // Info
                            var removeInfoProductList = new List<DoNotCallActivityProductInput>();
                            foreach (var inputItem in result.ActivityProducts)
                            {
                                var existingItems = info.InfoProducts.Where(x => x.ProductId == inputItem.ProductId).ToList();
                                bool itemAlreadyExists = existingItems != null && existingItems.Count > 0;

                                if (itemAlreadyExists)
                                {
                                    bool inputItemIsActive = inputItem.IsActive == Constants.FlagY;
                                    var activeItem = existingItems.FirstOrDefault(x => !x.IsDeleted);
                                    bool haveExistingActiveItem = activeItem != null;
                                    bool haveExistingDeletedItem = existingItems.Any(x => x.IsDeleted);

                                    bool itemAlreadyActive = haveExistingActiveItem && inputItemIsActive;
                                    bool itemAlreadyDeleted = !haveExistingActiveItem && haveExistingDeletedItem && !inputItemIsActive;
                                    if (itemAlreadyActive || itemAlreadyDeleted)
                                    {
                                        removeInfoProductList.Add(inputItem);
                                    }
                                    else if (haveExistingActiveItem && !inputItemIsActive) // have item to delete
                                    {
                                        inputItem.ProductId = activeItem.ProductId;
                                    }
                                }
                            }
                            // remove items
                            result.ActivityProducts = result.ActivityProducts.Except(removeInfoProductList).ToList();
                        }
                    }
                }
            }

            result.IsValid = string.IsNullOrWhiteSpace(result.ErrorMessage);

            return result;
        }

        private static bool ValidateRequestModel<T>(T request, DoNotCallInterfaceValidateResult result)
            where T : DoNotCallTransactionInput
        {
            dynamic[] modelValidateResults = ApplicationHelpers.DoValidation(request);
            var valid = (bool)modelValidateResults[0];
            List<ValidationResult> validationResults = (List<ValidationResult>)modelValidateResults[1];

            if (!valid) // validate format 
            {
                if (validationResults != null && validationResults.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Bad Request, the body is not valid:");

                    foreach (var item in validationResults)
                    {
                        sb.AppendLine(item.ErrorMessage);
                    }
                    result.ErrorMessage = sb.ToString();
                }
            }

            return valid;
        }

        private void ValidateProductCodes<T>(T request, DoNotCallInterfaceValidateResult result)
            where T : DoNotCallTransactionInput
        {
            var productCodes = new List<DoNotCallActivityProductInput>();
            var blockInfoProducList = request.InformationBlockInfo.BlockProducts;
            var blockSalesProductList = request.SalesBlockInfo.BlockProducts;

            if (blockInfoProducList?.Count > 0)
            {
                productCodes.AddRange(blockInfoProducList.Select(x => new DoNotCallActivityProductInput
                {
                    BlockType = Constants.ActivityProductTypeInformation,
                    ProductCode = x.ProductCode,
                    IsActive = x.IsActive
                }));
            }

            if (blockSalesProductList?.Count > 0)
            {
                productCodes.AddRange(blockSalesProductList.Select(x => new DoNotCallActivityProductInput
                {
                    BlockType = Constants.ActivityProductTypeSales,
                    ProductCode = x.ProductCode,
                    IsActive = x.IsActive
                }));
            }

            if (productCodes.Count > 0)
            {
                _productFacade = new ProductFacade();
                var products = _productFacade.GetActivityProductIdFromProductCodeList(productCodes);
                StringBuilder sb = new StringBuilder();
                List<string> invalidCodes = new List<string>();
                invalidCodes.AddRange(products.Where(x => !x.ProductId.HasValue).Select(x => x.ProductCode).ToList());
                if (invalidCodes.Count > 0)
                {
                    result.ErrorMessage = $"ProductCode not found: {string.Join(", ", invalidCodes)}";
                }
                else
                {
                    result.ActivityProducts = products;
                    result.IsValid = true;
                }
            }
        }

        private bool ValidateInquireDoNotCallRequest(InquireDoNotCallRequest request, out string errorMessage)
        {
            bool validInput = false;
            errorMessage = string.Empty;
            StringBuilder error = new StringBuilder();

            if (request == null)
            {
                error.AppendLine("Request is null");
            }
            else
            {
                _commonFacade = new CommonFacade();
                bool hasAtLeastOneCriteria = request.CisId > 0
                                          || !string.IsNullOrWhiteSpace(request.CardNo)
                                          || !string.IsNullOrWhiteSpace(request.SubscriptionTypeCode)
                                          || !string.IsNullOrWhiteSpace(request.ProductCode)
                                          || !string.IsNullOrWhiteSpace(request.PhoneNo)
                                          || !string.IsNullOrWhiteSpace(request.Email);

                if (!hasAtLeastOneCriteria)
                {
                    error.AppendLine("Must insert at least one search criteria");
                }
                else
                {
                    // Card Info
                    bool hasCardNo = !string.IsNullOrWhiteSpace(request.CardNo);
                    bool hasSubscriptionType = !string.IsNullOrWhiteSpace(request.SubscriptionTypeCode);
                    bool incompleteCardInfo = hasCardNo ^ hasSubscriptionType;// exclusive-or: ONLY ONE condition is true
                    if (incompleteCardInfo)
                    {
                        error.AppendLine("Incomplete card info: missing CardNo or SubscriptionTypeCode");
                    }
                    // Total Limit
                    int doNotCallUploadLimit = _commonFacade.GetDoNotCallUploadTotalRecord();
                    bool validLimitNumber = request.DataLimit >= 1 && request.DataLimit <= doNotCallUploadLimit;
                    if (!validLimitNumber)
                    {
                        error.AppendLine($"Limit must be between 1 and {doNotCallUploadLimit.FormatNumber()} records");
                    }

                    validInput = validLimitNumber && !incompleteCardInfo;
                }
            }

            if (!validInput)
                errorMessage = error.ToString();

            return validInput;
        }
        #endregion
    }
}
