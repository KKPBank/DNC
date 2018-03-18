using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Data.DataAccess.Common;
using CSM.Data.DataAccess.Interfaces;
using CSM.Entity;
using CSM.Entity.Common;
using CSM.Service.Messages.DoNotCall;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Data.DataAccess
{
    public class DoNotCallDataAccess : IDoNotCallDataAccess
    {
        private object sync = new Object();
        private readonly CSMContext _csmContext;
        private readonly DNCContext _dncContext;
        private LogMessageBuilder _logMsg = new LogMessageBuilder();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DoNotCallDataAccess));

        public DoNotCallDataAccess(DNCContext dncContext, CSMContext csmContext)
        {
            _csmContext = csmContext;
            _csmContext.Configuration.ValidateOnSaveEnabled = false;
            _csmContext.Database.CommandTimeout = Constants.CommandTimeout;

            _dncContext = dncContext;
            _dncContext.Configuration.ValidateOnSaveEnabled = false;
            _dncContext.Database.CommandTimeout = Constants.CommandTimeout;
        }

        public List<DoNotCallUpdatePhoneNoModel> GetDoNotCallPhoneNoListByPeriod(DateTime fromDateTime, DateTime toDateTime)
        {
            // Get transaction with phone no
            var transactionPhones = (from t in _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking().Where(x => x.UPDATE_DATE >= fromDateTime && x.UPDATE_DATE <= toDateTime)
                                     join p in _dncContext.TB_T_DNC_PHONE_NO.AsNoTracking()
                                     on t.DNC_TRANSACTION_ID equals p.DNC_TRANSACTION_ID
                                     select new
                                     {
                                         IsActive = t.STATUS == Constants.DigitTrue && p.DELETE_STATUS == Constants.DeleteFlagFalse,
                                         PhoneNo = p.PHONE_NO
                                     })
                                    .Distinct();

            var phoneList = transactionPhones.GroupBy(x => x.PhoneNo).Where(group => group.Count() == 1).Select(x => x.FirstOrDefault());

            // Exclude phones with same status from last export
            var unchangedPhones = from p in phoneList
                                  join t in _dncContext.TB_T_DNC_EXPORT_FLAG.AsNoTracking()
                                  on p.PhoneNo equals t.PHONE_NO
                                  where p.IsActive == (t.STATUS_TO_TOT == Constants.DNC.ExportStatusIsBlock)
                                  select p;

            var newStatusPhones = phoneList.Except(unchangedPhones);

            List<string> expiredPhoneNos = new List<string>();
            List<string> newPhoneNoList = new List<string>();

            DateTime now = DateTime.Now;
            // Only export phones with same status in ALL transactions
            foreach (var phone in newStatusPhones)
            {
                // for active phones, find any phone with inactive status (deleted or transaction status = inactive)
                bool hasOtherStatus = (from t in _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking()
                                       join p in _dncContext.TB_T_DNC_PHONE_NO on t.DNC_TRANSACTION_ID equals p.DNC_TRANSACTION_ID
                                       select new { PhoneNo = p.PHONE_NO, IsActive = t.STATUS == Constants.DigitTrue && p.DELETE_STATUS == Constants.DeleteFlagFalse })
                                      .Any(x => x.PhoneNo == phone.PhoneNo && x.IsActive != phone.IsActive);

                if (!hasOtherStatus)
                {
                    string phoneNo = phone.PhoneNo;
                    if (phone.IsActive)
                    {
                        newPhoneNoList.Add(phone.PhoneNo);
                    }
                    else
                    {
                        expiredPhoneNos.Add(phone.PhoneNo);
                    }

                    string status = phone.IsActive ? Constants.DeleteFlagFalse : Constants.DeleteFlagTrue;

                    var entity = _dncContext.TB_T_DNC_EXPORT_FLAG.FirstOrDefault(x => x.PHONE_NO == phoneNo);
                    if (entity != null)
                    {
                        entity.STATUS_TO_TOT = status;
                        entity.UPDATE_DATE = now;
                    }
                    else
                    {
                        entity = new TB_T_DNC_EXPORT_FLAG
                        {
                            PHONE_NO = phoneNo,
                            CREATE_DATE = now,
                            UPDATE_DATE = now,
                            STATUS_TO_TOT = status,
                        };
                        _dncContext.TB_T_DNC_EXPORT_FLAG.Add(entity);
                    }
                }
            }

            if (newPhoneNoList.Count + expiredPhoneNos.Count > 0)
            {
                _dncContext.SaveChanges();
            }

            // generate report model
            List<DoNotCallUpdatePhoneNoModel> updatedList = GeneratePhoneNoUpdateList(newPhoneNoList, expiredPhoneNos);
            return updatedList;
        }

        public DoNotCallTimePeriodEntity GetExecuteTimePeriodEntity(string executeTime)
        {
            return _dncContext.TB_C_DNC_TIME_PERIOD.AsNoTracking()
                           .Where(x => x.EXECUTE_TIME == executeTime)
                           .Select(x => new DoNotCallTimePeriodEntity
                           {
                               Id = x.TIME_PERIOD_ID,
                               ExecuteTimeStr = x.EXECUTE_TIME,
                               FromTimeStr = x.TIME_PERIOD_FROM,
                               ToTimeStr = x.TIME_PERIOD_TO
                           }).FirstOrDefault();
        }

        public DoNotCallTransactionInfo GetDoNotCallTransactionInfoById(int transactionId)
        {
            TB_T_DNC_TRANSACTION transaction = _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking().FirstOrDefault(x => x.DNC_TRANSACTION_ID == transactionId);

            if (transaction != null)
            {
                DoNotCallTransactionInfo result = GetTransactionInfo(transaction);

                return result;
            }

            return new DoNotCallTransactionInfo();
        }

        public DoNotCallTransactionInfo GetCustomerTransaction(string cardNo, string subscriptTypeCode)
        {
            List<TB_T_DNC_TRANSACTION> transactions = _dncContext.TB_T_DNC_TRANSACTION.Where(x => x.TRANS_TYPE == Constants.DNC.TransactionTypeCustomer
                                                                       && x.CARD_NO.Equals(cardNo, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var subscripType = _csmContext.TB_M_SUBSCRIPT_TYPE.Where(x => x.SUBSCRIPT_TYPE_CODE == subscriptTypeCode).FirstOrDefault();

            if (subscripType != null)
            {
                var transaction = transactions.Where(x => x.SUBSCRIPT_TYPE_ID == subscripType.SUBSCRIPT_TYPE_ID).FirstOrDefault();

                if (transaction != null)
                {
                    DoNotCallTransactionInfo result = GetTransactionInfo(transaction);

                    return result;
                }
            }

            return new DoNotCallTransactionInfo();
        }

        private DoNotCallTransactionInfo GetTransactionInfo(TB_T_DNC_TRANSACTION transaction)
        {
            TB_T_DNC_ACTIVITY_TYPE activityTypes = transaction.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault();
            var products = _csmContext.TB_R_PRODUCT.Select(x => new { x.PRODUCT_NAME, x.PRODUCT_ID }).ToList();
            var result = new DoNotCallTransactionInfo
            {
                Emails = transaction.TB_T_DNC_EMAIL
                                          .Select(x => new DoNotCallEmailModel
                                          {
                                              Id = x.DNC_EMAIL_ID,
                                              Email = x.EMAIL,
                                              IsDeleted = x.DELETE_STATUS == Constants.DeleteFlagTrue,
                                          }).ToList(),
                Telephones = transaction.TB_T_DNC_PHONE_NO
                                            .Select(x => new DoNotCallTelephoneModel
                                            {
                                                Id = x.DNC_PHONE_NO_ID,
                                                PhoneNo = x.PHONE_NO,
                                                IsDeleted = x.DELETE_STATUS == Constants.DeleteFlagTrue
                                            }).ToList(),
                TransactionId = transaction.DNC_TRANSACTION_ID,
                IsBlockAllSalesProduct = activityTypes.SALES_BLOCK_ALL_PRODUCT == Constants.DigitTrue,
                IsBlockAllInfoProduct = activityTypes.INFORMATION_BLOCK_ALL_PRODUCT == Constants.DigitTrue,
                InfoProducts = activityTypes.TB_T_DNC_ACTIVITY_PRODUCT.Where(x => x.TYPE == Constants.ActivityProductTypeInformation)
                                        .Select(x => new ActivityProductEntity
                                        {
                                            ProductName = products.Where(p => x.PRODUCT_ID == p.PRODUCT_ID).Select(s => s.PRODUCT_NAME).FirstOrDefault(),
                                            ProductId = x.PRODUCT_ID,
                                            ActivityProductId = x.DNC_ACTIVITY_PRODUCT_ID,
                                            IsDeleted = x.DELETE_STATUS == Constants.DeleteFlagTrue
                                        }).ToList(),
                SalesProducts = activityTypes.TB_T_DNC_ACTIVITY_PRODUCT.Where(x => x.TYPE == Constants.ActivityProductTypeSales)
                                        .Select(x => new ActivityProductEntity
                                        {
                                            ProductName = products.Where(p => x.PRODUCT_ID == p.PRODUCT_ID).Select(s => s.PRODUCT_NAME).FirstOrDefault(),
                                            ProductId = x.PRODUCT_ID,
                                            ActivityProductId = x.DNC_ACTIVITY_PRODUCT_ID,
                                            IsDeleted = x.DELETE_STATUS == Constants.DeleteFlagTrue
                                        }).ToList(),
                UpdateDate = transaction.UPDATE_DATE.Value
            };
            return result;
        }

        public List<DoNotCallUpdatePhoneNoModel> GenerateUpdatedDoNotCallPhoneList()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1).Date;
            // Update first
            UpdateExpiredTransaction();

            // Get transaction with phone no
            var transactionPhones = (from t in _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking().Where(x => x.UPDATE_DATE.Value < today && x.UPDATE_DATE.Value >= yesterday)
                                     join p in _dncContext.TB_T_DNC_PHONE_NO.AsNoTracking()
                                     on t.DNC_TRANSACTION_ID equals p.DNC_TRANSACTION_ID
                                     select new
                                     {
                                         IsActive = t.STATUS == Constants.DigitTrue && p.DELETE_STATUS == Constants.DeleteFlagFalse,
                                         PhoneNo = p.PHONE_NO
                                     })
                                    .Distinct();

            var phoneList = transactionPhones.GroupBy(x => x.PhoneNo).Where(group => group.Count() == 1).Select(x => x.FirstOrDefault());

            // Exclude phones with same status from last export
            var unchangedPhones = from p in phoneList
                                  join t in _dncContext.TB_T_DNC_EXPORT_FLAG.AsNoTracking()
                                  on p.PhoneNo equals t.PHONE_NO
                                  where p.IsActive == (t.STATUS_TO_TOT == Constants.DNC.ExportStatusIsBlock)
                                  select p;

            var newStatusPhones = phoneList.Except(unchangedPhones);

            List<string> expiredPhoneNos = new List<string>();
            List<string> newPhoneNoList = new List<string>();

            DateTime now = DateTime.Now;
            // Only export phones with same status in ALL transactions
            foreach (var phone in newStatusPhones)
            {
                // for active phones, find any phone with inactive status (deleted or transaction status = inactive)
                bool hasOtherStatus = (from t in _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking()
                                       join p in _dncContext.TB_T_DNC_PHONE_NO on t.DNC_TRANSACTION_ID equals p.DNC_TRANSACTION_ID
                                       select new { PhoneNo = p.PHONE_NO, IsActive = t.STATUS == Constants.DigitTrue && p.DELETE_STATUS == Constants.DeleteFlagFalse })
                                      .Any(x => x.PhoneNo == phone.PhoneNo && x.IsActive != phone.IsActive);

                if (!hasOtherStatus)
                {
                    string phoneNo = phone.PhoneNo;
                    if (phone.IsActive)
                    {
                        newPhoneNoList.Add(phone.PhoneNo);
                    }
                    else
                    {
                        expiredPhoneNos.Add(phone.PhoneNo);
                    }

                    string status = phone.IsActive ? Constants.DeleteFlagFalse : Constants.DeleteFlagTrue;

                    var entity = _dncContext.TB_T_DNC_EXPORT_FLAG.FirstOrDefault(x => x.PHONE_NO == phoneNo);
                    if (entity != null)
                    {
                        entity.STATUS_TO_OTHER = status;
                        entity.UPDATE_DATE = now;
                    }
                    else
                    {
                        entity = new TB_T_DNC_EXPORT_FLAG
                        {
                            PHONE_NO = phoneNo,
                            CREATE_DATE = now,
                            UPDATE_DATE = now,
                            STATUS_TO_OTHER = status,
                        };
                        _dncContext.TB_T_DNC_EXPORT_FLAG.Add(entity);
                    }
                }
            }

            if (newPhoneNoList.Count + expiredPhoneNos.Count > 0)
            {
                _dncContext.SaveChanges();
            }

            List<DoNotCallUpdatePhoneNoModel> updatedList = GeneratePhoneNoUpdateList(newPhoneNoList, expiredPhoneNos);

            return updatedList;
        }

        private static List<DoNotCallUpdatePhoneNoModel> GeneratePhoneNoUpdateList(List<string> newPhoneNoList, List<string> expirePhoneNoList)
        {
            var updatedList = new List<DoNotCallUpdatePhoneNoModel>();

            foreach (var phoneNo in newPhoneNoList)
            {
                var record = new DoNotCallUpdatePhoneNoModel
                {
                    PhoneNo = phoneNo,
                    Status = Constants.DNC.Block
                };
                updatedList.Add(record);
            }

            foreach (var phoneNo in expirePhoneNoList)
            {
                var record = new DoNotCallUpdatePhoneNoModel
                {
                    PhoneNo = phoneNo,
                    Status = Constants.DNC.Unblock
                };
                updatedList.Add(record);
            }

            return updatedList;
        }

        public List<string> UpdateExpiredTransaction()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var result = new List<string>();

            List<TB_T_DNC_TRANSACTION> updateTransactions = _dncContext.TB_T_DNC_TRANSACTION
                                                            .Where(x => x.EXPIRY_DATE < today
                                                                     && x.STATUS == Constants.DigitTrue)
                                                            .ToList();

            bool hasUpdateItems = updateTransactions != null && updateTransactions.Count > 0;
            if (hasUpdateItems)
            {
                foreach (var item in updateTransactions)
                {
                    item.STATUS = Constants.DigitFalse;
                    item.UPDATE_DATE = now;
                    item.UPDATE_USER = Constants.SystemUserId;
                    item.UPDATE_USERNAME = Constants.SystemUserName;

                    var activity = item.TB_T_DNC_ACTIVITY_TYPE.First();

                    var transHis = new TB_T_DNC_TRANSACTION_HIS
                    {
                        CARD_NO = item.CARD_NO,
                        CREATE_DATE = now,
                        CREATE_USER = Constants.SystemUserId,
                        CREATE_USERNAME = Constants.SystemUserName,
                        DNC_TRANSACTION_ID = item.DNC_TRANSACTION_ID,
                        EFFECTIVE_DATE = item.EFFECTIVE_DATE,
                        EXPIRY_DATE = item.EXPIRY_DATE,
                        FIRST_NAME = item.FIRST_NAME,
                        LAST_NAME = item.LAST_NAME,
                        FROM_SYSTEM = item.FROM_SYSTEM,
                        INFORMATION_BLOCK_ALL_PRODUCT = activity.INFORMATION_BLOCK_ALL_PRODUCT,
                        INFORMATION_BLOCK_EMAIL = activity.INFORMATION_BLOCK_EMAIL,
                        INFORMATION_BLOCK_SMS = activity.INFORMATION_BLOCK_SMS,
                        INFORMATION_BLOCK_TELEPHONE = activity.INFORMATION_BLOCK_TELEPHONE,
                        SALES_BLOCK_ALL_PRODUCT = activity.SALES_BLOCK_ALL_PRODUCT,
                        SALES_BLOCK_EMAIL = activity.SALES_BLOCK_EMAIL,
                        SALES_BLOCK_SMS = activity.SALES_BLOCK_SMS,
                        SALES_BLOCK_TELEPHONE = activity.SALES_BLOCK_TELEPHONE,
                        STATUS = item.STATUS,
                        SUBSCRIPT_TYPE_ID = item.SUBSCRIPT_TYPE_ID,
                        TRANS_TYPE = item.TRANS_TYPE,
                        KKCIS_ID = item.KKCIS_ID,
                        REMARK = item.REMARK,
                        TB_T_DNC_TRANSACTION_HIS_EMAIL = item.TB_T_DNC_EMAIL.Select(x => new TB_T_DNC_TRANSACTION_HIS_EMAIL {
                            CREATE_DATE = x.CREATE_DATE,
                            CREATE_USER = x.CREATE_USER,
                            CREATE_USERNAME = x.CREATE_USERNAME,
                            DELETE_STATUS = x.DELETE_STATUS,
                            EMAIL = x.EMAIL
                        }).ToList(),
                        TB_T_DNC_TRANSACTION_HIS_PHONE = item.TB_T_DNC_PHONE_NO.Select(x => new TB_T_DNC_TRANSACTION_HIS_PHONE
                        {
                            CREATE_DATE = x.CREATE_DATE,
                            CREATE_USER = x.CREATE_USER,
                            CREATE_USERNAME = x.CREATE_USERNAME,
                            DELETE_STATUS = x.DELETE_STATUS,
                            PHONE_NO = x.PHONE_NO
                        }).ToList(),
                        TB_T_DNC_TRANSACTION_HIS_PRODUCT = activity.TB_T_DNC_ACTIVITY_PRODUCT.Select(x => new TB_T_DNC_TRANSACTION_HIS_PRODUCT
                        {
                            CREATE_DATE = x.CREATE_DATE,
                            CREATE_USER = x.CREATE_USER,
                            CREATE_USERNAME = x.CREATE_USERNAME,
                            DELETE_STATUS = x.DELETE_STATUS,
                            PRODUCT_ID = x.PRODUCT_ID,
                            TYPE = x.TYPE
                        }).ToList()
                    };
                    _dncContext.TB_T_DNC_TRANSACTION_HIS.Add(transHis);
                }
                int updateCount = _dncContext.SaveChanges();

                if (updateCount > 0)
                {
                    var resultList = (from t in updateTransactions
                                      join p in _dncContext.TB_T_DNC_PHONE_NO.AsNoTracking() on t.DNC_TRANSACTION_ID equals p.DNC_TRANSACTION_ID
                                      select p.PHONE_NO).Distinct().ToList();

                    if (resultList != null)
                        result = resultList;
                }
            }

            return result;
        }

        public DoNotCallFileUploadDetailModel GetFileUploadDetail(int id, Pager pager)
        {
            var loadList = _dncContext.TB_T_DNC_LOAD_LIST.Find(id);
            var result = new DoNotCallFileUploadDetailModel
            {
                IsSuccessUpload = true,
                CreateDate = loadList.CREATE_DATE.Value,
                FileName = loadList.FILE_NAME,
                FileUploadId = loadList.DNC_LOAD_LIST_ID
            };

            List<DoNotCallFileUploadDataModel> list = GetFileUploadDataListFromLoadListId(id, pager);

            if (list != null)
                result.DataList = list;

            result.Pager = pager;

            return result;
        }

        public List<DoNotCallFileUploadDataModel> GetFileUploadDataListFromLoadListId(int id, Pager pager)
        {
            var query = from x in _dncContext.TB_T_DNC_LOAD_LIST_DATA.Where(x => x.DNC_LOAD_LIST_ID == id)
                        select new DoNotCallFileUploadDataModel
                        {
                            RowNum = x.ROW_NO,
                            CardNo = x.CARD_NO,
                            Email = x.EMAIL,
                            FirstName = x.FIRST_NAME,
                            LastName = x.LAST_NAME,
                            IsActive = x.STATUS == Constants.DigitTrue,
                            IsBlockInfoEmail = x.INFORMATION_BLOCK_EMAIL == Constants.DigitTrue,
                            IsBlockInfoSMS = x.SALES_BLOCK_SMS == Constants.DigitTrue,
                            IsBlockInfoTelephone = x.INFORMATION_BLOCK_TELEPHONE == Constants.DigitTrue,
                            IsBlockSalesEmail = x.SALES_BLOCK_EMAIL == Constants.DigitTrue,
                            IsBlockSalesSMS = x.SALES_BLOCK_SMS == Constants.DigitTrue,
                            IsBlockSalesTelephone = x.SALES_BLOCK_TELEPHONE == Constants.DigitTrue,
                            PhoneNo = x.PHONE_NO,
                            TransactionType = x.TRANS_TYPE,
                            CardTypeId = x.SUBSCRIPT_TYPE_ID
                        };

            pager.TotalRecords = query.Count();

            query = QueryHelpers.SortIQueryable(query, pager.SortField, pager.SortOrder == Constants.SortOrderDesc);
            query = QueryHelpers.ApplyPaging(query, pager);

            var list = query.ToList();
            var cardTypes = _csmContext.TB_M_SUBSCRIPT_TYPE.Select(x => new { x.SUBSCRIPT_TYPE_ID, x.SUBSCRIPT_TYPE_NAME }).ToList();
            foreach (var item in list)
            {
                item.CardTypeName = cardTypes.Where(x => x.SUBSCRIPT_TYPE_ID == item.CardTypeId).Select(x => x.SUBSCRIPT_TYPE_NAME).FirstOrDefault();
            }
            return list;
        }

        public List<DoNotCallFileUploadSearchResultModel> SearchDoNotCallUploadList(DoNotCallLoadListSearchFilter model)
        {
            IQueryable<TB_T_DNC_LOAD_LIST> loadList = _dncContext.TB_T_DNC_LOAD_LIST.AsNoTracking();
            // Filter
            loadList = FilterUploadList(model, loadList);

            var query = from l in loadList
                        select new DoNotCallFileUploadSearchResultModel
                        {
                            FileName = l.FILE_NAME,
                            Id = l.DNC_LOAD_LIST_ID,
                            UpdateByUserId = l.UPDATE_USER,
                            UpdateDate = l.UPDATE_DATE.Value,
                            UploadDate = l.UPLOAD_DATE,
                            RecordCount = l.TB_T_DNC_LOAD_LIST_DATA.Count()
                        };

            model.TotalRecords = query.Count();

            query = QueryHelpers.SortIQueryable(query, model.SortField, model.SortOrder == Constants.SortOrderDesc);
            query = QueryHelpers.ApplyPaging(query, model);

            var list = query.ToList();
            var userList = _csmContext.TB_R_USER.ToList();
            foreach (var item in list)
            {
                var u = userList.Where(x => x.USER_ID == item.UpdateByUserId).First();
                item.LastUpdateBy = new DoNotCallUserModel
                {
                    FirstName = u.FIRST_NAME,
                    LastName = u.LAST_NAME,
                    PositionCode = u.POSITION_CODE,
                    UserId = u.USER_ID
                };
            }

            return list;
        }

        public int SaveFileUploadModels(List<TB_T_DNC_TRANSACTION> models, TB_T_DNC_LOAD_LIST loadList)
        {
            try
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = false;

                var customers = _dncContext.TB_T_DNC_TRANSACTION.Where(x => x.TRANS_TYPE == Constants.DNC.TransactionTypeCustomer);

                var customerModels = models.Where(x => x.TRANS_TYPE == Constants.DNC.TransactionTypeCustomer);

                var currentTransactions = (from c in customerModels
                                           join t in customers
                                           on new { cardNo = c.CARD_NO, cardType = c.SUBSCRIPT_TYPE_ID }
                                           equals new { cardNo = t.CARD_NO, cardType = t.SUBSCRIPT_TYPE_ID }
                                           select t).ToList();

                List<string> currentCardNos = currentTransactions.Select(x => x.CARD_NO).ToList();
                var newItemList = new List<TB_T_DNC_TRANSACTION>();
                foreach (var model in models)
                {
                    var cust = currentTransactions.FirstOrDefault(x => x.CARD_NO == model.CARD_NO);
                    if (cust != null)
                    {
                        SetEntryStateModified(cust);
                        // Update existing customer
                        cust.UPDATE_DATE = model.UPDATE_DATE;
                        cust.UPDATE_USER = model.UPDATE_USER;
                        cust.UPDATE_USERNAME = model.UPDATE_USERNAME;
                        cust.FIRST_NAME = model.FIRST_NAME;
                        cust.LAST_NAME = model.LAST_NAME;
                        cust.EFFECTIVE_DATE = model.EFFECTIVE_DATE;
                        cust.EXPIRY_DATE = model.EXPIRY_DATE;
                        cust.STATUS = model.STATUS;
                        cust.FROM_SYSTEM = model.FROM_SYSTEM;
                        cust.REMARK = model.REMARK;
                        // Activity Type
                        var activityType = cust.TB_T_DNC_ACTIVITY_TYPE.First();
                        SetEntryStateModified(activityType);
                        var modelActType = model.TB_T_DNC_ACTIVITY_TYPE.First();
                        activityType.UPDATE_DATE = modelActType.UPDATE_DATE;
                        activityType.UPDATE_USER = modelActType.UPDATE_USER;
                        activityType.UPDATE_USERNAME = modelActType.UPDATE_USERNAME;
                        activityType.INFORMATION_BLOCK_ALL_PRODUCT = modelActType.INFORMATION_BLOCK_ALL_PRODUCT;
                        activityType.INFORMATION_BLOCK_EMAIL = modelActType.INFORMATION_BLOCK_EMAIL;
                        activityType.INFORMATION_BLOCK_SMS = modelActType.INFORMATION_BLOCK_SMS;
                        activityType.INFORMATION_BLOCK_TELEPHONE = modelActType.INFORMATION_BLOCK_TELEPHONE;
                        activityType.SALES_BLOCK_ALL_PRODUCT = modelActType.SALES_BLOCK_ALL_PRODUCT;
                        activityType.SALES_BLOCK_EMAIL = modelActType.SALES_BLOCK_EMAIL;
                        activityType.SALES_BLOCK_SMS = modelActType.SALES_BLOCK_SMS;
                        activityType.SALES_BLOCK_TELEPHONE = modelActType.SALES_BLOCK_TELEPHONE;
                        // Add Hist
                        cust.TB_T_DNC_TRANSACTION_HIS.Add(model.TB_T_DNC_TRANSACTION_HIS.First());
                        // Activity products
                        var currentProducts = activityType.TB_T_DNC_ACTIVITY_PRODUCT.Where(x => x.DELETE_STATUS == Constants.DeleteFlagFalse).ToList();
                        foreach (var product in modelActType.TB_T_DNC_ACTIVITY_PRODUCT)
                        {
                            var currentProd = currentProducts.FirstOrDefault(x => x.PRODUCT_ID == product.PRODUCT_ID && x.TYPE == product.TYPE);
                            if (currentProd != null)
                            {
                                currentProd.UPDATE_DATE = product.UPDATE_DATE;
                                currentProd.UPDATE_USER = product.UPDATE_USER;
                                currentProd.UPDATE_USERNAME = product.UPDATE_USERNAME;
                            }
                            else
                            {
                                activityType.TB_T_DNC_ACTIVITY_PRODUCT.Add(product);
                            }
                        }

                        // Phone No
                        var currentTels = cust.TB_T_DNC_PHONE_NO.Where(x => x.DELETE_STATUS == Constants.DeleteFlagFalse);
                        foreach (var tel in model.TB_T_DNC_PHONE_NO)
                        {
                            var currentTel = currentTels.FirstOrDefault(x => x.PHONE_NO == tel.PHONE_NO);
                            if (currentTel != null)
                            {
                                currentTel.UPDATE_DATE = tel.UPDATE_DATE;
                                currentTel.UPDATE_USER = tel.UPDATE_USER;
                                currentTel.UPDATE_USERNAME = currentTel.UPDATE_USERNAME;
                            }
                            else
                            {
                                cust.TB_T_DNC_PHONE_NO.Add(tel);
                            }
                        }

                        // Email
                        var currentMails = cust.TB_T_DNC_EMAIL.Where(x => x.DELETE_STATUS == Constants.DeleteFlagFalse);
                        foreach (var mail in model.TB_T_DNC_EMAIL)
                        {
                            var currentEmail = currentMails.FirstOrDefault(x => x.EMAIL == mail.EMAIL);
                            if (currentEmail != null)
                            {
                                currentEmail.UPDATE_DATE = mail.UPDATE_DATE;
                                currentEmail.UPDATE_USER = mail.UPDATE_USER;
                                currentEmail.UPDATE_USERNAME = mail.UPDATE_USERNAME;
                            }
                            else
                            {
                                cust.TB_T_DNC_EMAIL.Add(mail);
                            }
                        }
                    }
                    else
                    {
                        newItemList.Add(model);
                    }
                }

                if (newItemList.Count > 0)
                    _dncContext.TB_T_DNC_TRANSACTION.AddRange(newItemList);

                _dncContext.TB_T_DNC_LOAD_LIST.Add(loadList);

                return _dncContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public List<DoNotCallProductModel> GetAllProductsForFileUploadCompare()
        {
            return _csmContext.TB_R_PRODUCT.AsNoTracking().Select(x =>
            new DoNotCallProductModel
            {
                Id = x.PRODUCT_ID,
                Name = x.PRODUCT_NAME
            }).ToList();
        }

        public List<DoNotCallHistoryEntity> GetDoNotCallHistoryList(int transactionId, Pager pager, int totalLimit)
        {
            var query = from t in _dncContext.TB_T_DNC_TRANSACTION_HIS.Where(x => x.DNC_TRANSACTION_ID == transactionId)
                        select new DoNotCallHistoryEntity
                        {
                            LogId = t.DNC_TRANSACTION_HIS_ID,
                            TransactionDate = t.CREATE_DATE.Value,
                            EditByUserId = t.CREATE_USER,
                            CreateUsername = t.CREATE_USERNAME
                        };

            query = QueryHelpers.ApplyPaging(query.OrderByDescending(x => x.LogId), pager);
            var items = query.ToList();
            var userList = _csmContext.TB_R_USER.ToList();
            foreach (var item in items)
            {
                var u = userList.Where(x => x.USER_ID == item.EditByUserId).FirstOrDefault();
                item.EditBy = new DoNotCallUserModel
                {
                    Username = u != null ? u.USERNAME : item.CreateUsername,
                    FirstName = u != null ? u.FIRST_NAME : "",
                    LastName = u != null ? u.LAST_NAME : "",
                    PositionCode = u != null ? u.POSITION_CODE : "",
                    UserId = u != null ? u.USER_ID : 0
                };
            }

            return items.Take(totalLimit).ToList();
        }

        public DoNotCallTransactionHistoryEntity GetDoNotCallHistoryDetailFromId(int logId)
        {
            var result = new DoNotCallTransactionHistoryEntity();

            var users = _csmContext.TB_R_USER.AsNoTracking().ToList();
            var products = _csmContext.TB_R_PRODUCT.AsNoTracking().ToList();

            var transHis = _dncContext.TB_T_DNC_TRANSACTION_HIS.AsNoTracking().Where(x => x.DNC_TRANSACTION_HIS_ID == logId).FirstOrDefault();
            if (transHis == null) return null;

            var transaction = _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking().FirstOrDefault(x => x.DNC_TRANSACTION_ID == transHis.DNC_TRANSACTION_ID);

            var transUpdateUser = users.FirstOrDefault(x => x.USER_ID == transaction.CREATE_USER);
            var transCreateUser = users.FirstOrDefault(x => x.USER_ID == transaction.UPDATE_USER);

            result.CardNo = transHis.CARD_NO;
            result.TransactionType = transHis.TRANS_TYPE;
            result.CardTypeName = _csmContext.TB_M_SUBSCRIPT_TYPE.AsNoTracking().FirstOrDefault(x => x.SUBSCRIPT_TYPE_ID == transHis.SUBSCRIPT_TYPE_ID)?.SUBSCRIPT_TYPE_NAME;

            var _commonDataAccess = new CommonDataAccess(_csmContext);
            string neverExpireDate = _commonDataAccess.GetParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime nonExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDate, "yyyy-MM-dd").Value;
            result.BaseInfo = new DoNotCallBasicInfoModel
            {
                CreateBy = new DoNotCallUserModel
                {
                    Username = transCreateUser != null ? transCreateUser.USERNAME : transaction.CREATE_USERNAME,
                    UserId = transCreateUser != null ? transCreateUser.USER_ID : 0,
                    FirstName = transCreateUser != null ? transCreateUser.FIRST_NAME : "",
                    LastName = transCreateUser != null ? transCreateUser.LAST_NAME : "",
                    PositionCode = transCreateUser != null ? transCreateUser.POSITION_CODE : ""
                },
                FirstName = transHis.FIRST_NAME,
                LastName = transHis.LAST_NAME,
                CreateDate = transaction.CREATE_DATE.Value,
                EffectiveDate = transHis.EFFECTIVE_DATE,
                ExpireDate = transHis.EXPIRY_DATE,
                FromSystem = transHis.FROM_SYSTEM,
                IsActive = transHis.STATUS == Constants.DigitTrue,
                IsNeverExpire = transHis.EXPIRY_DATE.Date == nonExpireDate.Date,
                Remark = transHis.REMARK,
                TransactionId = transHis.DNC_TRANSACTION_ID,
                UpdateBy = new DoNotCallUserModel
                {
                    Username = transUpdateUser != null ? transUpdateUser.USERNAME : transHis.CREATE_USERNAME,
                    UserId = transUpdateUser != null ? transUpdateUser.USER_ID : 0,
                    FirstName = transUpdateUser != null ? transUpdateUser.FIRST_NAME : "",
                    LastName = transUpdateUser != null ? transUpdateUser.LAST_NAME : "",
                    PositionCode = transUpdateUser != null ? transUpdateUser.POSITION_CODE : ""
                },
                UpdateDate = transHis.CREATE_DATE.Value
            };

            result.Emails = (from e in _dncContext.TB_T_DNC_TRANSACTION_HIS_EMAIL.AsNoTracking().Where(x => x.DNC_TRANSACTION_HIS_ID == logId)
                             select new DoNotCallEmailModel
                             {
                                 Email = e.EMAIL,
                                 Id = e.DNC_TRANSACTION_HIS_EMAIL_ID,
                                 IsDeleted = e.DELETE_STATUS == Constants.DeleteFlagTrue,
                                 LastUpdateDate = e.CREATE_DATE.Value,
                                 LastUpdateUserId = e.CREATE_USER.Value,
                                 CreateUsername = e.CREATE_USERNAME
                             }).ToList();

            foreach (var email in result.Emails)
            {
                var u = users.Where(x => x.USER_ID == email.LastUpdateUserId).FirstOrDefault();
                email.UpdateBy = new DoNotCallUserModel
                {
                    Username = u != null ? u.USERNAME : email.CreateUsername,
                    FirstName = u != null ? u.FIRST_NAME : "",
                    LastName = u != null ? u.LAST_NAME : "",
                    PositionCode = u != null ? u.POSITION_CODE : "",
                    UserId = u != null ? u.USER_ID : 0
                };
            }

            result.Telephones = (from p in _dncContext.TB_T_DNC_TRANSACTION_HIS_PHONE.AsNoTracking().Where(x => x.DNC_TRANSACTION_HIS_ID == logId)
                                 select new DoNotCallTelephoneModel
                                 {
                                     PhoneNo = p.PHONE_NO,
                                     Id = p.DNC_TRANSACTION_HIS_PHONE_ID,
                                     IsDeleted = p.DELETE_STATUS == Constants.DeleteFlagTrue,
                                     LastUpdateDate = p.CREATE_DATE.Value,
                                     LastUpdateUserId = p.CREATE_USER.Value,
                                     CreateUsername = p.CREATE_USERNAME
                                 }).ToList();

            foreach (var phone in result.Telephones)
            {
                var u = users.Where(x => x.USER_ID == phone.LastUpdateUserId).FirstOrDefault();
                phone.UpdateBy = new DoNotCallUserModel
                {
                    Username = u != null ? u.USERNAME : phone.CreateUsername,
                    FirstName = u != null ? u.FIRST_NAME : "",
                    LastName = u != null ? u.LAST_NAME : "",
                    PositionCode = u != null ? u.POSITION_CODE : "",
                    UserId = u != null ? u.USER_ID : 0
                };
            }

            var transactionProducts = _dncContext.TB_T_DNC_TRANSACTION_HIS_PRODUCT.AsNoTracking().Where(x => x.DNC_TRANSACTION_HIS_ID == logId).ToList();
            var tproducts = from t in transactionProducts
                            join p in products on t.PRODUCT_ID equals p.PRODUCT_ID
                            select new { t, p };

            var blockProducts = from p in tproducts
                                from u in users.Where(x => x.USER_ID == p.t.CREATE_USER).DefaultIfEmpty()
                                select new
                                {
                                    Product = p.p,
                                    ProductHst = p.t,
                                    User = u
                                };

            result.BlockInfoModel = new DoNotCallBlockInformationModel
            {
                BlockInfoProductList = blockProducts.Where(x => x.ProductHst.TYPE == Constants.ActivityProductTypeInformation)
                                        .Select(x => new DoNotCallProductModel
                                        {
                                            Name = x.Product.PRODUCT_NAME,
                                            UpdateDate = x.Product.CREATE_DATE.Value,
                                            IsDeleted = x.ProductHst.DELETE_STATUS == Constants.DeleteFlagTrue,
                                            UpdateBy = new DoNotCallUserModel
                                            {
                                                Username = x.User != null ? x.User.USERNAME : x.ProductHst.CREATE_USERNAME,
                                                FirstName = x.User != null ? x.User.FIRST_NAME : "",
                                                LastName = x.User != null ? x.User.LAST_NAME : "",
                                                PositionCode = x.User != null ? x.User.POSITION_CODE : "",
                                                UserId = x.User != null ? x.User.USER_ID : 0
                                            }
                                        }).ToList(),
                IsBlockAllInfoProducts = transHis.INFORMATION_BLOCK_ALL_PRODUCT == Constants.DigitTrue,
                IsBlockInfoEmail = transHis.INFORMATION_BLOCK_EMAIL == Constants.DigitTrue,
                IsBlockInfoSms = transHis.INFORMATION_BLOCK_SMS == Constants.DigitTrue,
                IsBlockInfoTelephone = transHis.INFORMATION_BLOCK_TELEPHONE == Constants.DigitTrue
            };

            result.BlockSalesModel = new DoNotCallBlockSalesModel
            {
                BlockSalesProductList = blockProducts.Where(x => x.ProductHst.TYPE == Constants.ActivityProductTypeSales)
                                                    .Select(x => new DoNotCallProductModel
                                                    {
                                                        Name = x.Product.PRODUCT_NAME,
                                                        UpdateDate = x.Product.CREATE_DATE.Value,
                                                        IsDeleted = x.ProductHst.DELETE_STATUS == Constants.DeleteFlagTrue,
                                                        UpdateBy = new DoNotCallUserModel
                                                        {
                                                            Username = x.User != null ? x.User.USERNAME : x.ProductHst.CREATE_USERNAME,
                                                            FirstName = x.User != null ? x.User.FIRST_NAME : "",
                                                            LastName = x.User != null ? x.User.LAST_NAME : "",
                                                            PositionCode = x.User != null ? x.User.POSITION_CODE : "",
                                                            UserId = x.User != null ? x.User.USER_ID : 0
                                                        }
                                                    }).ToList(),

                IsBlockAllSalesProducts = transHis.SALES_BLOCK_ALL_PRODUCT == Constants.DigitTrue,
                IsBlockSalesEmail = transHis.SALES_BLOCK_EMAIL == Constants.DigitTrue,
                IsBlockSalesSms = transHis.SALES_BLOCK_SMS == Constants.DigitTrue,
                IsBlockSalesTelephone = transHis.SALES_BLOCK_TELEPHONE == Constants.DigitTrue
            };

            return result;
        }

        public int UpdateDoNotCallTelephone(DoNotCallByTelephoneEntity model)
        {
            try
            {
                return UpdateDoNotCallData(model, Constants.DNC.TransactionTypeTelephone);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public DoNotCallByCustomerEntity GetDoNotCallCustomerFromId(int id)
        {
            var transaction = _dncContext.TB_T_DNC_TRANSACTION.FirstOrDefault(c => c.DNC_TRANSACTION_ID == id);
            if (transaction == null) return null;

            return QueryDoNotCallTransactions<DoNotCallByCustomerEntity>(transaction);
        }

        public DoNotCallTransactionEntity FindDoNotCallTransactionById(int transactionId)
        {
            var transaction = _dncContext.TB_T_DNC_TRANSACTION.FirstOrDefault(c => c.DNC_TRANSACTION_ID == transactionId);
            if (transaction == null) return null;

            return QueryDoNotCallTransactions<DoNotCallTransactionEntity>(transaction);
        }

        public DoNotCallByTelephoneEntity GetDoNotCallTelephoneFromTransactionId(int id)
        {
            var transaction = _dncContext.TB_T_DNC_TRANSACTION.FirstOrDefault(c => c.DNC_TRANSACTION_ID == id);
            if (transaction == null) return null;

            return QueryDoNotCallTransactions<DoNotCallByTelephoneEntity>(transaction);
        }

        public DoNotCallByCustomerEntity GetDoNotCallCustomerFromCardId(string cardId)
        {
            var transaction = _dncContext.TB_T_DNC_TRANSACTION.FirstOrDefault(c => c.CARD_NO == cardId);
            if (transaction == null) return null;

            return QueryDoNotCallTransactions<DoNotCallByCustomerEntity>(transaction);
        }

        public bool IsExistingCustomerTransactionCard(string cardNo, int subscriptionTypeId, int transactionId)
        {
            bool isExists = _dncContext.TB_T_DNC_TRANSACTION
                                    .Any(x => (transactionId == 0 || x.DNC_TRANSACTION_ID != transactionId)
                                           && x.SUBSCRIPT_TYPE_ID == subscriptionTypeId
                                           && x.CARD_NO == cardNo
                                    );
            return isExists;
        }

        public List<SubscriptTypeEntity> GetActiveSubscriptionTypes()
        {
            var query = from st in _csmContext.TB_M_SUBSCRIPT_TYPE
                        orderby st.SUBSCRIPT_TYPE_NAME ascending
                        select new SubscriptTypeEntity
                        {
                            SubscriptTypeId = st.SUBSCRIPT_TYPE_ID,
                            SubscriptTypeName = st.SUBSCRIPT_TYPE_NAME,
                            SubscriptTypeCode = st.SUBSCRIPT_TYPE_CODE
                        };

            return query.ToList();
        }

        public List<DoNotCallTransactionModel> SearchDoNotCallBlockByCustomerTransactionExact(string cardNo, int cardTypeId, Pager pager)
        {
            string cardNoInput = cardNo.GetCleanString(toLower: false);

            var transactions = _dncContext.TB_T_DNC_TRANSACTION
                                       .Where(x => x.TRANS_TYPE == Constants.DNC.TransactionTypeCustomer
                                                && x.CARD_NO.Equals(cardNoInput, StringComparison.InvariantCulture)
                                                && x.SUBSCRIPT_TYPE_ID == cardTypeId);

            IQueryable<DoNotCallTransactionModel> query = QueryTransactionModel(transactions);

            query = QueryHelpers.ApplyPaging(query, pager);

            var list = query.ToList();
            var userList = _csmContext.TB_R_USER.ToList();
            foreach (var item in list)
            {
                var u = userList.Where(x => x.USER_ID == item.CreateByUserId).FirstOrDefault();
                item.CreateBy = new DoNotCallUserModel
                {
                    Username = u != null ? u.USERNAME : item.CreateUserName,
                    UserId = u != null ? u.USER_ID : 0,
                    FirstName = u != null ? u.FIRST_NAME : "",
                    LastName = u != null ? u.LAST_NAME : "",
                    PositionCode = u != null ? u.POSITION_CODE : "",
                };
            }

            return list;
        }

        public List<DoNotCallTransactionModel> SearchDoNotCallBlockByTelephoneContact(string phoneNo, string email, Pager pager)
        {
            string phoneInput = phoneNo.GetCleanString();
            string emailInput = email.GetCleanString();

            IQueryable<TB_T_DNC_TRANSACTION> transactions = _dncContext.TB_T_DNC_TRANSACTION.Where(x => x.TRANS_TYPE == Constants.DNC.TransactionTypeTelephone);

            if (phoneInput != string.Empty)
                transactions = transactions.Where(x => x.TB_T_DNC_PHONE_NO.Any(p => p.PHONE_NO.Contains(phoneInput)));
            if (emailInput != string.Empty)
                transactions = transactions.Where(x => x.TB_T_DNC_EMAIL.Any(e => e.EMAIL.Contains(emailInput)));

            IQueryable<DoNotCallTransactionModel> query = QueryTransactionModel(transactions);

            query = QueryHelpers.ApplyPaging(query, pager);
            var list = query.ToList();

            var userList = _csmContext.TB_R_USER.ToList();
            foreach (var item in list)
            {
                var u = userList.Where(x => x.USER_ID == item.CreateByUserId).First();
                item.CreateBy = new DoNotCallUserModel
                {
                    FirstName = u.FIRST_NAME,
                    LastName = u.LAST_NAME,
                    PositionCode = u.POSITION_CODE,
                    UserId = u.USER_ID
                };
            }

            return list;
        }

        public int InsertDoNotCallTelephone(DoNotCallByTelephoneEntity model)
        {
            try
            {
                TB_T_DNC_TRANSACTION transaction = MapNewDoNotCallTypeTelephone(model);
                using (var scope = _dncContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    int transactionId = InsertDoNotCallBlockData(model, transaction);
                    scope.Commit();
                    return transactionId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public bool TransactionExists(int transactionId)
        {
            return _dncContext.TB_T_DNC_TRANSACTION.Any(x => x.DNC_TRANSACTION_ID == transactionId);
        }

        public int UpdateDoNotCallCustomerFromInterface(DoNotCallByCustomerEntity model)
        {
            try
            {
                return UpdateDoNotCallDataFromInterface(model, Constants.DNC.TransactionTypeCustomer);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public int UpdateDoNotCallCustomer(DoNotCallByCustomerEntity model)
        {
            try
            {
                return UpdateDoNotCallData(model, Constants.DNC.TransactionTypeCustomer);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public int InsertDoNotCallCustomer(DoNotCallByCustomerEntity model)
        {
            try
            {
                TB_T_DNC_TRANSACTION transaction = MapNewToDoNotCallTypeCustomer(model);
                using (var scope = _dncContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    int transactionId = InsertDoNotCallBlockData(model, transaction);
                    scope.Commit();
                    return transactionId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public List<DoNotCallExcelModel> SearchDoNotCallListExcelModel(DoNotCallListSearchFilter searchFilter)
        {
            try
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = false;
                List<TB_T_DNC_TRANSACTION> transactions = GetFilteredtransactions(searchFilter).ToList();
                List<TB_R_USER> users = GetFilteredUsers(searchFilter);

                var branches = _csmContext.TB_R_BRANCH.AsNoTracking().ToList();
                var createUsers = (from u in _csmContext.TB_R_USER join b in branches on u.BRANCH_ID equals b.BRANCH_ID select new { User = u, Branch = b }).ToList();
                var updateUsers = (from u in users join b in branches on u.BRANCH_ID equals b.BRANCH_ID select new { User = u, Branch = b }).ToList();

                var activityProducts = _dncContext.TB_T_DNC_ACTIVITY_PRODUCT.AsNoTracking();
                var products = _csmContext.TB_R_PRODUCT.AsNoTracking().ToList();

                var infoActProducts = from aip in activityProducts.Where(x => x.TYPE == Constants.ActivityProductTypeInformation && x.DELETE_STATUS == Constants.DeleteFlagFalse)
                                      join ip in products on aip.PRODUCT_ID equals ip.PRODUCT_ID into infoProducts
                                      from ip in infoProducts.DefaultIfEmpty()
                                      group ip by aip.DNC_ACTIVITY_TYPE_ID into activityGroup
                                      select new
                                      {
                                          ActivityTypeId = activityGroup.Key,
                                          Products = activityGroup.Select(x => x.PRODUCT_NAME)
                                      };

                var salesActProducts = from asp in activityProducts.Where(x => x.TYPE == Constants.ActivityProductTypeSales && x.DELETE_STATUS == Constants.DeleteFlagFalse)
                                       join sp in products on asp.PRODUCT_ID equals sp.PRODUCT_ID into salesProducts
                                       from sp in salesProducts.DefaultIfEmpty()
                                       group sp by asp.DNC_ACTIVITY_TYPE_ID into activityGroup
                                       select new
                                       {
                                           ActivityTypeId = activityGroup.Key,
                                           Products = activityGroup.Select(x => x.PRODUCT_NAME)
                                       };

                var phones = _dncContext.TB_T_DNC_PHONE_NO.AsNoTracking()
                                     .Where(x => x.DELETE_STATUS == Constants.DeleteFlagFalse)
                                     .Select(x => new
                                     {
                                         TransactionId = x.DNC_TRANSACTION_ID,
                                         PhoneNo = x.PHONE_NO
                                     })
                                     .GroupBy(x => x.TransactionId);

                var emails = _dncContext.TB_T_DNC_EMAIL.AsNoTracking()
                                    .Where(x => x.DELETE_STATUS == Constants.DeleteFlagFalse)
                                    .Select(x => new
                                    {
                                        TransactionId = x.DNC_TRANSACTION_ID,
                                        Email = x.EMAIL
                                    }).GroupBy(x => x.TransactionId);

                var itemList = (from t in transactions
                                join cu in createUsers on t.CREATE_USER equals cu.User.USER_ID into creators
                                from cu in creators.DefaultIfEmpty()
                                join uu in updateUsers on t.UPDATE_USER equals uu.User.USER_ID into updators
                                from uu in updators.DefaultIfEmpty()
                                join at in _dncContext.TB_T_DNC_ACTIVITY_TYPE.AsNoTracking() on t.DNC_TRANSACTION_ID equals at.DNC_TRANSACTION_ID // required
                                join sp in salesActProducts on at.DNC_ACTIVITY_TYPE_ID equals sp.ActivityTypeId into spGroup
                                from sp in spGroup.DefaultIfEmpty()
                                join ip in infoActProducts on at.DNC_ACTIVITY_TYPE_ID equals ip.ActivityTypeId into ipGroup
                                from ip in ipGroup.DefaultIfEmpty()
                                join p in phones on t.DNC_TRANSACTION_ID equals p.Key into phoneGroup
                                from p in phoneGroup.DefaultIfEmpty()
                                join e in emails on t.DNC_TRANSACTION_ID equals e.Key into emailGroup
                                from e in emailGroup.DefaultIfEmpty()
                                select new
                                {
                                    Transaction = t,
                                    ActivityType = at,
                                    CreateBy = cu != null ? cu.User.POSITION_CODE + "-" + cu.User.FIRST_NAME + " " + cu.User.LAST_NAME : t.CREATE_USERNAME,
                                    UpdateBy = uu != null ? uu.User.POSITION_CODE + "-" + uu.User.FIRST_NAME + " " + uu.User.LAST_NAME : t.UPDATE_USERNAME,
                                    CreateBranch = cu != null ? cu.Branch.BRANCH_NAME : "",
                                    UpdateBranch = uu != null ? uu.Branch.BRANCH_NAME : "",
                                    SalesProducts = sp != null ? sp.Products : new List<string>(),
                                    InfoProducts = ip != null ? ip.Products : new List<string>(),
                                    PhoneNos = p != null ? p.Select(x => x.PhoneNo) : new List<string>(),
                                    Emails = e != null ? e.Select(x => x.Email) : new List<string>()
                                }).ToList();

                var resultList = new List<DoNotCallExcelModel>();
                var itemNo = 1;

                foreach (var item in itemList)
                {
                    TB_T_DNC_TRANSACTION t = item.Transaction;
                    var at = item.ActivityType;
                    string createByName = item.CreateBy;
                    string updateByName = item.UpdateBy;
                    string salesProducts = string.Join(", ", item.SalesProducts);
                    string infoProducts = string.Join(", ", item.InfoProducts);
                    string expireDate = t.EXPIRY_DATE.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);
                    string createDate = t.CREATE_DATE.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);
                    string updateDate = t.UPDATE_DATE.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);
                    string status = t.STATUS == Constants.DigitTrue ? "Active" : "Inactive";
                    string blockInfoAllProducts = at.INFORMATION_BLOCK_ALL_PRODUCT == Constants.DigitTrue ? "Yes" : "No";
                    string blockInfoEmail = at.INFORMATION_BLOCK_EMAIL == Constants.DigitTrue ? "Yes" : "No";
                    string blockInfoSMS = at.INFORMATION_BLOCK_SMS == Constants.DigitTrue ? "Yes" : "No";
                    string blockInfoTel = at.INFORMATION_BLOCK_TELEPHONE == Constants.DigitTrue ? "Yes" : "No";
                    string blockSalesAllProducts = at.SALES_BLOCK_ALL_PRODUCT == Constants.DigitTrue ? "Yes" : "No";
                    string blockSalesEmail = at.SALES_BLOCK_EMAIL == Constants.DigitTrue ? "Yes" : "No";
                    string blockSalesSMS = at.SALES_BLOCK_SMS == Constants.DigitTrue ? "Yes" : "No";
                    string blockSalesTel = at.SALES_BLOCK_TELEPHONE == Constants.DigitTrue ? "Yes" : "No";
                    string transactionTypeName = t.TRANS_TYPE == Constants.DNC.Customer ? "Customer" : "Telephone";
                    string subscriptionTypeName = _csmContext.TB_M_SUBSCRIPT_TYPE.Where(x => x.SUBSCRIPT_TYPE_ID == t.SUBSCRIPT_TYPE_ID).Select(x => x.SUBSCRIPT_TYPE_NAME).FirstOrDefault();

                    foreach (var e in item.Emails)
                    {
                        var email = new DoNotCallExcelModel
                        {
                            No = itemNo,
                            SubscriptionTypeName = subscriptionTypeName,
                            CardNo = t.CARD_NO,
                            FirstName = t.FIRST_NAME,
                            LastName = t.LAST_NAME,
                            ExpireDate = expireDate,
                            Status = status,
                            Remark = t.REMARK,
                            Email = e,
                            PhoneNo = null,
                            CreateDate = createDate,
                            CreateBy = createByName,
                            CreateBranch = item.CreateBranch,
                            UpdateDate = updateDate,
                            UpdateBy = updateByName,
                            UpdateBranch = item.UpdateBranch,
                            BlockInformationEmail = blockInfoEmail,
                            BlockInformationSms = blockInfoSMS,
                            BlockInformationTelephone = blockInfoTel,
                            BlockSalesEmail = blockSalesEmail,
                            BlockSalesSms = blockSalesSMS,
                            BlockSalesTelephone = blockSalesTel,
                            BlockInfoAllProduct = blockInfoAllProducts,
                            BlockSalesAllProduct = blockSalesAllProducts,
                            TransactionType = transactionTypeName,
                            InfoProducts = infoProducts,
                            SalesProducts = salesProducts,
                            System = t.FROM_SYSTEM
                        };
                        resultList.Add(email);
                        itemNo++;
                    }

                    foreach (var p in item.PhoneNos)
                    {
                        var phone = new DoNotCallExcelModel
                        {
                            No = itemNo,
                            SubscriptionTypeName = subscriptionTypeName,
                            CardNo = t.CARD_NO,
                            FirstName = t.FIRST_NAME,
                            LastName = t.LAST_NAME,
                            ExpireDate = expireDate,
                            Status = status,
                            Remark = t.REMARK,
                            PhoneNo = p,
                            CreateDate = createDate,
                            CreateBy = createByName,
                            CreateBranch = item.CreateBranch,
                            UpdateDate = updateDate,
                            UpdateBy = updateByName,
                            UpdateBranch = item.UpdateBranch,
                            BlockInformationEmail = blockInfoEmail,
                            BlockInformationSms = blockInfoSMS,
                            BlockInformationTelephone = blockInfoTel,
                            BlockSalesEmail = blockSalesEmail,
                            BlockSalesSms = blockSalesSMS,
                            BlockSalesTelephone = blockSalesTel,
                            BlockInfoAllProduct = blockInfoAllProducts,
                            BlockSalesAllProduct = blockSalesAllProducts,
                            TransactionType = transactionTypeName,
                            InfoProducts = infoProducts,
                            SalesProducts = salesProducts,
                            System = t.FROM_SYSTEM
                        };
                        resultList.Add(phone);
                        itemNo++;
                    }
                }

                return resultList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dncContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public List<TransactionInfo> SearchExactDoNotCallTransaction(DoNotCallListSearchFilter searchFilter)
        {
            string cardNo = searchFilter.CardNo.GetCleanString(defaultWhenNull: null);
            string phoneNo = searchFilter.Telephone.GetCleanString(defaultWhenNull: null);
            string email = searchFilter.Email.GetCleanString(defaultWhenNull: null);
            int cisId = searchFilter.CisId.HasValue ? searchFilter.CisId.Value : 0;

            var transactions = _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking()
                                .Where(x => (cisId == 0 || x.KKCIS_ID == cisId)
                                         && (cardNo == null || x.CARD_NO.Equals(cardNo, StringComparison.InvariantCultureIgnoreCase)));

            var emails = _dncContext.TB_T_DNC_EMAIL.AsNoTracking();
            var phones = _dncContext.TB_T_DNC_PHONE_NO.AsNoTracking();

            if (phoneNo != null)
            {
                transactions = from t in transactions
                               join p in phones.Where(x => x.PHONE_NO.Equals(phoneNo))
                               on t.DNC_TRANSACTION_ID equals p.DNC_TRANSACTION_ID
                               select t;
            }
            if (email != null)
            {
                transactions = from t in transactions
                               join e in emails.Where(x => x.EMAIL.Equals(email))
                               on t.DNC_TRANSACTION_ID equals e.DNC_TRANSACTION_ID
                               select t;
            }

            var activityProducts = _dncContext.TB_T_DNC_ACTIVITY_PRODUCT.AsNoTracking();
            const string ACTIVE = "Y";
            const string INACTIVE = "N";
            const string NOT_DELETE = Constants.DeleteFlagFalse;

            var resultData = from t in transactions.OrderByDescending(x => x.CREATE_DATE).Skip(0).Take(searchFilter.PageSize)
                             join a in _dncContext.TB_T_DNC_ACTIVITY_TYPE.AsNoTracking() on t.DNC_TRANSACTION_ID equals a.DNC_TRANSACTION_ID
                             select new TransactionInfo
                             {
                                 EmailList = emails.Where(e => e.DNC_TRANSACTION_ID == t.DNC_TRANSACTION_ID)
                                                   .Select(e => new EmailInfo
                                                   {
                                                       Email = e.EMAIL,
                                                       ActiveStatus = e.DELETE_STATUS == NOT_DELETE ? ACTIVE : INACTIVE
                                                   }).ToList(),
                                 TelephoneList = phones.Where(p => p.DNC_TRANSACTION_ID == t.DNC_TRANSACTION_ID)
                                                       .Select(p => new TelelphoneInfo
                                                       {
                                                           PhoneNo = p.PHONE_NO,
                                                           ActiveStatus = p.DELETE_STATUS == NOT_DELETE ? ACTIVE : INACTIVE
                                                       }).ToList(),
                                 InformationBlockProductList = (from ap in activityProducts.Where(p => p.DNC_ACTIVITY_TYPE_ID == a.DNC_ACTIVITY_TYPE_ID
                                                                                                    && p.TYPE == Constants.ActivityProductTypeInformation)
                                                                select new ProductInfo
                                                                {
                                                                    ProductId = ap.PRODUCT_ID,
                                                                    ActivityProductId = ap.DNC_ACTIVITY_PRODUCT_ID,
                                                                    ActiveStatus = ap.DELETE_STATUS == NOT_DELETE ? ACTIVE : INACTIVE
                                                                }).ToList(),
                                 SalesBlockProductList = (from ap in activityProducts.Where(p => p.DNC_ACTIVITY_TYPE_ID == a.DNC_ACTIVITY_TYPE_ID
                                                                                              && p.TYPE == Constants.ActivityProductTypeSales)
                                                          select new ProductInfo
                                                          {
                                                              ProductId = ap.PRODUCT_ID,
                                                              ActivityProductId = ap.DNC_ACTIVITY_PRODUCT_ID,
                                                              ActiveStatus = ap.DELETE_STATUS == NOT_DELETE ? ACTIVE : INACTIVE
                                                          }).ToList(),
                                 BlockLevelCode = Constants.DNC.TransactionTypeCustomer,
                                 BlockLevelDesc = Constants.DNC.Customer,
                                 CardNo = t.CARD_NO,
                                 CisId = t.KKCIS_ID != null ? t.KKCIS_ID.Value : (decimal?)null,
                                 ExpiryDate = t.EXPIRY_DATE,
                                 FirstName = t.FIRST_NAME,
                                 LastName = t.LAST_NAME,
                                 InformationBlockAllProduct = a.INFORMATION_BLOCK_ALL_PRODUCT,
                                 InformationBlockEmail = a.INFORMATION_BLOCK_EMAIL,
                                 InformationBlockSMS = a.INFORMATION_BLOCK_SMS,
                                 InformationBlockTelephone = a.INFORMATION_BLOCK_TELEPHONE,
                                 SalesBlockAllProduct = a.SALES_BLOCK_ALL_PRODUCT,
                                 SalesBlockEmail = a.SALES_BLOCK_EMAIL,
                                 SalesBlockSMS = a.SALES_BLOCK_SMS,
                                 SalesBlockTelephone = a.SALES_BLOCK_TELEPHONE,
                                 StatusCode = t.STATUS,
                                 StatusDesc = t.STATUS == Constants.DigitTrue ? "Active" : "Inactive",
                                 SubscriptTypeId = t.SUBSCRIPT_TYPE_ID,
                                 SystemCode = t.FROM_SYSTEM,
                                 TransactionDate = t.CREATE_DATE.Value,
                                 TransactionId = t.DNC_TRANSACTION_ID
                             };

            var list = resultData.ToList();
            var subscriptTypes = _csmContext.TB_M_SUBSCRIPT_TYPE.AsNoTracking().ToList();
            string productCode = searchFilter.ProductCode.GetCleanString(toLower: false, defaultWhenNull: null);
            var products = _csmContext.TB_R_PRODUCT.AsNoTracking().Select(x => new { x.PRODUCT_CODE, x.PRODUCT_ID, x.PRODUCT_NAME });
            if (productCode != null)
            {
                var product = products.Where(p => p.PRODUCT_CODE == productCode).FirstOrDefault();

                if (product == null || product.PRODUCT_ID == 0)
                    throw new NullReferenceException($"Product code {productCode} not found.");

                list = list.Where(x => x.SalesBlockProductList.Any(s => s.ProductId == product.PRODUCT_ID)
                                    || x.InformationBlockProductList.Any(i => i.ProductId == product.PRODUCT_ID)).ToList();
            }

            foreach (var item in list)
            {
                item.SubscriptTypeName = subscriptTypes.Where(x => x.SUBSCRIPT_TYPE_ID == item.SubscriptTypeId).Select(x => x.SUBSCRIPT_TYPE_NAME).FirstOrDefault();
                foreach (var infoProduct in item.InformationBlockProductList)
                {
                    var product = products.First(x => x.PRODUCT_ID == infoProduct.ProductId);
                    infoProduct.ProductCode = product.PRODUCT_CODE;
                    infoProduct.ProductName = product.PRODUCT_NAME;
                }

                foreach (var salesProduct in item.SalesBlockProductList)
                {
                    var product = products.First(x => x.PRODUCT_ID == salesProduct.ProductId);
                    salesProduct.ProductCode = product.PRODUCT_CODE;
                    salesProduct.ProductName = product.PRODUCT_NAME;
                }
            };
            return list;
        }

        public List<DoNotCallEntity> SearchDoNotCallList(DoNotCallListSearchFilter searchFilter)
        {
            IQueryable<TB_T_DNC_TRANSACTION> transactions = GetFilteredtransactions(searchFilter);
            List<TB_R_USER> users = GetFilteredUsers(searchFilter);

            // inner join user if search with user
            if (searchFilter.UpdateBranchId.HasValue || searchFilter.UpdateUser.HasValue)
            {
                transactions = transactions.Join(users,
                                                 transaction => transaction.CREATE_USER,
                                                 user => user.USER_ID,
                                                 (transaction, user) => transaction);
            }

            var query = from t in transactions
                        let email = t.TB_T_DNC_EMAIL.OrderBy(x => x.DELETE_STATUS).ThenByDescending(x => x.UPDATE_DATE).FirstOrDefault()
                        let tel = t.TB_T_DNC_PHONE_NO.OrderBy(x => x.DELETE_STATUS).ThenByDescending(x => x.UPDATE_DATE).FirstOrDefault()
                        select new DoNotCallEntity
                        {
                            CardNo = t.CARD_NO,
                            CreateByUserId = t.CREATE_USER,
                            CreateByUsername = t.CREATE_USERNAME,
                            FirstName = t.FIRST_NAME,
                            LastName = t.LAST_NAME,
                            SubscriptionTypeId = t.SUBSCRIPT_TYPE_ID,
                            BlockInformationEmail = t.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault().INFORMATION_BLOCK_EMAIL,
                            BlockInformationSms = t.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault().INFORMATION_BLOCK_SMS,
                            BlockInformationTelephone = t.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault().INFORMATION_BLOCK_TELEPHONE,
                            BlockSalesEmail = t.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault().SALES_BLOCK_EMAIL,
                            BlockSalesSms = t.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault().SALES_BLOCK_SMS,
                            BlockSalesTelephone = t.TB_T_DNC_ACTIVITY_TYPE.FirstOrDefault().SALES_BLOCK_TELEPHONE,
                            Status = t.STATUS,
                            TransactionDate = t.UPDATE_DATE.Value,
                            TransactionId = t.DNC_TRANSACTION_ID,
                            TransactionType = t.TRANS_TYPE,
                            Email = email != null ? email.EMAIL : "",
                            Telephone = tel != null ? tel.PHONE_NO : "",
                        };

            var resultData = query;

            resultData = QueryHelpers.SortIQueryable(resultData, searchFilter.SortField, searchFilter.SortOrder == Constants.SortOrderDesc);
            resultData = QueryHelpers.ApplyPaging(resultData, searchFilter);

            var list = resultData.ToList();
            var userList = _csmContext.TB_R_USER.ToList();
            var subscriptTypes = _csmContext.TB_M_SUBSCRIPT_TYPE.AsNoTracking().ToList();
            foreach (var item in list)
            {
                var u = userList.Where(x => x.USER_ID == item.CreateByUserId).FirstOrDefault();
                item.SubscriptionType = subscriptTypes.Where(x => x.SUBSCRIPT_TYPE_ID == item.SubscriptionTypeId).Select(x => x.SUBSCRIPT_TYPE_NAME).FirstOrDefault();
                item.CreateBy = new DoNotCallUserModel
                {
                    Username = u != null ? u.USERNAME : item.CreateByUsername,
                    UserId = u != null ? u.USER_ID : 0,
                    FirstName = u != null ? u.FIRST_NAME : "",
                    LastName = u != null ? u.LAST_NAME : "",
                    PositionCode = u != null ? u.POSITION_CODE : "",
                };
            }

            return list;
        }

        #region Private

        private List<DoNotCallProductModel> GetDoNotCallProducts()
        {
            var resultData = from ap in _dncContext.TB_T_DNC_ACTIVITY_PRODUCT
                             select new DoNotCallProductModel
                             {
                                 ActivityProductType = ap.TYPE,
                                 ProductId = ap.PRODUCT_ID,
                                 CreateDate = ap.CREATE_DATE.Value,
                                 CreateByUsername = ap.CREATE_USERNAME,
                                 CreateByUserId = ap.CREATE_USER,
                                 Id = ap.PRODUCT_ID,
                                 IsDeleted = ap.DELETE_STATUS == Constants.DeleteFlagTrue,
                                 ActivityTypeId = ap.DNC_ACTIVITY_TYPE_ID,
                                 ActivityProductId = ap.DNC_ACTIVITY_PRODUCT_ID,
                                 UpdateDate = ap.UPDATE_DATE.Value,
                             };

            var list = resultData.ToList();
            var userList = _csmContext.TB_R_USER.ToList();
            var products = _csmContext.TB_R_PRODUCT.ToList();
            foreach (var item in list)
            {
                item.Name = products.Where(x => x.PRODUCT_ID == item.ProductId).Select(x => x.PRODUCT_NAME).FirstOrDefault();
                var u = userList.Where(x => x.USER_ID == item.CreateByUserId).FirstOrDefault();
                item.UpdateBy = new DoNotCallUserModel
                {
                    Username = u != null ? u.USERNAME : item.CreateByUsername,
                    UserId = u != null ? u.USER_ID : 0,
                    FirstName = u != null ? u.FIRST_NAME : "",
                    LastName = u != null ? u.LAST_NAME : "",
                    PositionCode = u != null ? u.POSITION_CODE : "",
                };
            }

            return list;
        }

        private T QueryDoNotCallTransactions<T>(TB_T_DNC_TRANSACTION transaction) where T : DoNotCallTransactionEntity, new()
        {
            int transactionId = transaction.DNC_TRANSACTION_ID;
            var _commonDataAccess = new CommonDataAccess(_csmContext);
            string neverExpireDate = _commonDataAccess.GetParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime nonExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDate, "yyyy-MM-dd").Value;

            var products = GetDoNotCallProducts();
            var users = _csmContext.TB_R_USER.Where(x => x.USER_ID == transaction.CREATE_USER || x.USER_ID == transaction.UPDATE_USER);
            var createBy = users.Where(x => x.USER_ID == transaction.CREATE_USER).FirstOrDefault();
            var updateBy = users.Where(x => x.USER_ID == transaction.UPDATE_USER).FirstOrDefault();
            var activityType = transaction.TB_T_DNC_ACTIVITY_TYPE.First();
            var isTrue = Constants.DigitTrue;

            return new T
            {
                CardInfo = new DoNotCallCardInfoModel
                {
                    CardNo = transaction.CARD_NO,
                    SubscriptionTypeId = transaction.SUBSCRIPT_TYPE_ID.HasValue ? transaction.SUBSCRIPT_TYPE_ID.Value : 0
                },
                BasicInfo = new DoNotCallBasicInfoModel
                {
                    TransactionId = transaction.DNC_TRANSACTION_ID,
                    CreateBy = new DoNotCallUserModel
                    {
                        Username = createBy != null ? createBy.USERNAME : transaction.CREATE_USERNAME,
                        UserId = createBy != null ? createBy.USER_ID : 0,
                        FirstName = createBy != null ? createBy.FIRST_NAME : "",
                        LastName = createBy != null ? createBy.LAST_NAME : "",
                        PositionCode = createBy != null ? createBy.POSITION_CODE : "",
                    },
                    UpdateBy = new DoNotCallUserModel
                    {
                        Username = updateBy != null ? updateBy.USERNAME : transaction.UPDATE_USERNAME,
                        UserId = updateBy != null ? updateBy.USER_ID : 0,
                        FirstName = updateBy != null ? updateBy.FIRST_NAME : "",
                        LastName = updateBy != null ? updateBy.LAST_NAME : "",
                        PositionCode = updateBy != null ? updateBy.POSITION_CODE : "",
                    },
                    CreateDate = transaction.CREATE_DATE.Value,
                    UpdateDate = transaction.UPDATE_DATE.Value,
                    EffectiveDate = transaction.EFFECTIVE_DATE,
                    ExpireDate = transaction.EXPIRY_DATE,
                    FromSystem = transaction.FROM_SYSTEM,
                    FirstName = transaction.FIRST_NAME,
                    LastName = transaction.LAST_NAME,
                    IsActive = transaction.STATUS == isTrue,
                    IsNeverExpire = transaction.EXPIRY_DATE.Date == nonExpireDate.Date,
                    Remark = transaction.REMARK
                },
                BlockInformation = new DoNotCallBlockInformationModel
                {
                    BlockInfoProductList = products.Where(x => x.ActivityProductType == Constants.ActivityProductTypeInformation
                                                            && x.ActivityTypeId == activityType.DNC_ACTIVITY_TYPE_ID).ToList(),
                    IsBlockAllInfoProducts = activityType.INFORMATION_BLOCK_ALL_PRODUCT == isTrue,
                    IsBlockInfoEmail = activityType.INFORMATION_BLOCK_EMAIL == isTrue,
                    IsBlockInfoSms = activityType.INFORMATION_BLOCK_SMS == isTrue,
                    IsBlockInfoTelephone = activityType.INFORMATION_BLOCK_TELEPHONE == isTrue
                },
                BlockSales = new DoNotCallBlockSalesModel
                {
                    BlockSalesProductList = products.Where(x => x.ActivityProductType == Constants.ActivityProductTypeSales
                                                             && x.ActivityTypeId == activityType.DNC_ACTIVITY_TYPE_ID).ToList(),
                    IsBlockAllSalesProducts = activityType.SALES_BLOCK_ALL_PRODUCT == isTrue,
                    IsBlockSalesEmail = activityType.SALES_BLOCK_EMAIL == isTrue,
                    IsBlockSalesSms = activityType.SALES_BLOCK_SMS == isTrue,
                    IsBlockSalesTelephone = activityType.SALES_BLOCK_TELEPHONE == isTrue
                },
                ContactDetail = new DoNotCallContactModel
                {
                    Email = new DoNotCallEmail
                    {
                        EmailList = (from e in transaction.TB_T_DNC_EMAIL
                                     join u in _csmContext.TB_R_USER on e.UPDATE_USER equals u.USER_ID into emailUsers
                                     from u in emailUsers.DefaultIfEmpty()
                                     select new DoNotCallEmailModel
                                     {
                                         Id = e.DNC_EMAIL_ID,
                                         Email = e.EMAIL,
                                         LastUpdateDate = e.UPDATE_DATE.Value,
                                         IsDeleted = e.DELETE_STATUS == Constants.DeleteFlagTrue,
                                         UpdateBy = new DoNotCallUserModel
                                         {
                                             Username = u != null ? u.USERNAME : e.UPDATE_USERNAME,
                                             UserId = u != null ? u.USER_ID : 0,
                                             FirstName = u != null ? u.FIRST_NAME : "",
                                             LastName = u != null ? u.LAST_NAME : "",
                                             PositionCode = u != null ? u.POSITION_CODE : ""
                                         }
                                     }).ToList()
                    },
                    Telephone = new DoNotCallTelephone
                    {
                        TelephoneList = (from p in transaction.TB_T_DNC_PHONE_NO
                                         join u in _csmContext.TB_R_USER on p.UPDATE_USER equals u.USER_ID into phoneUsers
                                         from u in phoneUsers.DefaultIfEmpty()
                                         select new DoNotCallTelephoneModel
                                         {
                                             Id = p.DNC_PHONE_NO_ID,
                                             PhoneNo = p.PHONE_NO,
                                             IsDeleted = p.DELETE_STATUS == Constants.DeleteFlagTrue,
                                             LastUpdateDate = p.UPDATE_DATE.Value,
                                             UpdateBy = new DoNotCallUserModel
                                             {
                                                 Username = u != null ? u.USERNAME : p.UPDATE_USERNAME,
                                                 UserId = u != null ? u.USER_ID : 0,
                                                 FirstName = u != null ? u.FIRST_NAME : "",
                                                 LastName = u != null ? u.LAST_NAME : "",
                                                 PositionCode = u != null ? u.POSITION_CODE : "",
                                             }
                                         }).ToList()
                    }
                }
            };
        }

        private IQueryable<DoNotCallTransactionModel> QueryTransactionModel(IQueryable<TB_T_DNC_TRANSACTION> transactions)
        {
            var query = from t in transactions
                        let email = t.TB_T_DNC_EMAIL.OrderBy(x => x.DELETE_STATUS).ThenByDescending(x => x.UPDATE_DATE).FirstOrDefault()
                        let tel = t.TB_T_DNC_PHONE_NO.OrderBy(x => x.DELETE_STATUS).ThenByDescending(x => x.UPDATE_DATE).FirstOrDefault()
                        select new DoNotCallTransactionModel
                        {
                            CardNo = t.CARD_NO,
                            CreateByUserId = t.CREATE_USER,
                            CreateUserName = t.CREATE_USERNAME,
                            CreateDate = t.CREATE_DATE.Value,
                            FirstName = t.FIRST_NAME,
                            LastName = t.LAST_NAME,
                            Id = t.DNC_TRANSACTION_ID,
                            Email = email != null ? email.EMAIL : "",
                            PhoneNo = tel != null ? tel.PHONE_NO : "",
                            IsActive = t.STATUS == Constants.DigitTrue,
                        };

            return query.OrderByDescending(x => x.CreateDate);
        }

        private int UpdateDoNotCallDataFromInterface(DoNotCallTransactionEntity model, string type)
        {
            int transactionId = model.BasicInfo.TransactionId;
            int userId = model.CurrentUserId;
            string username = model.CurrentUsername;
            DateTime now = DateTime.Now;
            using (var scope = _dncContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                TB_T_DNC_TRANSACTION transaction = _dncContext.TB_T_DNC_TRANSACTION.Where(c => c.DNC_TRANSACTION_ID == transactionId).SingleOrDefault();
                if (transaction == null) throw new NullReferenceException($"transaction ID: {transactionId} not found.");
                UpdateTransactionDetails(model, now, transaction);

                TB_T_DNC_ACTIVITY_TYPE activityType = _dncContext.TB_T_DNC_ACTIVITY_TYPE.Where(x => x.DNC_TRANSACTION_ID == transactionId).SingleOrDefault();
                if (activityType == null) throw new NullReferenceException($"Do Not Call Activity Type for transaction ID: {transactionId} not found.");
                UpdateActivityTypeDetails(model, now, activityType);

                var phoneList = model.ContactDetail.Telephone.TelephoneList;
                UpdateTransactionPhoneListFromInterface(transactionId, userId, username, phoneList);

                var emailList = model.ContactDetail.Email.EmailList;
                UpdateTransactionEmailListFromInterface(transactionId, userId, username, emailList);

                var salesProductList = model.BlockSales.BlockSalesProductList;
                var infoProductList = model.BlockInformation.BlockInfoProductList;
                UpdateTransactionProductList(userId, username, activityType, salesProductList, infoProductList);

                // Log
                AddTransactionHistory(transaction, userId, username, activityType, phoneList, emailList, salesProductList, infoProductList);

                _dncContext.SaveChanges();
                scope.Commit();
                return transactionId;
            }
        }

        private int UpdateDoNotCallData(DoNotCallTransactionEntity model, string type)
        {
            int transactionId = model.BasicInfo.TransactionId;
            int userId = model.CurrentUserId;
            string username = model.CurrentUsername;
            DateTime now = DateTime.Now;
            using (var scope = _dncContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                TB_T_DNC_TRANSACTION transaction = _dncContext.TB_T_DNC_TRANSACTION.Where(c => c.DNC_TRANSACTION_ID == transactionId).SingleOrDefault();
                if (transaction == null) throw new NullReferenceException($"transaction ID: {transactionId} not found.");
                UpdateTransactionDetails(model, now, transaction);

                TB_T_DNC_ACTIVITY_TYPE activityType = _dncContext.TB_T_DNC_ACTIVITY_TYPE.Where(x => x.DNC_TRANSACTION_ID == transactionId).SingleOrDefault();
                if (activityType == null) throw new NullReferenceException($"Do Not Call Activity Type for transaction ID: {transactionId} not found.");
                UpdateActivityTypeDetails(model, now, activityType);

                var phoneList = model.ContactDetail.Telephone.TelephoneList;
                UpdateTransactionPhoneList(transactionId, userId, username, phoneList);

                var emailList = model.ContactDetail.Email.EmailList;
                UpdateTransactionEmailList(transactionId, userId, username, emailList);

                var salesProductList = model.BlockSales.BlockSalesProductList;
                var infoProductList = model.BlockInformation.BlockInfoProductList;
                UpdateTransactionProductList(userId, username, activityType, salesProductList, infoProductList);

                // Log
                AddTransactionHistory(transaction, userId, username, activityType, phoneList, emailList, salesProductList, infoProductList);

                _dncContext.SaveChanges();
                scope.Commit();
                return transactionId;
            }
        }

        private void UpdateTransactionProductList(int userId, string username, TB_T_DNC_ACTIVITY_TYPE activityType, List<DoNotCallProductModel> salesProductList, List<DoNotCallProductModel> infoProductList)
        {
            int activityTypeId = activityType.DNC_ACTIVITY_TYPE_ID;
            // Delete product list 
            var deletedProducts = new List<DoNotCallProductModel>();
            bool isBlockAllSalesProducts = activityType.SALES_BLOCK_ALL_PRODUCT == Constants.DigitTrue;
            bool isBlockAllInfoProducts = activityType.INFORMATION_BLOCK_ALL_PRODUCT == Constants.DigitTrue;

            if (!isBlockAllSalesProducts)
            {
                // Insert block sales product list
                var newSalesProducts = salesProductList.Where(x => x.ActivityProductId == 0);
                foreach (DoNotCallProductModel product in newSalesProducts)
                {
                    InsertNewActivityProduct(userId, username, activityTypeId, product, Constants.ActivityProductTypeSales);
                }
            }

            if (!isBlockAllInfoProducts)
            {
                // Insert block information product list
                var newInfoProducts = infoProductList.Where(x => x.ActivityProductId == 0);
                foreach (DoNotCallProductModel product in newInfoProducts)
                {
                    InsertNewActivityProduct(userId, username, activityTypeId, product, Constants.ActivityProductTypeInformation);
                }
            }

            // Sales
            deletedProducts.AddRange(salesProductList.Where(x => x.IsDeleted && x.ActivityProductId > 0));
            // Info
            deletedProducts.AddRange(infoProductList.Where(x => x.IsDeleted && x.ActivityProductId > 0));

            if (deletedProducts.Count > 0 || isBlockAllSalesProducts || isBlockAllInfoProducts)
            {
                DeleteTransactionProducts(deletedProducts, userId, username, isBlockAllSalesProducts, isBlockAllInfoProducts);
            }
        }
        
        private void UpdateTransactionEmailListFromInterface(int transactionId, int userId, string username, List<DoNotCallEmailModel> emailList)
        {
            // Insert email list
            var newEmails = emailList.Where(x => x.Id == 0);

            foreach (DoNotCallEmailModel emailModel in newEmails)
            {
                InsertNewEmail(emailModel, userId, transactionId, username);
            }
            // Delete emails 
            var deletedEmails = emailList.Where(x => x.IsDeleted && x.Id > 0);
            if (deletedEmails.Any())
            {
                DeleteTransactionEmail(deletedEmails, userId, username, transactionId);
            }
        }

        private void UpdateTransactionEmailList(int transactionId, int userId, string username, List<DoNotCallEmailModel> emailList)
        {
            // Insert email list
            var newEmails = emailList.Where(x => x.Id == 0 && !x.IsDeleted);
            foreach (DoNotCallEmailModel emailModel in newEmails)
            {
                InsertNewEmail(emailModel, userId, transactionId, username);
            }
            // Delete emails 
            var deletedEmails = emailList.Where(x => x.IsDeleted);
            if (deletedEmails.Any())
            {
                DeleteTransactionEmail(deletedEmails, userId, username, transactionId);
            }
        }

        private void UpdateTransactionPhoneListFromInterface(int transactionId, int userId, string username, List<DoNotCallTelephoneModel> phoneList)
        {
            var newPhones = phoneList.Where(x => x.Id == 0);

            // Insert phone no list
            foreach (DoNotCallTelephoneModel phoneModel in newPhones)
            {
                InsertNewPhoneNo(phoneModel, userId, transactionId, username);
            }
            // Delete existing phone no list
            var deletedPhones = phoneList.Where(x => x.IsDeleted && x.Id > 0);
            if (deletedPhones.Any())
            {
                DeleteTransactionTelephones(deletedPhones, userId, username, transactionId);
            }
        }

        private void UpdateTransactionPhoneList(int transactionId, int userId, string username, List<DoNotCallTelephoneModel> phoneList)
        {
            var newPhones = phoneList.Where(x => x.Id == 0 && !x.IsDeleted);

            // Insert phone no list
            foreach (DoNotCallTelephoneModel phoneModel in newPhones)
            {
                InsertNewPhoneNo(phoneModel, userId, transactionId, username);
            }
            // Delete phone no list
            var deletedPhones = phoneList.Where(x => x.IsDeleted);
            if (deletedPhones.Any())
            {
                DeleteTransactionTelephones(deletedPhones, userId, username, transactionId);
            }
        }

        private void UpdateActivityTypeDetails(DoNotCallTransactionEntity model, DateTime now, TB_T_DNC_ACTIVITY_TYPE activityType)
        {
            activityType.UPDATE_DATE = now;
            activityType.UPDATE_USER = model.CurrentUserId;
            activityType.UPDATE_USERNAME = model.CurrentUsername;
            var info = model.BlockInformation;
            activityType.INFORMATION_BLOCK_ALL_PRODUCT = BoolToDigit(info.IsBlockAllInfoProducts);
            activityType.INFORMATION_BLOCK_EMAIL = BoolToDigit(info.IsBlockInfoEmail);
            activityType.INFORMATION_BLOCK_SMS = BoolToDigit(info.IsBlockInfoSms);
            activityType.INFORMATION_BLOCK_TELEPHONE = BoolToDigit(info.IsBlockInfoTelephone);
            var sales = model.BlockSales;
            activityType.SALES_BLOCK_ALL_PRODUCT = BoolToDigit(sales.IsBlockAllSalesProducts);
            activityType.SALES_BLOCK_EMAIL = BoolToDigit(sales.IsBlockSalesEmail);
            activityType.SALES_BLOCK_SMS = BoolToDigit(sales.IsBlockSalesSms);
            activityType.SALES_BLOCK_TELEPHONE = BoolToDigit(sales.IsBlockSalesTelephone);
        }

        private void UpdateTransactionDetails(DoNotCallTransactionEntity model, DateTime now, TB_T_DNC_TRANSACTION transaction)
        {
            transaction.FIRST_NAME = model.BasicInfo.FirstName;
            transaction.LAST_NAME = model.BasicInfo.LastName;
            transaction.UPDATE_DATE = now;
            transaction.UPDATE_USER = model.CurrentUserId;
            transaction.FROM_SYSTEM = model.BasicInfo.FromSystem;
            transaction.UPDATE_USERNAME = model.CurrentUsername;
            transaction.EFFECTIVE_DATE = model.BasicInfo.EffectiveDate;
            DateTime expireDate = model.BasicInfo.ExpireDate;
            transaction.EXPIRY_DATE = expireDate;
            transaction.STATUS = BoolToDigit(expireDate.Date >= now.Date);
            transaction.REMARK = model.BasicInfo.Remark;
            transaction.SUBSCRIPT_TYPE_ID = model.CardInfo.SubscriptionTypeId;
        }

        private int InsertDoNotCallBlockData(DoNotCallTransactionEntity model, TB_T_DNC_TRANSACTION transaction)
        {
            // Insert transaction
            int transactionId = InsertNewtransaction(transaction);
            var createDate = transaction.CREATE_DATE.Value;
            int userId = model.CurrentUserId;
            string username = model.CurrentUsername;

            // Insert activity type
            TB_T_DNC_ACTIVITY_TYPE activityType = InsertNewActivityType(model, createDate, userId, transactionId);
            int activityTypeId = activityType.DNC_ACTIVITY_TYPE_ID;

            // Insert phone no list
            var newPhones = model.ContactDetail.Telephone.TelephoneList;
            foreach (DoNotCallTelephoneModel phoneModel in newPhones)
            {
                var phone = InsertNewPhoneNo(phoneModel, userId, transactionId, username);
            }
            // Insert email list
            var newEmails = model.ContactDetail.Email.EmailList;
            foreach (DoNotCallEmailModel emailModel in newEmails)
            {
                var email = InsertNewEmail(emailModel, userId, transactionId, username);
            }
            // Insert block sales product lisst
            var newSalesBlockProducts = model.BlockSales.BlockSalesProductList;
            foreach (DoNotCallProductModel product in newSalesBlockProducts)
            {
                InsertNewActivityProduct(userId, username, activityTypeId, product, Constants.ActivityProductTypeSales);
            }
            // Insert block information product lisst
            var newInfoBlockProducts = model.BlockInformation.BlockInfoProductList;
            foreach (DoNotCallProductModel product in newInfoBlockProducts)
            {
                InsertNewActivityProduct(userId, username, activityTypeId, product, Constants.ActivityProductTypeInformation);
            }

            // Insert transaction his
            AddTransactionHistory(transaction, userId, username, activityType, newPhones, newEmails, newSalesBlockProducts, newInfoBlockProducts);

            _dncContext.SaveChanges();

            return transactionId;
        }

        private void AddTransactionHistory(
            TB_T_DNC_TRANSACTION transaction,
            int userId,
            string username,
            TB_T_DNC_ACTIVITY_TYPE activityType,
            List<DoNotCallTelephoneModel> newPhones,
            List<DoNotCallEmailModel> newEmails,
            List<DoNotCallProductModel> newSalesBlockProducts,
            List<DoNotCallProductModel> newInfoBlockProducts)
        {
            DateTime logDate = DateTime.Now;

            int transHisId = InsertTransactionHistory(transaction, activityType, logDate, userId, username).DNC_TRANSACTION_HIS_ID;

            // Insert email his
            InsertTransactionEmailHis(userId, username, newEmails, transHisId);

            // Insert phone his
            InsertTransactionPhoneHis(userId, username, newPhones, transHisId);

            // Insert product his
            InsertTransactionProductHis(userId, username, newSalesBlockProducts, transHisId, Constants.ActivityProductTypeSales, logDate);
            InsertTransactionProductHis(userId, username, newInfoBlockProducts, transHisId, Constants.ActivityProductTypeInformation, logDate);
        }

        private void InsertTransactionProductHis(int userId, string username, List<DoNotCallProductModel> products, int transHisId, string type, DateTime logDate)
        {
            foreach (var prod in products)
            {
                var sProdHis = new TB_T_DNC_TRANSACTION_HIS_PRODUCT
                {
                    CREATE_DATE = logDate,
                    CREATE_USER = userId,
                    CREATE_USERNAME = username,
                    DELETE_STATUS = prod.IsDeleted ? Constants.DeleteFlagTrue : Constants.DeleteFlagFalse,
                    DNC_TRANSACTION_HIS_ID = transHisId,
                    PRODUCT_ID = prod.Id,
                    TYPE = type
                };

                _dncContext.TB_T_DNC_TRANSACTION_HIS_PRODUCT.Add(sProdHis);
            }
        }

        private void InsertTransactionPhoneHis(int userId, string username, List<DoNotCallTelephoneModel> newPhones, int transHisId)
        {
            foreach (var phone in newPhones)
            {
                var phoneHis = new TB_T_DNC_TRANSACTION_HIS_PHONE
                {
                    CREATE_DATE = phone.LastUpdateDate,
                    CREATE_USER = userId,
                    CREATE_USERNAME = username,
                    DELETE_STATUS = phone.IsDeleted ? Constants.DeleteFlagTrue : Constants.DeleteFlagFalse,
                    DNC_TRANSACTION_HIS_ID = transHisId,
                    PHONE_NO = phone.PhoneNo
                };
                _dncContext.TB_T_DNC_TRANSACTION_HIS_PHONE.Add(phoneHis);
            }
        }

        private void SetEntryStateModified(object entity)
        {
            if (_dncContext.Configuration.AutoDetectChangesEnabled == false)
            {
                // Set state to Modified
                _dncContext.Entry(entity).State = EntityState.Modified;
            }
        }

        private void InsertTransactionEmailHis(int userId, string username, List<DoNotCallEmailModel> newEmails, int transHisId)
        {
            foreach (var email in newEmails)
            {
                var emailHis = new TB_T_DNC_TRANSACTION_HIS_EMAIL
                {
                    CREATE_DATE = email.LastUpdateDate,
                    CREATE_USER = userId,
                    CREATE_USERNAME = username,
                    DELETE_STATUS = email.IsDeleted ? Constants.DeleteFlagTrue : Constants.DeleteFlagFalse,
                    DNC_TRANSACTION_HIS_ID = transHisId,
                    EMAIL = email.Email
                };

                _dncContext.TB_T_DNC_TRANSACTION_HIS_EMAIL.Add(emailHis);
            }
        }

        private TB_T_DNC_TRANSACTION_HIS InsertTransactionHistory(TB_T_DNC_TRANSACTION tran, TB_T_DNC_ACTIVITY_TYPE activityType, DateTime logDate, int userId, string username)
        {
            var entity = new TB_T_DNC_TRANSACTION_HIS
            {
                // Transaction
                DNC_TRANSACTION_ID = tran.DNC_TRANSACTION_ID,
                SUBSCRIPT_TYPE_ID = tran.SUBSCRIPT_TYPE_ID,
                CARD_NO = tran.CARD_NO,
                FIRST_NAME = tran.FIRST_NAME,
                LAST_NAME = tran.LAST_NAME,
                EFFECTIVE_DATE = tran.EFFECTIVE_DATE,
                EXPIRY_DATE = tran.EXPIRY_DATE,
                FROM_SYSTEM = tran.FROM_SYSTEM,
                REMARK = tran.REMARK,
                STATUS = tran.STATUS,
                CREATE_DATE = logDate,
                CREATE_USER = userId,
                CREATE_USERNAME = username,
                TRANS_TYPE = tran.TRANS_TYPE,
                // Activity Type
                INFORMATION_BLOCK_ALL_PRODUCT = activityType.INFORMATION_BLOCK_ALL_PRODUCT,
                SALES_BLOCK_ALL_PRODUCT = activityType.SALES_BLOCK_ALL_PRODUCT,
                INFORMATION_BLOCK_EMAIL = activityType.INFORMATION_BLOCK_EMAIL,
                SALES_BLOCK_EMAIL = activityType.SALES_BLOCK_EMAIL,
                INFORMATION_BLOCK_TELEPHONE = activityType.INFORMATION_BLOCK_TELEPHONE,
                SALES_BLOCK_TELEPHONE = activityType.SALES_BLOCK_TELEPHONE,
                INFORMATION_BLOCK_SMS = activityType.INFORMATION_BLOCK_SMS,
                SALES_BLOCK_SMS = activityType.SALES_BLOCK_SMS,
            };

            _dncContext.TB_T_DNC_TRANSACTION_HIS.Add(entity);
            int saveCount = _dncContext.SaveChanges();
            int id = entity.DNC_TRANSACTION_HIS_ID;
            if (saveCount == 0 || id == 0)
                throw new DataException($"Insert {nameof(TB_T_DNC_TRANSACTION_HIS)} failed.");

            return entity;
        }

        private IQueryable<TB_T_DNC_LOAD_LIST> FilterUploadList(DoNotCallLoadListSearchFilter model, IQueryable<TB_T_DNC_LOAD_LIST> loadList)
        {
            string fileName = model.FileName.GetCleanString();
            if (fileName != string.Empty)
            {
                loadList = loadList.Where(x => x.FILE_NAME.Contains(fileName));
            }
            string statusId = model.FileStatusId.GetCleanString();
            if (statusId != string.Empty)
            {
                loadList = loadList.Where(x => x.STATUS == statusId);
            }
            if (model.FromDate.HasValue)
            {
                DateTime fromDate = model.FromDate.Value.Date; // at 00:00 am
                loadList = loadList.Where(c => c.CREATE_DATE >= fromDate);
            }
            if (model.ToDate.HasValue)
            {
                DateTime toDate = model.ToDate.Value.AddDays(1).Date; // before 00:00 next date
                loadList = loadList.Where(c => c.UPDATE_DATE < toDate);
            }

            return loadList;
        }

        private TB_T_DNC_ACTIVITY_PRODUCT InsertNewActivityProduct(int userId, string username, int activityTypeId, DoNotCallProductModel product, string type)
        {
            TB_T_DNC_ACTIVITY_PRODUCT entity = MapNewActivityProductEntity(userId, username, activityTypeId, product, type);
            _dncContext.TB_T_DNC_ACTIVITY_PRODUCT.Add(entity);

            int saveCount = _dncContext.SaveChanges();
            int id = entity.DNC_ACTIVITY_PRODUCT_ID;
            if (saveCount == 0 || id == 0)
                throw new DataException($"Insert {nameof(TB_T_DNC_PHONE_NO)} failed.");

            return entity;
        }

        private TB_T_DNC_EMAIL InsertNewEmail(DoNotCallEmailModel model, int userId, int transactionId, string username)
        {
            string email = model.Email;
            TB_T_DNC_EMAIL entity = MapNewEmailEntity(model, userId, transactionId, username);
            _dncContext.TB_T_DNC_EMAIL.Add(entity);

            int saveCount = _dncContext.SaveChanges();
            int id = entity.DNC_EMAIL_ID;
            if (saveCount == 0 || id == 0)
                throw new DataException($"Insert {nameof(TB_T_DNC_EMAIL)} failed.");

            return entity;
        }

        private TB_T_DNC_PHONE_NO InsertNewPhoneNo(DoNotCallTelephoneModel phoneModel, int userId, int transactionId, string username)
        {
            TB_T_DNC_PHONE_NO entity = MapNewPhoneEntity(phoneModel, userId, transactionId, username);
            _dncContext.TB_T_DNC_PHONE_NO.Add(entity);

            int saveCount = _dncContext.SaveChanges();
            int id = entity.DNC_PHONE_NO_ID;
            if (saveCount == 0 || id == 0)
                throw new DataException($"Insert {nameof(TB_T_DNC_PHONE_NO)} failed.");

            return entity;
        }

        private TB_T_DNC_ACTIVITY_TYPE InsertNewActivityType(DoNotCallTransactionEntity model, DateTime now, int userId, int transactionId)
        {
            TB_T_DNC_ACTIVITY_TYPE entity = MapNewDoNotCallActivityType(model, transactionId, now);
            _dncContext.TB_T_DNC_ACTIVITY_TYPE.Add(entity);
            int saveCount = _dncContext.SaveChanges();
            int id = entity.DNC_ACTIVITY_TYPE_ID;
            if (saveCount == 0 || id == 0)
                throw new DataException($"Insert {nameof(TB_T_DNC_ACTIVITY_TYPE)} failed.");
            return entity;
        }

        private int InsertNewtransaction(TB_T_DNC_TRANSACTION transaction)
        {
            _dncContext.TB_T_DNC_TRANSACTION.Add(transaction);
            int savetransactionCount = _dncContext.SaveChanges();
            int transactionId = transaction.DNC_TRANSACTION_ID;
            if (savetransactionCount == 0 || transactionId == 0)
                throw new DataException("Insert Do Not Call transaction failed.");
            return transactionId;
        }

        private void DeleteTransactionEmail(IEnumerable<DoNotCallEmailModel> deletedEmails, int userId, string username, int transactionId)
        {
            List<int> deleteIds = deletedEmails.Select(x => x.Id).ToList();
            List<string> emails = deletedEmails.Select(x => x.Email).ToList();
            var targetUpdateItems = _dncContext.TB_T_DNC_EMAIL.Where(x => deleteIds.Contains(x.DNC_EMAIL_ID) || (emails.Contains(x.EMAIL) && x.DNC_TRANSACTION_ID == transactionId));

            foreach (var email in targetUpdateItems)
            {
                DateTime updateDate = deletedEmails.First(x => x.Id == email.DNC_EMAIL_ID || x.Email.Equals(email.EMAIL, StringComparison.InvariantCultureIgnoreCase)).LastUpdateDate;
                email.UPDATE_DATE = updateDate;
                email.UPDATE_USER = userId;
                email.UPDATE_USERNAME = username;
                email.DELETE_STATUS = Constants.DeleteFlagTrue;
            }
        }

        private void DeleteTransactionTelephones(IEnumerable<DoNotCallTelephoneModel> deletedPhones, int userId, string username, int transactionId)
        {
            // Delete phone no 
            List<int> deleteIds = deletedPhones.Select(x => x.Id).ToList();
            List<string> phoneNos = deletedPhones.Select(x => x.PhoneNo).ToList();
            var targetUpdatePhones = _dncContext.TB_T_DNC_PHONE_NO.Where(x => deleteIds.Contains(x.DNC_PHONE_NO_ID) || (phoneNos.Contains(x.PHONE_NO) && x.DNC_TRANSACTION_ID == transactionId));

            foreach (var phone in targetUpdatePhones)
            {
                DateTime updateDate = deletedPhones.First(x => x.Id == phone.DNC_PHONE_NO_ID || x.PhoneNo.Equals(phone.PHONE_NO, StringComparison.InvariantCultureIgnoreCase)).LastUpdateDate;
                phone.UPDATE_DATE = updateDate;
                phone.UPDATE_USERNAME = username;
                phone.UPDATE_USER = userId;
                phone.DELETE_STATUS = Constants.DeleteFlagTrue;
            }
        }

        private void DeleteTransactionProducts(List<DoNotCallProductModel> deletedItems, int userId, string username, bool isBlockAllSalesProduct, bool isBlockAllInfoProducts)
        {
            List<int> deleteIds = deletedItems.Select(x => x.ActivityProductId).ToList();

            var existingProducts = _dncContext.TB_T_DNC_ACTIVITY_PRODUCT.Where(x => deleteIds.Contains(x.DNC_ACTIVITY_PRODUCT_ID)
                                                                              || (isBlockAllInfoProducts && x.TYPE == Constants.ActivityProductTypeInformation)
                                                                              || (isBlockAllSalesProduct && x.TYPE == Constants.ActivityProductTypeSales)
                                                                              && x.DELETE_STATUS == Constants.DeleteFlagFalse);
            foreach (var prod in existingProducts)
            {
                prod.UPDATE_DATE = DateTime.Now;
                prod.UPDATE_USER = userId;
                prod.UPDATE_USERNAME = username;
                prod.DELETE_STATUS = Constants.DeleteFlagTrue;
            }
        }

        private List<TB_R_USER> GetFilteredUsers(DoNotCallListSearchFilter searchFilter)
        {
            IQueryable<TB_R_USER> users = _csmContext.TB_R_USER.AsNoTracking();
            if (searchFilter.UpdateBranchId.HasValue)
            {
                users = users.Where(u => u.BRANCH_ID == searchFilter.UpdateBranchId.Value);
            }
            if (searchFilter.UpdateUser.HasValue)
            {
                users = users.Where(u => u.USER_ID == searchFilter.UpdateUser.Value);
            }
            return users.ToList();
        }

        private IQueryable<TB_T_DNC_TRANSACTION> GetFilteredtransactions(DoNotCallListSearchFilter searchFilter)
        {
            IQueryable<TB_T_DNC_TRANSACTION> transactions = _dncContext.TB_T_DNC_TRANSACTION.AsNoTracking();
            string cardNo = searchFilter.CardNo.GetCleanString(toLower: false);
            if (cardNo != string.Empty)
            {
                transactions = transactions.Where(c => c.CARD_NO.Contains(cardNo));
            }
            string transactionType = searchFilter.TransactionType.GetCleanString();
            if (transactionType != string.Empty)
            {
                transactions = transactions.Where(t => transactionType == Constants.SelectListItemValue.All || t.TRANS_TYPE == transactionType);
            }
            if (searchFilter.DoNotCallListStatusId.HasValue)
            {
                string digitStatus = searchFilter.DoNotCallListStatusId.Value.ToString();
                transactions = transactions.Where(c => digitStatus == Constants.SelectListItemValue.All || c.STATUS == digitStatus);
            }
            if (searchFilter.SubscriptionTypeId.HasValue)
            {
                int subType = searchFilter.SubscriptionTypeId.Value;
                int all = int.Parse(Constants.SelectListItemValue.All);
                transactions = transactions.Where(x => subType == all || x.SUBSCRIPT_TYPE_ID == subType);
            }
            string firstName = searchFilter.FirstName.GetCleanString();
            if (firstName != string.Empty)
            {
                transactions = transactions.Where(c => c.FIRST_NAME.ToLower().Contains(firstName));
            }
            string lastName = searchFilter.LastName.GetCleanString();
            if (lastName != string.Empty)
            {
                transactions = transactions.Where(c => c.LAST_NAME.ToLower().Contains(lastName));
            }
            if (searchFilter.FromDate.HasValue)
            {
                DateTime fromDate = searchFilter.FromDate.Value.Date; // at 00:00 am
                transactions = transactions.Where(c => c.UPDATE_DATE >= fromDate);
            }
            if (searchFilter.ToDate.HasValue)
            {
                DateTime toDate = searchFilter.ToDate.Value.AddDays(1).Date; // before 00:00 next date
                transactions = transactions.Where(c => c.UPDATE_DATE < toDate);
            }
            var email = searchFilter.Email.GetCleanString();
            if (email != string.Empty)
            {
                transactions = transactions.Where(x => x.TB_T_DNC_EMAIL.Any(e => e.EMAIL.Contains(email)));
            }
            var phoneNo = searchFilter.Telephone.GetCleanString();
            if (phoneNo != string.Empty)
            {
                transactions = transactions.Where(x => x.TB_T_DNC_PHONE_NO.Any(e => e.PHONE_NO.Contains(phoneNo)));
            }

            return transactions;
        }

        private TB_T_DNC_ACTIVITY_PRODUCT MapNewActivityProductEntity(int userId, string username, int activityTypeId, DoNotCallProductModel product, string type)
        {
            DateTime updateDate = product.UpdateDate;
            string deleteStatus = product.IsDeleted ? Constants.DeleteFlagTrue : Constants.DeleteFlagFalse;
            return new TB_T_DNC_ACTIVITY_PRODUCT
            {
                CREATE_DATE = updateDate,
                CREATE_USER = userId,
                CREATE_USERNAME = username,
                UPDATE_DATE = updateDate,
                UPDATE_USER = userId,
                UPDATE_USERNAME = username,
                DNC_ACTIVITY_TYPE_ID = activityTypeId,
                PRODUCT_ID = product.Id,
                DELETE_STATUS = deleteStatus,
                TYPE = type
            };
        }

        private TB_T_DNC_EMAIL MapNewEmailEntity(DoNotCallEmailModel model, int userId, int transactionId, string username)
        {
            DateTime createDate = model.LastUpdateDate;
            string deleteStatus = model.IsDeleted ? Constants.DeleteFlagTrue : Constants.DeleteFlagFalse;
            return new TB_T_DNC_EMAIL
            {
                CREATE_DATE = createDate,
                CREATE_USERNAME = username,
                UPDATE_DATE = createDate,
                CREATE_USER = userId,
                UPDATE_USERNAME = username,
                UPDATE_USER = userId,
                DNC_TRANSACTION_ID = transactionId,
                EMAIL = model.Email,
                DELETE_STATUS = deleteStatus
            };
        }

        private TB_T_DNC_PHONE_NO MapNewPhoneEntity(DoNotCallTelephoneModel phone, int userId, int transactionId, string username)
        {
            DateTime createDate = phone.LastUpdateDate;
            string deleteStatus = phone.IsDeleted ? Constants.DeleteFlagTrue : Constants.DeleteFlagFalse;
            return new TB_T_DNC_PHONE_NO
            {
                CREATE_DATE = createDate,
                PHONE_NO = phone.PhoneNo,
                UPDATE_DATE = createDate,
                CREATE_USER = userId,
                CREATE_USERNAME = username,
                UPDATE_USER = userId,
                UPDATE_USERNAME = username,
                DNC_TRANSACTION_ID = transactionId,
                DELETE_STATUS = deleteStatus
            };
        }

        private TB_T_DNC_ACTIVITY_TYPE MapNewDoNotCallActivityType(DoNotCallTransactionEntity model, int transactionId, DateTime updateDate)
        {
            var blockInfo = model.BlockInformation;
            var blockSales = model.BlockSales;
            return new TB_T_DNC_ACTIVITY_TYPE
            {
                DNC_TRANSACTION_ID = transactionId,
                INFORMATION_BLOCK_ALL_PRODUCT = BoolToDigit(blockInfo.IsBlockAllInfoProducts),
                INFORMATION_BLOCK_EMAIL = BoolToDigit(blockInfo.IsBlockInfoEmail),
                INFORMATION_BLOCK_SMS = BoolToDigit(blockInfo.IsBlockInfoSms),
                INFORMATION_BLOCK_TELEPHONE = BoolToDigit(blockInfo.IsBlockInfoTelephone),
                SALES_BLOCK_ALL_PRODUCT = BoolToDigit(blockSales.IsBlockAllSalesProducts),
                SALES_BLOCK_EMAIL = BoolToDigit(blockSales.IsBlockSalesEmail),
                SALES_BLOCK_SMS = BoolToDigit(blockSales.IsBlockSalesSms),
                SALES_BLOCK_TELEPHONE = BoolToDigit(blockSales.IsBlockSalesTelephone),
                CREATE_DATE = updateDate,
                CREATE_USERNAME = model.CurrentUsername,
                CREATE_USER = model.CurrentUserId,
                UPDATE_DATE = updateDate,
                UPDATE_USER = model.CurrentUserId,
                UPDATE_USERNAME = model.CurrentUsername
            };
        }

        private TB_T_DNC_TRANSACTION MapNewDoNotCallTypeTelephone(DoNotCallByTelephoneEntity model)
        {
            var _commonDataAccess = new CommonDataAccess(_csmContext);
            var card = model.CardInfo;
            int userId = model.CurrentUserId;
            var basicInfo = model.BasicInfo;

            TB_T_DNC_TRANSACTION transaction = new TB_T_DNC_TRANSACTION
            {
                CARD_NO = card.CardNo,
                CREATE_DATE = basicInfo.CreateDate,
                CREATE_USER = userId,
                CREATE_USERNAME = model.CurrentUsername,
                EFFECTIVE_DATE = basicInfo.EffectiveDate,
                EXPIRY_DATE = basicInfo.ExpireDate,
                FIRST_NAME = basicInfo.FirstName,
                LAST_NAME = basicInfo.LastName,
                FROM_SYSTEM = basicInfo.FromSystem,
                REMARK = basicInfo.Remark,
                STATUS = BoolToDigit(basicInfo.ExpireDate.Date >= DateTime.Today.Date),
                UPDATE_USER = userId,
                UPDATE_USERNAME = model.CurrentUsername,
                UPDATE_DATE = basicInfo.UpdateDate,
                SUBSCRIPT_TYPE_ID = card.SubscriptionTypeId,
                TRANS_TYPE = Constants.DNC.TransactionTypeTelephone
            };

            return transaction;
        }

        private TB_T_DNC_TRANSACTION MapNewToDoNotCallTypeCustomer(DoNotCallByCustomerEntity model)
        {
            var _commonDataAccess = new CommonDataAccess(_csmContext);
            var basicInfo = model.BasicInfo;
            var card = model.CardInfo;
            TB_T_DNC_TRANSACTION transaction = new TB_T_DNC_TRANSACTION
            {
                CARD_NO = card.CardNo,
                CREATE_DATE = basicInfo.CreateDate,
                CREATE_USER = model.CurrentUserId,
                CREATE_USERNAME = model.CurrentUsername,
                EFFECTIVE_DATE = basicInfo.EffectiveDate,
                EXPIRY_DATE = basicInfo.ExpireDate,
                FIRST_NAME = basicInfo.FirstName,
                LAST_NAME = basicInfo.LastName,
                FROM_SYSTEM = basicInfo.FromSystem,
                REMARK = basicInfo.Remark,
                STATUS = BoolToDigit(basicInfo.ExpireDate.Date >= DateTime.Today.Date),
                UPDATE_USER = model.CurrentUserId,
                UPDATE_USERNAME = model.CurrentUsername,
                UPDATE_DATE = basicInfo.UpdateDate,
                SUBSCRIPT_TYPE_ID = card.SubscriptionTypeId,
                TRANS_TYPE = Constants.DNC.TransactionTypeCustomer
            };

            return transaction;
        }

        private string BoolToDigit(bool isTrue)
        {
            return isTrue ? Constants.DigitTrue : Constants.DigitFalse;
        }
        #endregion Private
    }
}
