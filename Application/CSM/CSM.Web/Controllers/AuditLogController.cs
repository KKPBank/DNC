﻿using System;
using System.Globalization;
using System.Web.Mvc;
using CSM.Business;
using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Web.Controllers.Common;
using CSM.Web.CSMMailService;
using CSM.Web.CSMSRService;
using CSM.Web.Filters;
using CSM.Web.Models;
using log4net;
using System.Collections;
using System.Threading.Tasks;
using CSM.Web.CSMFileService;
using CSM.Service.Messages.SchedTask;
using System.Threading;

namespace CSM.Web.Controllers
{
    [CheckUserSession]
    public class AuditLogController : BaseController
    {
        private ICommonFacade _commonFacade;
        private IAuditLogFacade _auditlogFacade;
        private LogMessageBuilder _logMsg = new LogMessageBuilder();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AuditLogController));

        [CheckUserRole(ScreenCode.SearchAuditLog)]
        public ActionResult Search()
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("InitSearch AuditLog").ToInputLogString());
            try
            {
                _commonFacade = new CommonFacade();
                _auditlogFacade = new AuditLogFacade();
                AuditLogViewModel auditlogVM = new AuditLogViewModel();

                auditlogVM.SearchFilter = new AuditLogSearchFilter
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    DateFrom = string.Empty,
                    DateTo = string.Empty,
                    Module = string.Empty,
                    Action = string.Empty,
                    Status = null,
                    PageNo = 1,
                    PageSize = _commonFacade.GetPageSizeStart(),
                    SortField = "AuditLogId",
                    SortOrder = "DESC"
                };

                var moduleList = _auditlogFacade.GetModule(Resource.Ddl_Status_All, Constants.ApplicationStatus.All);
                ViewBag.Module = new SelectList(moduleList, "Key", "Value", string.Empty);

                var actionList = _auditlogFacade.GetAction(Resource.Ddl_Status_All, Constants.ApplicationStatus.All);
                ViewBag.Action = new SelectList(actionList, "Key", "Value", string.Empty);

                var statusList = _auditlogFacade.GetStatusSelectList(Resource.Ddl_Status_All, Constants.ApplicationStatus.All);
                ViewBag.Status = new SelectList(statusList, "Key", "Value", string.Empty);

                ViewBag.PageSize = auditlogVM.SearchFilter.PageSize;
                ViewBag.PageSizeList = _commonFacade.GetPageSizeList();
                ViewBag.Message = string.Empty;

                return View(auditlogVM);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("InitSearch AuditLog").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckUserRole(ScreenCode.SearchAuditLog)]
        public ActionResult AuditLogList(AuditLogSearchFilter searchFilter)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Search AuditLog").Add("Name", searchFilter.FirstName)
               .Add("DateFrom", searchFilter.DateFrom).Add("DateTo", searchFilter.DateTo).Add("Module", searchFilter.Module)
               .Add("Action", searchFilter.Action).Add("Status", searchFilter.Status));
            try
            {
                #region "Validation"

                bool isValid = TryUpdateModel(searchFilter);
                if (!string.IsNullOrEmpty(searchFilter.DateFrom) && !searchFilter.DateFromValue.HasValue)
                {
                    isValid = false;
                    ModelState.AddModelError("txtFromDate", Resource.ValErr_InvalidDate);
                }
                else if (searchFilter.DateFromValue.HasValue)
                {
                    if (searchFilter.DateFromValue.Value > DateTime.Now.Date)
                    {
                        isValid = false;
                        ModelState.AddModelError("txtFromDate", Resource.ValErr_InvalidDate_MustLessThanToday);
                    }
                }

                if (!string.IsNullOrEmpty(searchFilter.DateTo) && !searchFilter.DateToValue.HasValue)
                {
                    isValid = false;
                    ModelState.AddModelError("txtToDate", Resource.ValErr_InvalidDate);
                }
                else if (searchFilter.DateToValue.HasValue)
                {
                    if (searchFilter.DateToValue.Value > DateTime.Now.Date)
                    {
                        isValid = false;
                        ModelState.AddModelError("txtToDate", Resource.ValErr_InvalidDate_MustLessThanToday);
                    }
                }

                if (searchFilter.DateFromValue.HasValue && searchFilter.DateToValue.HasValue
                    && searchFilter.DateFromValue.Value > searchFilter.DateToValue.Value)
                {
                    isValid = false;
                    ModelState.AddModelError("dvDateRange", Resource.ValErr_InvalidDateRange);
                }

                #endregion

                if (isValid)
                {
                    _commonFacade = new CommonFacade();
                    _auditlogFacade = new AuditLogFacade();
                    AuditLogViewModel auditlogVM = new AuditLogViewModel();
                    auditlogVM.SearchFilter = searchFilter;
                    auditlogVM.AuditLogList = _auditlogFacade.SearchAuditLogs(searchFilter);

                    ViewBag.PageSize = auditlogVM.SearchFilter.PageSize;
                    ViewBag.PageSizeList = _commonFacade.GetPageSizeList();

                    return PartialView("~/Views/AuditLog/_AuditLogList.cshtml", auditlogVM);
                }

                return Json(new
                {
                    Valid = false,
                    Error = string.Empty,
                    Errors = GetModelValidationErrors()
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Search AuditLog").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LoadActionByModule(string module)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("LoadActionByModule").Add("module", module).ToInputLogString());
            try
            {
                _auditlogFacade = new AuditLogFacade();

                var actionList = _auditlogFacade.GetActionByModule(module, Resource.Ddl_Status_All, Constants.ApplicationStatus.All);
                var lstAction = new SelectList(actionList, "Key", "Value", string.Empty);

                return Json(new
                {
                    Valid = true,
                    lstAction = lstAction
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("LoadActionByModule").Add("Error Message", ex.Message).ToFailLogString());
                return Json(new
                {
                    Valid = false,
                    Error = Resource.Error_System,
                    Errors = string.Empty
                });
            }
        }

        [CheckUserRole(ScreenCode.SearchAuditLog)]
        public ActionResult BatchMonitoring()
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("BatchMonitoring").ToInputLogString());
            try
            {
                _auditlogFacade = new AuditLogFacade();
                BatchMonitoringViewModel batchMonitorVM = null;

                if (TempData["BatchMonitoringVM"] != null)
                {
                    batchMonitorVM = (BatchMonitoringViewModel)TempData["BatchMonitoringVM"];
                }
                else
                {
                    batchMonitorVM = new BatchMonitoringViewModel();
                    int intervalTime = _auditlogFacade.GetBatchInterval();
                    batchMonitorVM.IntervalTime = intervalTime;
                    batchMonitorVM.IntervalTimeInput = intervalTime.ToString(CultureInfo.InvariantCulture);
                }

                batchMonitorVM.BatchProcessList = _auditlogFacade.GetBatchProcess();
                batchMonitorVM.MonitorDateTime = DateTime.Now.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

                return View("~/Views/AuditLog/BatchMonitoring.cshtml", batchMonitorVM);

            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("BatchMonitoring").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        public ActionResult BatchProcessList()
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("BatchProcessList").ToInputLogString());
            try
            {
                _auditlogFacade = new AuditLogFacade();
                var batchMonitorVM = new BatchMonitoringViewModel();
                batchMonitorVM.BatchProcessList = _auditlogFacade.GetBatchProcess();
                batchMonitorVM.MonitorDateTime = DateTime.Now.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

                return PartialView("~/Views/AuditLog/_BatchProcessList.cshtml", batchMonitorVM);

            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("BatchProcessList").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        public ActionResult SetBatchInterval(BatchMonitoringViewModel batchMonitoringVM)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("SetBatchInterval").Add("intervalTime", batchMonitoringVM.IntervalTime).ToInputLogString());
            try
            {
                if (ModelState.IsValid)
                {
                    _auditlogFacade = new AuditLogFacade();
                    if (_auditlogFacade.SaveBatchInterval(batchMonitoringVM.IntervalTimeInput.ToNullable<int>().Value))
                    {
                        return RedirectToAction("BatchMonitoring", "AuditLog");
                    }
                }

                TempData["BatchMonitoringVM"] = batchMonitoringVM;
                return BatchMonitoring();

            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("SetBatchInterval").Add("Error Message", ex.Message).ToFailLogString());
                return Json(new
                {
                    Valid = false,
                    Error = Resource.Error_System,
                    Errors = string.Empty
                });
            }
        }

        public ActionResult RerunBatch(string processCode)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("RerunBatch").Add("processCode", processCode).ToInputLogString());
            try
            {
                switch (processCode)
                {
                    case Constants.BatchProcessCode.CreateCommPool:
                        Logger.Info("I:--START--:--Get Mailbox--");
                        GetMailboxJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Get Mailbox--");
                        break;
                    case Constants.BatchProcessCode.ImportAFS:
                        Logger.Info("I:--START--:--Get AFSFile--");
                        GetFileAFSJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Get AFSFile--");
                        break;
                    case Constants.BatchProcessCode.ExportAFS:
                        Logger.Info("I:--START--:--Export AFSFile--");
                        ExportFileAFSJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Export AFSFile--");
                        break;
                    case Constants.BatchProcessCode.ExportMarketing:
                        Logger.Info("I:--START--:--Export NCBFile--");
                        ExportFileNCBJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Export NCBFile--");
                        break;
                    case Constants.BatchProcessCode.ImportBDW:
                        Logger.Info("I:--START--:--Get BDWFile--");
                        GetFileBDWJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Get BDWFile--");
                        break;
                    case Constants.BatchProcessCode.ImportCIS:
                        Logger.Info("I:--START--:--Get CISFile--");
                        GetFileCISJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Get CISFile--");
                        break;
                    case Constants.BatchProcessCode.ImportHP:
                        Logger.Info("I:--START--:--Get HPFile--");
                        GetFileHpJobAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Get HPFile--");
                        break;
                    case Constants.BatchProcessCode.SyncSRStatusFromReplyEmail:
                        Logger.Info("I:--START--:--Create SR Activity from Reply Email--");
                        CreateSRActivityFromReplyEmail().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Create SR Activity from Reply Email--");
                        break;
                    case Constants.BatchProcessCode.ReSubmitActivityToCARSystem:
                        Logger.Info("I:--START--:--Re-Submit SR Activity to CAR System--");
                        ReSubmitActivityToCARSystem().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Re-Submit SR Activity to CAR System--");
                        break;
                    case Constants.BatchProcessCode.ReSubmitActivityToCBSHPSystem:
                        Logger.Info("I:--START--:--Re-Submit SR Activity to CBSHP System (Log100)--");
                        ReSubmitActivityToCBSHPSystem().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Re-Submit SR Activity to CBSHP System (Log100)--");
                        break;
                    case Constants.BatchProcessCode.ImportHR:
                        Logger.Info("I:--START--:--Get HRIS File--");
                        GetFileHRISAsync().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Get HRIS File--");
                        break;
                    case Constants.BatchProcessCode.ExportDailySRReport:
                        Logger.Info("I:--START--:--Export Daily SR Report--");
                        ExportDailySRReport().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Export Daily SR Report--");
                        break;
                    case Constants.BatchProcessCode.ExportMonthlySRReport:
                        Logger.Info("I:--START--:--Export Monthly SR Report--");
                        ExportMonthlySRReport().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Export Monthly SR Report--");
                        break;
                    case Constants.BatchProcessCode.ExportDoNotCallUpdateFileToChannel:
                        Logger.Info("I:--START--:--Export Do not call Update File To Channel--");
                        ExportDoNotCallFileToChannel().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Export Do not call Update File To Channel--");
                        break;
                    case Constants.BatchProcessCode.ExportDoNotCallUpdateFileToTOT:
                        Logger.Info("I:--START--:--Export Do not call Update File To TOT--");
                        ExportDoNotCallFileToTOT().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Export Do not call Update File To TOT--");
                        break;
                    case Constants.BatchProcessCode.ExecuteDoNotCallBatchSelectService:
                        Logger.Info("I:--START--:--Execute Do not call Batch Select Service--");
                        ExecuteDoNotCallSelectServide().ConfigureAwait(false);
                        Logger.Info("O:--SUCCESS--:--Execute Do not call Batch Select Service--");
                        break;
                    default:
                        // do other stuff...
                        break;
                }

                Thread.Sleep(5000);

                return Json(new
                {
                    Valid = true,
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("RerunBatch").Add("Error Message", ex.Message).ToFailLogString());
                return Json(new
                {
                    Valid = false,
                    Error = Resource.Error_System,
                    Errors = string.Empty
                });
            }
        }

        #region "Call WebService"

        private static async Task GetMailboxJobAsync()
        {
            using (var client = new CSMMailServiceClient())
            {
                Task<JobTaskResponse> t = client.GetMailboxAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                JobTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task GetFileAFSJobAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ImportAFSTaskResponse> t = client.GetFileAFSAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ImportAFSTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task ExportFileAFSJobAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ExportAFSTaskResponse> t = client.ExportFileAFSAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ExportAFSTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task ExportFileNCBJobAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ExportNCBTaskResponse> t = client.ExportFileNCBAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ExportNCBTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task GetFileBDWJobAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ImportBDWTaskResponse> t = client.GetFileBDWAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword(), true);
                CSM.Service.Messages.SchedTask.ImportBDWTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task GetFileHRISAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ImportHRTaskResponse> t = client.GetFileHRAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ImportHRTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task GetFileCISJobAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ImportCISTaskResponse> t = client.GetFileCISAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ImportCISTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task GetFileHpJobAsync()
        {
            using (var client = new CSMFileServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ImportHpTaskResponse> t = client.GetFileHPAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ImportHpTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task CreateSRActivityFromReplyEmail()
        {
            using (var client = new CSMSRServiceClient())
            {
                //Task<CSM.Service.Messages.SchedTask.CreateSrFromReplyEmailTaskResponse> t = client.CreateSRActivityFromReplyEmailAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                //CSM.Service.Messages.SchedTask.CreateSrFromReplyEmailTaskResponse result = await t.ConfigureAwait(false);
                using (Task<CSM.Service.Messages.SchedTask.CreateSrFromReplyEmailTaskResponse> t = client.CreateSRActivityFromReplyEmailAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword()))
                {
                    CSM.Service.Messages.SchedTask.CreateSrFromReplyEmailTaskResponse result = await t.ConfigureAwait(false);
                }
            }
        }

        private static async Task ReSubmitActivityToCARSystem()
        {
            using (var client = new CSMSRServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ReSubmitActivityToCARSystemTaskResponse> t = client.ReSubmitActivityToCARSystemAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ReSubmitActivityToCARSystemTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private static async Task ReSubmitActivityToCBSHPSystem()
        {
            using (var client = new CSMSRServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ReSubmitActivityToCBSHPSystemTaskResponse> t = client.ReSubmitActivityToCBSHPSystemAsync(WebConfig.GetWebUsername(), WebConfig.GetWebPassword());
                CSM.Service.Messages.SchedTask.ReSubmitActivityToCBSHPSystemTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private async Task ExportMonthlySRReport()
        {
            using (var client = new CSMSRServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ExportSRTaskResponse> t = client.SRReportAsync();
                CSM.Service.Messages.SchedTask.ExportSRTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private async Task ExportDailySRReport()
        {
            using (var client = new CSMSRServiceClient())
            {
                Task<CSM.Service.Messages.SchedTask.ExportSRTaskResponse> t = client.DailySRReportAsync();
                CSM.Service.Messages.SchedTask.ExportSRTaskResponse result = await t.ConfigureAwait(false);
            }
        }

        private async Task ExportDoNotCallFileToChannel()
        {
            using (var client = new DoNotCallBatchService.DoNotCallBatchProcessClient())
            {
                CSM.Service.Messages.DoNotCall.ExportFileRequest req = new Service.Messages.DoNotCall.ExportFileRequest();
                req.Header = new Service.Messages.Common.Header()
                {
                    channel_id = "CSM",
                    password = "CSM",
                    user_name = "CSM",
                    reference_no = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    service_name = "ExportFile",
                    system_code = "CSM",
                    transaction_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                Task<CSM.Service.Messages.DoNotCall.ExportFileResponse> t = client.ExportFileAsync(req);
                CSM.Service.Messages.DoNotCall.ExportFileResponse result = await t.ConfigureAwait(false);
            }
        }

        private async Task ExportDoNotCallFileToTOT()
        {
            using (var client = new DoNotCallBatchService.DoNotCallBatchProcessClient())
            {
                CSM.Service.Messages.DoNotCall.ExportFileToTotRequest req = new Service.Messages.DoNotCall.ExportFileToTotRequest();
                req.Header = new Service.Messages.Common.Header()
                {
                    channel_id = "CSM",
                    password = "CSM",
                    user_name = "CSM",
                    reference_no = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    service_name = "ExportFileToTOT",
                    system_code = "CSM",
                    transaction_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                Task<CSM.Service.Messages.DoNotCall.ExportFileResponse> t = client.ExportFileToTOTAsync(req);
                CSM.Service.Messages.DoNotCall.ExportFileResponse result = await t.ConfigureAwait(false);
            }
        }

        private async Task ExecuteDoNotCallSelectServide()
        {
            using (var client = new DoNotCallBatchSelectService.DoNotCallBatchSelectServiceClient())
            {
                Task<DoNotCallBatchSelectService.DoNotCallBatchSelectServiceResponse> t = client.ExecuteBatchSelectServiceAsync();
                DoNotCallBatchSelectService.DoNotCallBatchSelectServiceResponse result = await t.ConfigureAwait(false);
            }
        }
        #endregion
    }
}
