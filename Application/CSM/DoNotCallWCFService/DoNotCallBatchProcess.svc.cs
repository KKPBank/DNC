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
using System.Linq;

namespace DoNotCallWCFService
{
    public class DoNotCallBatchProcess : IDoNotCallBatchProcess
    {
        private readonly ILog _logger;
        private IDoNotCallFacade _doNotCallFacade;
        private ICommonFacade _commonFacade;
        private CSMMailSender _mailSender;
        private LogMessageBuilder _logMsg = new LogMessageBuilder();

        public DoNotCallBatchProcess()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                // Set logfile name and application name variables
                GlobalContext.Properties["ApplicationCode"] = "DNCWS";
                GlobalContext.Properties["ServerName"] = Environment.MachineName;
                _logger = LogManager.GetLogger(typeof(DoNotCallBatchProcess));
            }
            catch (Exception ex)
            {
                _logger.Error("Exception occur:\n", ex);
            }
        }

        public ExportFileResponse ExportFileToTOT(ExportFileToTotRequest request)
        {
            string methodName = $"{nameof(DoNotCallBatchProcess)}.{nameof(ExportFileToTOT)}";
            var response = new ExportFileResponse();
            DateTime startTime = DateTime.Now;
            response.StartTime = startTime;
            try
            {
                if(request == null || request.Header == null)
                {
                    throw new NullReferenceException($"{methodName}: Request/Header is null!");
                }

                Header requestHeader = request.Header;

                response.Header = GetResponseHeader(requestHeader, methodName);

                // Authenticate user
                if (!ValidateServiceRequest(requestHeader))
                    return SetInvalidLoginResponse(response);

                _logger.DebugFormat("-- XMLRequest --\n{0}", request.SerializeObject());

                // Export 
                bool exportSuccess = ExportDataToTOT(request.ExecuteTime, response);

                if (exportSuccess)
                    response.ResultStatus = Constants.StatusResponse.Success;

                return SetSuccessResponse(response);
            }
            catch (Exception ex)
            {
                return SetErrorResponse(response, ex);
            }
            finally
            {
                //Insert Audit Log
                bool success = response.ResultStatus == Constants.StatusResponse.Success;
                string detail = !string.IsNullOrWhiteSpace(response.Error) ? response.Error : response.Description;
                InsertAuditLog(methodName, success, detail ?? response.ResultStatus);
            }
        }

        public ExportFileResponse ExportFile(ExportFileRequest request)
        {
            string methodName = $"{nameof(DoNotCallBatchProcess)}.{nameof(ExportFile)}";
            var response = new ExportFileResponse();
            DateTime startTime = DateTime.Now;
            response.StartTime = startTime;
            try
            {
                if (request == null || request.Header == null)
                {
                    throw new NullReferenceException($"{methodName}: Request/Header is null!");
                }
                var requestHeader = request.Header;

                response.Header = GetResponseHeader(requestHeader, methodName);
                // Set batch process status
                if (AppLog.BatchProcessStart(Constants.BatchProcessCode.ExportDoNotCallUpdateFile, startTime) == false)
                    return SetUnprocessResponse(response, methodName);

                // Authenticate user
                if (!ValidateServiceRequest(request.Header))
                    return SetInvalidLoginResponse(response);

                _logger.DebugFormat("-- XMLRequest --\n{0}", request.SerializeObject());

                // Export 
                ExportUpdateData(response);

                return SetSuccessResponse(response);
            }
            catch (Exception ex)
            {
                DateTime errorTime = DateTime.Now;
                response = SetErrorResponse(response, ex);
                _mailSender = CSMMailSender.GetCSMMailSender();
                _mailSender.NotifyFailExportDoNotCall(response.StartTime, errorTime, response.ElapsedTime, response.Error, response.ExportDataCount);

                return response;
            }
            finally
            {
                UpdateBatchProcessStatus(response);
                //Insert Audit Log
                bool success = response.ResultStatus == Constants.StatusResponse.Success;
                string detail = !string.IsNullOrWhiteSpace(response.Error) ? response.Error : response.Description;
                InsertAuditLog(methodName, success, detail ?? response.ResultStatus);
            }
        }

        #region Private

        private bool ExportDataToTOT(string executeTime, ExportFileResponse response)
        {
            var now = DateTime.Now;
            _doNotCallFacade = new DoNotCallFacade();
            List<DoNotCallUpdatePhoneNoModel> newDataList = _doNotCallFacade.GetDoNotCallPhoneNoListByExecuteTime(executeTime);
            int itemCount = newDataList.Count;
            if (itemCount > 0)
            {
                // Generate file
                byte[] fileByte = _doNotCallFacade.GenerateDoNotCallPhoneListAttachment(newDataList);
                // Send email 
                _mailSender = CSMMailSender.GetCSMMailSender();
                int activeCount = newDataList.Count(x => x.Status == Constants.DNC.Block);
                int inactiveCount = newDataList.Count(x => x.Status == Constants.DNC.Unblock);
                string fileName = now.FormatDateTime(Constants.DateTimeFormat.ExportDateTime);
                string attachmentFileName = $"{fileName}.csv";
                bool success = _mailSender.ExpportUpdatePhoneStatusToTOT(now, executeTime, activeCount, inactiveCount, fileByte, attachmentFileName);
                // generate file
                _doNotCallFacade.GenerateDoNotCallPhoneListFile(fileByte, fileName, success);

                response.ExportDataCount = success ? itemCount : 0;
                return success;
            }
            return true;
        }

        private void ExportUpdateData(ExportFileResponse response)
        {
            _doNotCallFacade = new DoNotCallFacade();
            // Update expired items and get update data list
            List<DoNotCallUpdatePhoneNoModel> newDataList = _doNotCallFacade.GetUpdatedDoNotCallPhoneNoList();
            int dataCount = newDataList?.Count ?? 0;
            if (dataCount > 0)
            {
                // Export new data to csv file
                response.ExportDataCount = dataCount;
                _doNotCallFacade.ExportDoNotCallUpdateFile(newDataList);
                response.Description = $"Exported data count: {dataCount}";
            }
            else
            {
                response.Description = "File not created. No new update data";
            }
        }

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

        private static void UpdateBatchProcessStatus(ExportFileResponse response)
        {
            if (response.ResultStatus != Constants.StatusResponse.NotProcess)
            {
                int batchStatus = (response.ResultStatus == Constants.StatusResponse.Success)
                    ? Constants.BatchProcessStatus.Success
                    : Constants.BatchProcessStatus.Fail;

                string detail = string.IsNullOrWhiteSpace(response.Error) ? response.Description : response.Error;

                AppLog.BatchProcessEnd(Constants.BatchProcessCode.ExportDoNotCallUpdateFile, batchStatus, response.EndTime, response.ElapsedTime, detail);
            }
        }

        private bool ValidateServiceRequest(Header requestHeader)
        {
            _commonFacade = new CommonFacade();
            string doNotCallProfilePath = _commonFacade.GetProfileXml("DoNotCallProfile");
            bool valid = _commonFacade.VerifyServiceRequest<Header>(requestHeader, doNotCallProfilePath);
            return valid;
        }

        private ExportFileResponse SetErrorResponse(ExportFileResponse response, Exception ex)
        {
            _logger.InfoFormat($"O:--FAILED--:Error Message/{ex.Message}");
            _logger.Error("Exception occur:\n", ex);
            response.Error = ex.Message;
            return response;
        }

        private ExportFileResponse SetSuccessResponse(ExportFileResponse response)
        {
            response.EndTime = DateTime.Now;
            response.ResultStatus = Constants.StatusResponse.Success;
            response.Error = string.Empty;

            _logger.Debug("-- Finish Job --:ElapsedMilliseconds/" + response.ElapsedTime.TotalMilliseconds);
            return response;
        }

        private ExportFileResponse SetInvalidLoginResponse(ExportFileResponse response)
        {
            response.EndTime = DateTime.Now;
            string errorMessage = "Bad Request, the header is not valid";
            response.Error = errorMessage;

            _logger.Info($"O:--LOGIN--:Error Message/{errorMessage}");
            return response;
        }

        private ExportFileResponse SetUnprocessResponse(ExportFileResponse response, string methodName)
        {
            response.EndTime = DateTime.Now;
            response.ResultStatus = Constants.StatusResponse.NotProcess;
            response.Error = string.Empty;

            _logger.Info($"I:--NOT START--:--{methodName}--");
            _logger.Debug($"-- Finish Cron Job --:ElapsedMilliseconds/" + response.ElapsedTime.TotalMilliseconds);
            return response;
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
        #endregion

        #region "IDisposable"
        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (_doNotCallFacade != null) { _doNotCallFacade.Dispose(); }
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
