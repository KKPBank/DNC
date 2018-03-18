using CSM.Business;
using CSM.Business.Interfaces;
using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Entity.Common;
using CSM.Web.Controllers.Common;
using CSM.Web.Filters;
using CSM.Web.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CSM.Web.Controllers
{
    [CheckUserSession]
    [CheckUserRole(ScreenCode.SearchDoNotCall)]
    public class DoNotCallController : BaseController
    {
        private LogMessageBuilder _logMsg = new LogMessageBuilder();
        private IDoNotCallFacade _doNotCallFacade;
        private ICommonFacade _commonFacade;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CustomerController));

        #region ImportData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchUploadList()
        {
            // Setup log
            string logPrefix = "Search Upload List";
            string logMsg = _logMsg.Clear().SetPrefixMsg(logPrefix).ToInputLogString();
            Logger.Info(logMsg);

            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                _commonFacade = new CommonFacade();
                ViewBag.FileStatusList = new List<SelectListItem>
                {
                    new SelectListItem { Text="Success", Value="1" },
                    new SelectListItem { Text="Failed", Value="0" }
                };
                var model = new DoNotCallLoadListSearchFilter();
                model.PageSize = _commonFacade.GetPageSizeStart();
                model.SortField = "Id";
                return View(model);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitUploadFile(DoNotCallFileUploadInputModel model)
        {
            string logPrefix = "Submit Upload File";
            string logMsg = _logMsg.Clear().SetPrefixMsg(logPrefix).Add("FileName", model.File.FileName).ToInputLogString();
            Logger.Info(logMsg);
            bool success = false;

            try
            {
                var file = model.File;
                ValidateUploadFile(file);

                if (ModelState.IsValid)
                {
                    _doNotCallFacade = new DoNotCallFacade();
                    _commonFacade = new CommonFacade();
                    var now = DateTime.Now;
                    string dateTimeSuffix = now.FormatDateTime("yyyyMMddHHmmss");
                    string newFileName = $"DNC_{dateTimeSuffix}.xlsx";
                    //string folderPath = @"C:\DoNotCallPath\Upload"; 
                    string folderPath = _commonFacade.GetDoNotCallUploadPath();
                    string dateSuffix = now.FormatDateTime("yyyyMM");
                    string uploadFolder = $"{folderPath}/{dateSuffix}";

                    var fullFilePath = FileHelpers.GenerateFilePath(uploadFolder, newFileName);
                    // Insert File to temp location 
                    file.SaveAs(fullFilePath);

                    int userId = UserInfo.UserId;
                    string username = UserInfo.Username;
                    string ip = ApplicationHelpers.GetClientIP();
                    int totalRecord = _commonFacade.GetDoNotCallUploadTotalRecord();
                    DoNotCallFileUploadResultModel result = _doNotCallFacade.SaveUploadFile(fullFilePath, userId, ip, username, file.FileName, file.ContentType, totalRecord);
                    success = result.Success;
                    result.FileName = file.FileName;

                    return PartialView("~/Views/DoNotCall/_listFileUploadResult.cshtml", result);
                }
                else
                {
                    return ReturnJsonModelErrorResult();
                }
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.UploadFile, success);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadList()
        {
            // Setup log
            string logPrefix = "Upload List";
            string logMsg = _logMsg.Clear().SetPrefixMsg(logPrefix).ToInputLogString();
            Logger.Info(logMsg);

            try
            {
                _commonFacade = new CommonFacade();

                ParameterEntity paramSingleFileSize = _commonFacade.GetCacheParamByName(Constants.ParameterName.SingleFileSize);
                int limitFileSize;
                int.TryParse(paramSingleFileSize.ParamValue, out limitFileSize);
                var model = new DoNotCallFileUploadInputModel
                {
                    AllowedFileType = Constants.DNC.AllowedFileType,
                    LimitFileSize = limitFileSize
                };
                return View(model);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewUploadList(int id)
        {
            string logPrefix = "View File Upload List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix).Add("LoadListId", id).ToInputLogString();
            Logger.Info(log);

            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                _commonFacade = new CommonFacade();
                var resultModel = new DoNotCallFileUploadDetailModel();

                resultModel.Pager.PageSize = _commonFacade.GetPageSizeStart();

                DoNotCallFileUploadDetailModel data = _doNotCallFacade.GetViewUploadFileDetail(id, resultModel.Pager);

                if (data != null)
                {
                    resultModel = data;
                }

                ViewBag.PageSizeList = _commonFacade.GetPageSizeList();
                return View(resultModel);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetSortedFileDataList(DoNotCallFileUploadSortFilter searchFilter)
        {
            string logPrefix = "View File Upload List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                                     .Add("ID", searchFilter.Id)
                                     .ToInputLogString();
            Logger.Info(log);
            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                _commonFacade = new CommonFacade();

                var resultModel = new DoNotCallFileUploadDetailModel();
                DoNotCallFileUploadDetailModel data = _doNotCallFacade.GetViewUploadFileDetail(searchFilter.Id, searchFilter);

                if (data != null)
                {
                    resultModel = data;
                }

                ViewBag.PageSizeList = _commonFacade.GetPageSizeList();
                return PartialView("~/Views/DoNotCall/_ListFileUploadData.cshtml", resultModel);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchFileUploadList(DoNotCallLoadListSearchFilter model)
        {
            string logPrefix = "Search File Upload List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                                     .Add("FileName", model.FileName)
                                     .Add("FileStatusId", model.FileStatusId)
                                     .Add("FromDate", model.FromDate?.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate))
                                     .Add("ToDate", model.ToDate?.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate))
                                     .ToInputLogString();
            Logger.Info(log);
            bool success = false;

            try
            {
                if (ModelState.IsValid)
                {
                    _doNotCallFacade = new DoNotCallFacade();
                    _commonFacade = new CommonFacade();

                    var result = new DoNotCallUploadSearchResultViewModel();

                    List<DoNotCallFileUploadSearchResultModel> resultList = _doNotCallFacade.SearchDoNotCallUploadList(model);

                    if (resultList != null)
                    {
                        result.Results = resultList;
                        result.Pager = model;
                    }

                    ViewBag.PageSizeList = _commonFacade.GetPageSizeList();
                    success = true;
                    return PartialView("~/Views/DoNotCall/_ListFileUpload.cshtml", result);
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.SearchFileUpload, success);
            }
        }

        #endregion

        #region Transaction

        public ActionResult Search()
        {
            // Setup log
            string logPrefix = "InitSearch DoNotCall";
            string logMsg = _logMsg.Clear().SetPrefixMsg(logPrefix).ToInputLogString();
            Logger.Info(logMsg);

            try
            {
                _commonFacade = new CommonFacade();
                _doNotCallFacade = new DoNotCallFacade();

                ViewBag.DoNotCallListStatus = new List<SelectListItem>
                {
                    new SelectListItem { Text=Resource.Ddl_Status_All, Value=Constants.SelectListItemValue.All },
                    new SelectListItem { Text="Active", Value=Constants.DigitTrue },
                    new SelectListItem { Text="Inactive", Value=Constants.DigitFalse }
                };
                ViewBag.TransactionTypeList = new List<SelectListItem>
                {
                    new SelectListItem { Text=Resource.Ddl_Status_All, Value=Constants.SelectListItemValue.All },
                    new SelectListItem { Text="Customer", Value=Constants.DNC.TransactionTypeCustomer },
                    new SelectListItem { Text="Telephone/Email", Value=Constants.DNC.TransactionTypeTelephone }
                };

                var subsList = GetSubscriptionTypeList().ToList();
                subsList.Insert(0, new SelectListItem { Text=Resource.Ddl_Status_All, Value=Constants.SelectListItemValue.All });

                ViewBag.SubscriptionTypeList = subsList;

                var model = new DoNotCallViewModel();
                model.SearchFilter.PageSize = _commonFacade.GetPageSizeStart();
                model.SearchFilter.SortField = "TransactionDate";
                return View(model);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult GetHistoryDoNotCallList(DoNotCallHistorySearchFilter model)
        {
            string logPrefix = "Get History Do Not Call List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("CustomerId", model.CustomerId)
                             .ToInputLogString();
            Logger.Info(log);

            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                _commonFacade = new CommonFacade();
                DoNotCallHistoryLogViewModel resultModel = new DoNotCallHistoryLogViewModel();

                var pager = model.Pager;
                ViewBag.PageSizeList = _commonFacade.GetPageSizeList();

                if (model.PageNo > 0) pager.PageNo = model.PageNo;
                if (model.PageSize > 0) pager.PageSize = model.PageSize;

                ParameterEntity historyLimit = _commonFacade.GetCacheParamByName(Constants.ParameterName.DoNotCallHistoryLimit);
                int totalLimit = historyLimit.ParamValue.ToNullable<int>() ?? 10;

                List<DoNotCallHistoryEntity> resultList = _doNotCallFacade.GetHistoryDoNotCallListByCustomerId(model.CustomerId, pager, totalLimit);

                if (resultList.Count > 0)
                {
                    resultModel.LogList = resultList;
                    resultModel.Pager = pager;
                }

                return PartialView("~/Views/DoNotCall/Shared/_HistoryDoNotCallList.cshtml", resultModel);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchDoNotCall(DoNotCallListSearchFilter searchFilter)
        {
            string logPrefix = "Search DoNotCall List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("CardNo", searchFilter.CardNo)
                             .Add("CisId", searchFilter.CisId)
                             .Add("UpdateByBranchId", searchFilter.UpdateBranchId)
                             .Add("UpdateByUserId", searchFilter.UpdateUser)
                             .Add("StatusId", searchFilter.DoNotCallListStatusId)
                             .Add("Email", searchFilter.Email)
                             .Add("FirstName", searchFilter.FirstName)
                             .Add("LastName", searchFilter.LastName)
                             .Add("FromDate", searchFilter.FromDate?.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate))
                             .Add("ToDate", searchFilter.ToDate?.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate))
                             .Add("SubscriptionTypeId", searchFilter.SubscriptionTypeId)
                             .Add("Telephone", searchFilter.Telephone)
                             .Add("TransactionType", searchFilter.TransactionType)
                             .Add("SortField", searchFilter.SortField)
                             .Add("SortOrder", searchFilter.SortOrder)
                             .ToInputLogString();
            Logger.Info(log);
            bool success = false;

            try
            {
                if (ModelState.IsValid)
                {
                    _doNotCallFacade = new DoNotCallFacade();
                    _commonFacade = new CommonFacade();

                    var resultModel = new DoNotCallViewModel();
                    List<DoNotCallEntity> doNotCallList = _doNotCallFacade.SearchDoNotCallList(searchFilter);
                    if (doNotCallList.Count > 0)
                    {
                        foreach (var entity in doNotCallList)
                        {
                            DoNotCallSearchResultViewModel item = MapEntityToViewModel(entity);
                            resultModel.DoNotCallList.Add(item);
                        }
                    }

                    int listCount = resultModel.DoNotCallList.Count;

                    resultModel.SearchFilter = searchFilter;
                    ViewBag.PageSizeList = _commonFacade.GetPageSizeList();

                    Logger.Info(_logMsg.Clear().SetPrefixMsg(logPrefix).Add("SearchResultCount", listCount).ToSuccessLogString());
                    success = true;

                    return PartialView("~/Views/DoNotCall/_ListTransaction.cshtml", resultModel);
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.Search, success);
            }
        }

        #endregion

        #region Customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCustomer()
        {
            _doNotCallFacade = new DoNotCallFacade();
            ViewBag.SubscriptionTypeList = GetSubscriptionTypeList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewCustomer(DoNotCallCardInfoModel inputModel)
        {
            DoNotCallByCustomerEntity model = GenerateNewCustomerEntity(inputModel);

            _doNotCallFacade = new DoNotCallFacade();
            ViewBag.SubscriptionTypeList = GetSubscriptionTypeList();

            _commonFacade = new CommonFacade();
            string neverExpireDateStr = _commonFacade.GetCacheParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime neverExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDateStr, "yyyy-MM-dd").Value;
            ViewBag.NeverExpireYear = neverExpireDate.Year;
            ViewBag.NeverExpireMonth = neverExpireDate.Month;
            ViewBag.NeverExpireDay = neverExpireDate.Day;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomer(int id)
        {
            _doNotCallFacade = new DoNotCallFacade();
            DoNotCallByCustomerEntity model = _doNotCallFacade.GetDoNotCallCustomerModelById(id);

            ViewBag.SubscriptionTypeList = GetSubscriptionTypeList();
            _commonFacade = new CommonFacade();
            string neverExpireDateStr = _commonFacade.GetCacheParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime neverExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDateStr, "yyyy-MM-dd").Value;
            ViewBag.NeverExpireYear = neverExpireDate.Year;
            ViewBag.NeverExpireMonth = neverExpireDate.Month;
            ViewBag.NeverExpireDay = neverExpireDate.Day;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchNewCustomerTransaction(DoNotCallByCustomerSearchInputModel model)
        {
            string logPrefix = "Search New Customer Transaction";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("SubscriptionTypeId", model.SubscriptionTypeId)
                             .Add("CardNo", model.CardNo)
                             .ToInputLogString();
            Logger.Info(log);
            bool success = false;

            try
            {
                var result = new DoNotCallByCustomerSearchResultViewModel();
                _doNotCallFacade = new DoNotCallFacade();
                _commonFacade = new CommonFacade();
                Pager pager = SetupSearchPaging(model.PageSize, model.PageNo);
                result.Transactions = _doNotCallFacade.SearchDoNotCallCustomerTransactionList(model.CardNo, model.SubscriptionTypeId, pager);

                int itemCount = result.Transactions.Count;
                if (itemCount > 0)
                {
                    result.Pager = pager;
                }

                ViewBag.PageSizeList = _commonFacade.GetPageSizeList(pager.PageSize);
                success = true;
                return PartialView("~/Views/DoNotCall/_ListCustomerTransaction.cshtml", result);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.SearchNewCustomer, success);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ValidateNewCustomerCard(DoNotCallCardInfoModel model)
        {
            string logPrefix = "Validate New Customer Card";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("CardNo", model.CardNo)
                             .Add("SubscriptionTypeId", model.SubscriptionTypeId)
                             .ToInputLogString();
            Logger.Info(log);

            try
            {
                ValidateRequireCustomerCardInfo(model);

                if (ModelState.IsValid)
                {
                    ValidateCustomerPersonalCardTypeFormat(model);
                }

                if (ModelState.IsValid)
                {

                    return Json(new
                    {
                        Error = string.Empty,
                        Errors = string.Empty
                    });
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewTransactionHistoryDetail(int logId, int versionNo)
        {
            string logPrefix = "Get Do Not Call History Detail";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("LogId", logId)
                             .Add("VersionNo", versionNo)
                             .ToInputLogString();
            Logger.Info(log);

            try
            {
                _doNotCallFacade = new DoNotCallFacade();
                DoNotCallTransactionHistoryEntity resultModel = _doNotCallFacade.GetDoNotCallHistoryDetail(logId);
                resultModel.VersionNo = versionNo;

                return View("ViewTransactionDetail", resultModel);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost, ValidateJsonAntiForgeryToken]
        public ActionResult SaveCustomer(DoNotCallByCustomerEntity model)
        {
            string logPrefix = "Save Customer";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .AddObject(model)
                             .ToInputLogString();
            Logger.Info(log);
            bool success = false;
            string detail = string.Empty;
            string actionName = model.IsNewCustomer ? Constants.AuditAction.CreateCustomerTransaction
                                                     : Constants.AuditAction.EditCustomerTransaction;
            try
            {
                ValidateRequireEmail(model);
                ValidateRequireTelephone(model);
                var cardInfo = model.CardInfo;
                ValidateRequireCustomerCardInfo(cardInfo);
                if (ModelState.IsValid)
                {
                    ValidateCustomerPersonalCardTypeFormat(cardInfo);
                }
                if (ModelState.IsValid)
                {
                    ValidateDuplicatedTransactionCardInfo(cardInfo, model.BasicInfo.TransactionId);
                }

                if (ModelState.IsValid)
                {
                    _doNotCallFacade = new DoNotCallFacade();
                    UserEntity currentUser = UserInfo;
                    int userId = currentUser.UserId;
                    model.CurrentUserId = userId;
                    model.CurrentUsername = currentUser.Username;
                    model.CurrentUserIpAddress = ApplicationHelpers.GetClientIP();

                    int customerId = _doNotCallFacade.SaveCustomer(model);

                    if (customerId != 0)
                    {
                        success = true;
                        detail = $"Transaction ID: {customerId}";
                        return Json(customerId);
                    }
                }

                var errors = GetModelValidationErrors(removePrefix: true); // for BlockSales/Information
                var errorResult = new
                {
                    Valid = false,
                    Error = string.Empty,
                    Errors = errors
                };

                return Json(errorResult);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(actionName, success, detail);
            }
        }

        #endregion Customer

        #region Telephone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePhoneNo()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewPhoneNo(DoNotCallNewTelephoneModel inputModel)
        {
            DoNotCallByTelephoneEntity model = GenerateNewPhoneEntity(inputModel);

            _doNotCallFacade = new DoNotCallFacade();
            _commonFacade = new CommonFacade();
            string neverExpireDateStr = _commonFacade.GetCacheParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime neverExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDateStr, "yyyy-MM-dd").Value;
            ViewBag.NeverExpireYear = neverExpireDate.Year;
            ViewBag.NeverExpireMonth = neverExpireDate.Month;
            ViewBag.NeverExpireDay = neverExpireDate.Day;
            ViewBag.SubscriptionTypeList = GetSubscriptionTypeList();
            ViewBag.PhoneNo = inputModel.PhoneNo;
            ViewBag.Email = inputModel.Email;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPhoneNo(int id)
        {
            _doNotCallFacade = new DoNotCallFacade();
            DoNotCallByTelephoneEntity model = _doNotCallFacade.GetDoNotCallByTelephoneEntity(id);
            ViewBag.SubscriptionTypeList = GetSubscriptionTypeList();
            string phoneNo = string.Empty;
            string email = string.Empty;
            var contact = model.ContactDetail;

            var telephoneList = contact.Telephone.TelephoneList;
            if (telephoneList.Count > 0)
            {
                phoneNo = telephoneList.OrderBy(x => x.IsDeleted).ThenByDescending(x => x.Id).First().PhoneNo;
            }
            var emailList = contact.Email.EmailList;
            if (emailList.Count > 0)
            {
                email = emailList.OrderBy(x => x.IsDeleted).ThenByDescending(x => x.Id).First().Email;
            }

            _commonFacade = new CommonFacade();
            string neverExpireDateStr = _commonFacade.GetCacheParamByName("DNC_NEVER_EXPIRE_DATE").ParamValue;
            DateTime neverExpireDate = DateTimeHelpers.ParseDateTime(neverExpireDateStr, "yyyy-MM-dd").Value;
            ViewBag.NeverExpireYear = neverExpireDate.Year;
            ViewBag.NeverExpireMonth = neverExpireDate.Month;
            ViewBag.NeverExpireDay = neverExpireDate.Day;
            ViewBag.PhoneNo = phoneNo;
            ViewBag.Email = email;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchNewPhoneNo(DoNotCallNewTelephoneModel model)
        {
            string logPrefix = "Search New Phone No.";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("Email", model.Email)
                             .Add("PhoneNo", model.PhoneNo)
                             .ToInputLogString();
            Logger.Info(log);
            bool success = false;

            try
            {
                var result = new DoNotCallByPhoneSearchResultViewModel();
                _doNotCallFacade = new DoNotCallFacade();
                _commonFacade = new CommonFacade();
                Pager pager = SetupSearchPaging(model.PageSize, model.PageNo);

                result.Transactions = _doNotCallFacade.SearchDoNotCallTelephoneContact(model.Email, model.PhoneNo, pager);

                int itemCount = result.Transactions.Count;
                if (itemCount > 0)
                {
                    result.Pager = pager;
                }

                ViewBag.PageSizeList = _commonFacade.GetPageSizeList(pager.PageSize);
                success = true;
                return PartialView("~/Views/DoNotCall/_ListTelephoneTransaction.cshtml", result);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.SearchNewCustomer, success);
            }
        }

        [HttpPost, ValidateJsonAntiForgeryToken]
        public ActionResult SavePhone(DoNotCallByTelephoneEntity model)
        {
            string logPrefix = "Save Telephone";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .AddObject(model)
                             .ToInputLogString();
            Logger.Info(log);
            bool success = false;
            string detail = string.Empty;
            string actionName = model.IsNewCustomer ? Constants.AuditAction.CreateTelephoneTransaction
                                                     : Constants.AuditAction.EditTelephoneTransaction;

            try
            {
                ValidateRequireEmail(model);
                ValidateRequireTelephone(model);
                ValidateTelephoneCardInfo(model.CardInfo, model.BasicInfo.TransactionId);

                if (ModelState.IsValid)
                {
                    _doNotCallFacade = new DoNotCallFacade();
                    UserEntity currentUser = UserInfo;
                    int userId = currentUser.UserId;
                    model.CurrentUserId = userId;
                    model.CurrentUserIpAddress = ApplicationHelpers.GetClientIP();

                    int customerId = _doNotCallFacade.SavePhone(model);

                    if (customerId != 0)
                    {
                        success = true;
                        detail = $"Transaction ID: {customerId}";
                        return Json(customerId);
                    }
                }

                var errors = GetModelValidationErrors(removePrefix: true); // for BlockSales/Information
                var errorResult = new
                {
                    Valid = false,
                    Error = string.Empty,
                    Errors = errors
                };

                return Json(errorResult);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(actionName, success, detail);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ValidateTelephoneBaseContact(DoNotCallNewTelephoneModel model)
        {
            string logPrefix = "Validate Telephone Base Contact";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("PhoneNo", model.PhoneNo)
                             .Add("Email", model.Email)
                             .ToInputLogString();
            Logger.Info(log);

            try
            {
                if (ModelState.IsValid)
                {
                    return Json(new
                    {
                        Error = string.Empty,
                        Errors = string.Empty
                    });
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }
        #endregion Telephone

        #region Excel 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadForm()
        {
            string logPrefix = "DoNotCall Download Form";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix).ToInputLogString();
            Logger.Info(log);
            bool success = false;
            string detail = string.Empty;

            try
            {
                _commonFacade = new CommonFacade();
                byte[] byteArray = _commonFacade.GetDoNotCallFormTemplateFileBytes();
                var file = File(byteArray, "application/octet-stream", "FormTemplateDoNotCall.xlsx");
                success = true;
                return file;
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.DownloadTemplate, success, detail);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportList(DoNotCallListSearchFilter searchFilter)
        {
            string logPrefix = "Export DoNotCall List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("CardNo", searchFilter.CardNo)
                             .Add("CisId", searchFilter.CisId)
                             .Add("UpdateByBranchId", searchFilter.UpdateBranchId)
                             .Add("UpdateByUserId", searchFilter.UpdateUser)
                             .Add("StatusId", searchFilter.DoNotCallListStatusId)
                             .Add("Email", searchFilter.Email)
                             .Add("FirstName", searchFilter.FirstName)
                             .Add("LastName", searchFilter.LastName)
                             .Add("FromDate", searchFilter.FromDate?.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate))
                             .Add("ToDate", searchFilter.ToDate?.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate))
                             .Add("SubscriptionTypeId", searchFilter.SubscriptionTypeId)
                             .Add("Telephone", searchFilter.Telephone)
                             .Add("TransactionType", searchFilter.TransactionType)
                             .Add("SortField", searchFilter.SortField)
                             .Add("SortOrder", searchFilter.SortOrder)
                             .ToInputLogString();
            Logger.Info(log);
            bool success = false;
            string detail = string.Empty;

            try
            {
                if (ModelState.IsValid)
                {
                    _doNotCallFacade = new DoNotCallFacade();
                    // Export Excel
                    byte[] bytes = _doNotCallFacade.GenerateDoNotCallListExcelFile(searchFilter);
                    TempData["FILE_EXPORT_DO_NOT_CALL_LIST"] = bytes;

                    success = true;

                    return Json(new
                    {
                        Valid = true,
                        Message = string.Empty,
                        Error = string.Empty,
                    });
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
            finally
            {
                InsertAuditLog(Constants.AuditAction.ExportDoNotCall, success, detail);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrintDoNotCallListExcel()
        {
            string logPrefix = "Print DoNotCall List";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix).ToInputLogString();
            Logger.Info(log);

            try
            {
                var bytes = TempData["FILE_EXPORT_DO_NOT_CALL_LIST"] as Byte[];
                string dateStr = DateTime.Now.FormatDateTime(Constants.DateTimeFormat.ExportDateTime);
                string fileDownloadName = string.Format(CultureInfo.InvariantCulture, $"DoNotCallList_{DateTime.Now.FormatDateTime("yyyyMMddHHmmss")}.xlsx");
                return File(bytes, "application/octet-stream", fileDownloadName);
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }
        #endregion Excel

        public void ValidateRequireEmail(DoNotCallTransactionEntity model)
        {
            var sales = model.BlockSales;
            var info = model.BlockInformation;
            bool requireEmail = sales.IsBlockSalesEmail || info.IsBlockInfoEmail;
            if (requireEmail && !model.ContactDetail.HasActiveEmail)
            {
                ModelState.AddModelError("Email", string.Format(Resource.ValErrParam_PleaseInsertAtLeastOneRecord, Resource.Lbl_Email));
            }
        }

        public void ValidateRequireTelephone(DoNotCallTransactionEntity model)
        {
            var sales = model.BlockSales;
            var info = model.BlockInformation;
            bool requirePhoneNo = sales.IsBlockSalesSms || sales.IsBlockSalesTelephone || info.IsBlockInfoSms || info.IsBlockInfoTelephone;
            if (requirePhoneNo && !model.ContactDetail.HasActiveTelephone)
            {
                ModelState.AddModelError("Telephone", string.Format(Resource.ValErrParam_PleaseInsertAtLeastOneRecord, Resource.Lbl_Telephone));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ValidatePhoneNoInput(PhoneNoInputModel model)
        {
            string logPrefix = "Validate DoNotCall Phone No.";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("PhoneNo", model.PhoneNo)
                             .ToInputLogString();
            Logger.Info(log);

            try
            {
                if (ModelState.IsValid)
                {
                    return Json(new
                    {
                        model.PhoneNo,
                        Error = string.Empty,
                        Errors = string.Empty
                    });
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ValidateEmailInput(EmailInputModel model)
        {
            string logPrefix = "Validate Email";
            var log = _logMsg.Clear().SetPrefixMsg(logPrefix)
                             .Add("Email", model.Email)
                             .ToInputLogString();
            Logger.Info(log);

            try
            {
                if (ModelState.IsValid)
                {
                    return Json(new
                    {
                        model.Email,
                        Error = string.Empty,
                        Errors = string.Empty
                    });
                }

                return ReturnJsonModelErrorResult();
            }
            catch (Exception ex)
            {
                return HandleControllerException(ex, logPrefix);
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
                CreateUserId = UserInfo.UserId
            };
            AppLog.AuditLog(auditLog);
        }

        private void ValidateUploadFile(HttpPostedFileBase file)
        {
            _commonFacade = new CommonFacade();
            ParameterEntity paramSingleFileSize = _commonFacade.GetCacheParamByName(Constants.ParameterName.SingleFileSize);
            int? limitSingleFileSize = paramSingleFileSize.ParamValue.ToNullable<int>();

            if (file.ContentLength > limitSingleFileSize.Value)
            {
                ModelState.AddModelError("File", string.Format(CultureInfo.InvariantCulture, Resource.ValError_SingleFileSizeExceedMaxLimit, (limitSingleFileSize.Value / Constants.KbPerMB)));
            }
            if (file.ContentType != Constants.DNC.AllowedFileType)
            {
                ModelState.AddModelError("File", string.Format(CultureInfo.InvariantCulture, Resource.ValError_FileExtension));
            }
        }

        private void ValidateRequireCustomerCardInfo(DoNotCallCardInfoModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CardNo))
            {
                var cardNoError = string.Format(Resource.ValErr_RequiredField, "Card No");
                ModelState.AddModelError("CardNo", cardNoError);
            }
            if (!model.SubscriptionTypeId.HasValue)
            {
                var subscriptionTypeError = string.Format(Resource.ValErr_RequiredField, "Subscription Type");
                ModelState.AddModelError("SubscriptionTypeId", subscriptionTypeError);
            }
        }

        private void ValidateTelephoneCardInfo(DoNotCallCardInfoModel model, int transactionId)
        {
            bool isCompleteCardInfo = !string.IsNullOrWhiteSpace(model.CardNo) && model.SubscriptionTypeId.HasValue;
            bool isEmptyCardInfo = string.IsNullOrWhiteSpace(model.CardNo) && !model.SubscriptionTypeId.HasValue;
            if (isCompleteCardInfo)
            {
                bool validCardFormat = ValidateTelephonePersonalCardTypeFormat(model);
                if (validCardFormat)
                {
                    ValidateDuplicatedTransactionCardInfo(model, transactionId);
                }
            }
            else if (!isEmptyCardInfo)
            {
                ModelState.AddModelError("CardNo", "กรุณากรอกข้อมูลบัตรให้สมบูรณ์");
                ModelState.AddModelError("SubscriptionTypeId", "กรุณากรอกข้อมูลบัตรให้สมบูรณ์");
            }
        }

        private bool ValidateDuplicatedTransactionCardInfo(DoNotCallCardInfoModel model, int transactionId = 0)
        {
            _doNotCallFacade = new DoNotCallFacade();

            int subTypeId = model.SubscriptionTypeId.HasValue ? model.SubscriptionTypeId.Value : 0;

            bool isExistingCard = _doNotCallFacade.IsExistingCustomerTransactionCardInfo(model.CardNo, subTypeId, transactionId);
            if (isExistingCard)
            {
                ModelState.AddModelError("SubscriptionTypeId", Resource.ValError_DuplicateCardNo);
            }
            return !isExistingCard;
        }

        private bool ValidateCustomerPersonalCardTypeFormat(DoNotCallCardInfoModel model)
        {
            bool valid = ValidatePersonalCard(model);

            if (!valid)
            {
                ModelState.AddModelError("SubscriptionTypeId", Resource.ValErr_InvalidSubscriptionType);
                return false;
            }

            return true;
        }

        private bool ValidateTelephonePersonalCardTypeFormat(DoNotCallCardInfoModel model)
        {
            bool valid = ValidatePersonalCard(model);

            if (!valid)
            {
                ModelState.AddModelError("CardNo", Resource.ValErr_InvalidCardID);
                return false;
            }

            return true;
        }

        private bool ValidatePersonalCard(DoNotCallCardInfoModel model)
        {
            _commonFacade = new CommonFacade();
            var subscriptTypePersonal = _commonFacade.GetSubscriptTypeByCode(Constants.SubscriptTypeCode.Personal);
            string cardNo = model.CardNo.ExtractString();
            bool isPersonCardType = model.SubscriptionTypeId == subscriptTypePersonal.SubscriptTypeId;
            bool isValidPersonCardNo = string.IsNullOrWhiteSpace(cardNo) || ApplicationHelpers.ValidateCardNo(cardNo);
            bool valid = !isPersonCardType || isValidPersonCardNo;
            return valid;
        }

        private IEnumerable<SelectListItem> GetSubscriptionTypeList()
        {
            return _doNotCallFacade.GetSubscriptTypeSelectList().Select(x => new SelectListItem
            {
                Text = x.SubscriptTypeName,
                Value = x.SubscriptTypeId.ToString()
            });
        }

        private Pager SetupSearchPaging(int pageSize, int pageNo)
        {
            return new Pager
            {
                PageNo = pageNo > 0 ? pageNo : 1,
                PageSize = pageSize > 0 ? pageSize : _commonFacade.GetPageSizeStart(),
            };
        }

        private DoNotCallSearchResultViewModel MapEntityToViewModel(DoNotCallEntity entity)
        {
            return new DoNotCallSearchResultViewModel
            {
                Id = entity.TransactionId,
                CardNo = entity.CardNo,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                CreateByName = entity.CreateBy.DisplayName,
                Email = entity.Email,
                IsBlockInformationEmail = DigitToYesNo(entity.BlockInformationEmail),
                IsBlockInformationSms = DigitToYesNo(entity.BlockInformationSms),
                IsBlockInformationTelephone = DigitToYesNo(entity.BlockInformationTelephone),
                IsBlockSalesEmail = DigitToYesNo(entity.BlockSalesEmail),
                IsBlockSalesSms = DigitToYesNo(entity.BlockSalesSms),
                IsBlockSalesTelephone = DigitToYesNo(entity.BlockSalesTelephone),
                DisplayStatus = entity.Status == Constants.DigitTrue ? Resource.Lbl_Active : Resource.Lbl_Inactive,
                Telephone = entity.Telephone,
                TransactionDate = entity.TransactionDate,
                TransactionType = entity.TransactionType,
                SubscriptionType = entity.SubscriptionType
            };
        }

        private string DigitToYesNo(string digit)
        {
            return StringHelpers.ConvertBooleanToString(digit == Constants.DigitTrue, Constants.Yes, Constants.No);
        }

        private DoNotCallByTelephoneEntity GenerateNewPhoneEntity(DoNotCallNewTelephoneModel model)
        {
            var resultModel = new DoNotCallByTelephoneEntity();
            DoNotCallUserModel userModel = GetUserModel(UserInfo);

            resultModel.BasicInfo = new DoNotCallBasicInfoModel
            {
                CreateBy = userModel,
                UpdateBy = userModel
            };

            DateTime createDate = resultModel.BasicInfo.CreateDate;

            string phoneNo = model?.PhoneNo;
            string email = model?.Email;
            bool hasPhoneNo = !string.IsNullOrWhiteSpace(phoneNo);
            bool hasEmail = !string.IsNullOrWhiteSpace(email);

            if (hasPhoneNo)
            {
                resultModel.ContactDetail.Telephone.TelephoneList.Add(new DoNotCallTelephoneModel
                {
                    PhoneNo = phoneNo,
                    LastUpdateDate = createDate,
                    UpdateBy = userModel
                });
            }
            if (hasEmail)
            {
                resultModel.ContactDetail.Email.EmailList.Add(
                    new DoNotCallEmailModel
                    {
                        Email = email,
                        LastUpdateDate = createDate,
                        UpdateBy = userModel
                    });
            }

            resultModel.BlockSales.IsBlockSalesTelephone = hasPhoneNo;
            resultModel.BlockSales.IsBlockSalesSms = hasPhoneNo;
            resultModel.BlockInformation.IsBlockInfoTelephone = hasPhoneNo;
            resultModel.BlockInformation.IsBlockInfoSms = hasPhoneNo;

            resultModel.BlockSales.IsBlockSalesEmail = hasEmail;
            resultModel.BlockInformation.IsBlockInfoEmail = hasEmail;

            return resultModel;
        }

        private static DoNotCallUserModel GetUserModel(UserEntity user)
        {
            return new DoNotCallUserModel
            {
                UserId = user.UserId,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                PositionCode = user.PositionCode
            };
        }

        private DoNotCallByCustomerEntity GenerateNewCustomerEntity(DoNotCallCardInfoModel model)
        {
            DoNotCallUserModel userModel = GetUserModel(UserInfo);
            var resultModel = new DoNotCallByCustomerEntity()
            {
                CardInfo = new DoNotCallCardInfoModel
                {
                    CardNo = model.CardNo,
                    SubscriptionTypeId = model.SubscriptionTypeId
                },
                BasicInfo = new DoNotCallBasicInfoModel
                {
                    CreateBy = userModel,
                    UpdateBy = userModel,
                }
            };
            return resultModel;
        }

        private ActionResult ReturnJsonModelErrorResult()
        {
            return Json(new
            {
                Valid = false,
                Error = string.Empty,
                Errors = GetModelValidationErrors()
            });
        }

        private ActionResult HandleControllerException(Exception ex, string logPrefix)
        {
            // Log
            Logger.Error("Exception occur:\n", ex);
            string logMessage = _logMsg.Clear().SetPrefixMsg(logPrefix)
                                       .Add("Error Message", ex.Message)
                                       .ToFailLogString();
            Logger.Info(logMessage);

            // Return exception result
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            string actionName = ControllerContext.RouteData.Values["action"].ToString();
            var errorInfo = new HandleErrorInfo(ex, controllerName, actionName);
            return Error(errorInfo);
        }
        #endregion
    }
}
