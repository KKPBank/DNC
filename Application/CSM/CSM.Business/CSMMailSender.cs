using System;
using System.Globalization;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using CSM.Common.Mail;
using System.Collections.Generic;
using CSM.Common.Utilities;
using CSM.Service.Messages.SchedTask;
using log4net;
using CSM.Common.Resources;
using System.Text;
using System.Collections;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Net;

namespace CSM.Business
{
    public class CSMMailSender : MailSender
    {
        private static XDocument _mailSubjectDoc = null;
        private ICommonFacade _commonFacade;
        private const string TmpFolder = "~/Templates/Mail/";
        private static readonly CSMMailSender EmailSender = new CSMMailSender();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CSMMailSender));

        // Mail Templates
        private const string TmpExportDoNotCallPhoneNoToTOT = "ExportDoNotCallPhoneNoToTOT.html";
        private const string TmpNotifyFailExportDoNotCall = "NotifyFailExportDoNotCall.html";
        private const string TmpNotifySyncEmailFailed = "NotifySyncEmailFailed.html";
        private const string TmpNotifySyncEmailSuccess = "NotifySyncEmailSuccess.html";
        private const string TmpNotifyImportAssetFailed = "NotifyImportAssetFailed.html";
        private const string TmpNotifyImportAssetSuccess = "NotifyImportAssetSuccess.html";
        private const string TmpNotifyExportActivityFailed = "NotifyExportActivityFailed.html";
        private const string TmpNotifyExportActivitySuccess = "NotifyExportActivitySuccess.html";
        private const string TmpNotifyFailExportActvity = "NotifyFailExportActvity.html";
        private const string TmpNotifyExportSRReportFailed = "NotifyExportSRReportFailed.html";
        private const string TmpNotifyImportContactSuccess = "NotifyImportContactSuccess.html";
        private const string TmpNotifyImportContactFailed = "NotifyImportContactFailed.html";
        private const string TmpNotifyImportCISSuccess = "NotifyImportCISSuccess.html";
        private const string TmpNotifyImportCISFailed = "NotifyImportCISFailed.html";
        private const string TmpNotifyImportHPSuccess = "NotifyImportHPSuccess.html";
        private const string TmpNotifyImportHPFailed = "NotifyImportHPFailed.html";
        private const string TmpNotifyCreateSrFromReplyEmailSuccess = "NotifyCreateSrFromReplyEmailSuccess.html";
        private const string TmpNotifyCreateSrFromReplyEmailFailed = "NotifyCreateSrFromReplyEmailFailed.html";
        private const string TmpNotifyReSubmitActivityToCARSystemSuccess = "NotifyReSubmitActivityToCARSystemSuccess.html";
        private const string TmpNotifyReSubmitActivityToCARSystemFailed = "NotifyReSubmitActivityToCARSystemFailed.html";
        private const string TmpNotifyReSubmitActivityToCBSHPSystemSuccess = "NotifyReSubmitActivityToCBSHPSystemSuccess.html";
        private const string TmpNotifyReSubmitActivityToCBSHPSystemFailed = "NotifyReSubmitActivityToCBSHPSystemFailed.html";

        public static CSMMailSender GetCSMMailSender()
        {
            return EmailSender;
        }

        public bool SendMail(string senderEmail, string receiverEmails, string ccEmails, string subject, string message, 
            List<byte[]> attachmentStreams, List<string> attachmentFilenames, bool isSensitive)
        {
            return base.SendMail(senderEmail, receiverEmails, ccEmails, subject, message, attachmentStreams, attachmentFilenames, isSensitive);
        }

        public bool SendMail(string senderEmail, string receiverEmails, string ccEmails, string bccEmails, string subject, string message,
            List<byte[]> attachmentStreams, List<string> attachmentFilenames, bool isSensitive)
        {
            return base.SendMail(senderEmail, receiverEmails, ccEmails, bccEmails, subject, message, attachmentStreams, attachmentFilenames, isSensitive);
        }

        public bool NotifySyncEmailFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifySyncEmailFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifySyncEmailFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifySyncEmailFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyFailExportDoNotCall(DateTime startTime, DateTime errorDateTime, TimeSpan elapsedTime, string errorMessage, int exportDataCount)
        {
            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATE", startTime.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate));
            hData.Add("ERROR_DATETIME", $"วันที่ {errorDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate)} เวลา {errorDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime)} น.");
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", elapsedTime));
            hData.Add("ERROR_MESSAGE", errorMessage);
            string countUnit = exportDataCount > 1 ? "Records" : "Record";
            hData.Add("TOTAL_RECORDS", $"{exportDataCount.FormatNumber()} {countUnit}");

            string subject = GetMailSubject(Constants.MailSubject.NotifyFailExportDoNotCall).NamedFormat(new { timestamp = startTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyFailExportDoNotCall);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat($"Mail-template ({TmpNotifyFailExportDoNotCall}) not found, notify's email can not send");
                return false;
            }

            string message = GenText(templateFilePath, hData);

            return SendMailUsingDbConfig(subject, message);
        }

        public bool ExpportUpdatePhoneStatusToTOT(DateTime taskTime, string executeTime, int activeCount, int inactiveCount, byte[] attachment, string attachmentName)
        {
            Hashtable hData = new Hashtable();
            int totalExportCount = activeCount + inactiveCount;
            string scheduleDate = taskTime.FormatDateTime(Constants.DateTimeFormat.DefaultShortDate);
            string exportCount = totalExportCount.FormatNumber();
            hData.Add("SCHEDULE_DATE", scheduleDate);
            hData.Add("EXECUTE_TIME", executeTime);
            hData.Add("EXPORT_COUNT", exportCount);

            string subject = GetMailSubject(Constants.MailSubject.ExportDoNotCallPhoneNoToTOT)
                            .NamedFormat(new { scheduleDate, executeTime, exportCount });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpExportDoNotCallPhoneNoToTOT);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpExportDoNotCallPhoneNoToTOT);
                return false;
            }

            string message = GenText(templateFilePath, hData);

            return SendMailUsingDbConfig(subject, message, attachment, attachmentName);
        }

        private bool SendMailUsingDbConfig(string subject, string message, byte[] byteArray = null, string attachmentName=null)
        {
            try
            {
                _commonFacade = new CommonFacade();
                const string delimeter = ",";
                string senderName = _commonFacade.GetCacheParamByName("MailSenderName").ParamValue;
                string senderEmail = _commonFacade.GetCacheParamByName("MailSenderEmail").ParamValue;
                string receiverStr = _commonFacade.GetCacheParamByName("MailTOTReceiverTo").ParamValue;
                string ccMailStr = _commonFacade.GetCacheParamByName("MailTOTReceiverCC").ParamValue;
                EmailAddress sender = new EmailAddress(senderEmail, senderName); 
                List<EmailAddress> receivers = GetEmailAddresses(receiverStr, ','); 
                List<EmailAddress> ccAddresses = GetEmailAddresses(ccMailStr, ',');

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(sender.Address, sender.Name),
                    SubjectEncoding = Encoding.GetEncoding("TIS-620"),
                    Subject = subject,
                    Body = message,
                    BodyEncoding = Encoding.GetEncoding("TIS-620"), //Encoding.UTF8;
                    IsBodyHtml = true
                };

                if (receivers == null || receivers.Count == 0)
                {
                    Logger.Error("Failed to process message. The receiver cannot be null or empty.");
                    return false;
                }

                foreach (EmailAddress user in receivers.Where(user => !string.IsNullOrEmpty(user.Address)))
                {
                    mail.To.Add(new MailAddress(user.Address, user.Name));
                }

                if (ccAddresses != null && ccAddresses.Count > 0)
                {
                    foreach (EmailAddress user in ccAddresses.Where(user => !string.IsNullOrEmpty(user.Address)))
                    {
                        mail.CC.Add(new MailAddress(user.Address, user.Name));
                    }
                }

                if (byteArray != null && byteArray.Length > 0)
                {
                    Attachment attachment = new Attachment(new MemoryStream(byteArray), attachmentName);
                    mail.Attachments.Add(attachment);
                }

                SmtpClient mailClient = GetMailClientFromDbConfig();

                bool sendSuccess = StartSendMail(mail, mailClient);

                Logger.Info("Sending mail to " + receivers.Select(t => t.Name).Aggregate((i, j) => i + delimeter + j) + " at " + receivers.Select(t => t.Address).Aggregate((i, j) => i + delimeter + j) + subject);

                if (ccAddresses != null && ccAddresses.Count > 0)
                {
                    Logger.Info("Sending mail cc to " + ccAddresses.Select(t => t.Name).Aggregate((i, j) => i + delimeter + j) + " at " + ccAddresses.Select(t => t.Address).Aggregate((i, j) => i + delimeter + j) + " with subject " + subject);
                }

                return sendSuccess;
            }
            catch (Exception ex)
            {
                Logger.Error("Failure sending mail:\n", ex);
                return false;
            }
        }

        private SmtpClient GetMailClientFromDbConfig()
        {
            _commonFacade = new CommonFacade();
            SmtpClient mailClient = new SmtpClient
            {
                Host = _commonFacade.GetCacheParamByName("MailServer").ParamValue,
                Port = int.Parse(_commonFacade.GetCacheParamByName("MailServerPort").ParamValue),
            };

            string authenMethod = _commonFacade.GetCacheParamByName("MailAuthenMethod").ParamValue;
            string authenUsername = _commonFacade.GetCacheParamByName("MailAuthenUser").ParamValue;
            string authenPassword = _commonFacade.GetCacheParamByName("MailAuthenPassword").ParamValue;

            if (authenMethod == "default")
            {
                mailClient.UseDefaultCredentials = true;
            }
            else
            {
                mailClient.Credentials = new NetworkCredential(authenUsername, authenPassword);
                mailClient.EnableSsl = true;
            }

            return mailClient;
        }

        public bool NotifySyncEmailFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifySyncEmailFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifySyncEmailFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifySyncEmailFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifySyncEmailSuccess(string strReceivers, DateTime scheduledDate, JobTaskResult taskResult)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResult.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", taskResult.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResult.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResult.ElapsedTime));
            hData.Add("TOTAL_EMAIL_READ", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResult.TotalEmailRead));
            hData.Add("NUM_OF_SR", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResult.NumOfSR));
            hData.Add("NUM_OF_FAX", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResult.NumOfFax));
            hData.Add("NUM_OF_KK_WEBSITE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResult.NumOfKKWebSite));
            hData.Add("NUM_OF_EMAIL", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResult.NumOfEmail));
            hData.Add("NUM_OF_FAILED_DELETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResult.NumFailedDelete));

            if (taskResult.StatusResponse != null)
            {
                hData.Add("ERROR_MESSAGE", string.Format("<tr><td class='label'>Error Message</td><td>{0}</td></tr>", taskResult.StatusResponse.Description));
            }

            string subject = GetMailSubject(Constants.MailSubject.NotifySyncEmailSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifySyncEmailSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifySyncEmailSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportAssetFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportAssetFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportAssetFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportAssetFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportAssetFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportAssetFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportAssetFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportAssetFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportAssetSuccess(string strReceivers, ImportAFSTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("READING_FILES", StringHelpers.ConvertListToString(taskResponse.FileList, "/"));
            hData.Add("NUM_OF_PROPERTY", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfProp));
            hData.Add("NUM_OF_SALEZONES", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfSaleZones));
            hData.Add("NUM_OF_COMPLETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfComplete));
            hData.Add("NUM_OF_ERRPROP", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfErrProp));
            hData.Add("NUM_OF_ERR_SALEZONES", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfErrSaleZone));

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportAssetSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportAssetSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportAssetSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyExportActivityFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();
            
            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyExportActivityFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyExportActivityFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyExportActivityFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyExportActivityFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyExportActivityFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyExportActivityFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyExportActivityFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyExportActivitySuccess(string strReceivers, ExportAFSTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("NUM_OF_ACTIVITY", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfActivity));

            string subject = GetMailSubject(Constants.MailSubject.NotifyExportActivitySuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyExportActivitySuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyExportActivitySuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyFailExportActvityFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyFailExportActvity).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyFailExportActvity);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyFailExportActvity);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyFailExportActvity(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyFailExportActvity).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyFailExportActvity);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyFailExportActvity);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportContactFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportAssetFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportAssetFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportAssetFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportContactFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportContactFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportContactFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportContactFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportContactSuccess(string strReceivers, ImportBDWTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("READING_FILES", StringHelpers.ConvertListToString(taskResponse.FileList, "/"));
            hData.Add("NUM_OF_TOTAL", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfBdwContact));
            hData.Add("NUM_OF_COMPLETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfComplete));
            hData.Add("NUM_OF_ERROR", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfError));

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportContactSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportContactSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportContactSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportCISFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();
            
            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportCISFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportCISFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportCISFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportCISFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportCISFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportCISFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportCISFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportCISFailed(string strReceivers, ImportCISTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("TASK_RESULT_TABLE", ConvertCISFileListToHtml(taskResponse));
            

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportCISSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportCISSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportCISSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportHPFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);
            string errorMsg = exception.ToErrorMessage().ToLineBreak();
            
            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportHPFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportHPFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportHPFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportHPFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportHPFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportHPFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportHPFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyImportHPSuccess(string strReceivers, ImportHpTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("READING_FILES", StringHelpers.ConvertListToString(taskResponse.FileList, "/"));
            hData.Add("NUM_OF_TOTAL", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfTotal));
            hData.Add("NUM_OF_COMPLETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfComplete));
            hData.Add("NUM_OF_ERROR", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfError));

            string subject = GetMailSubject(Constants.MailSubject.NotifyImportHPSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyImportHPSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyImportHPSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyCreateSrFromReplyEmailSuccess(string strReceivers, CreateSrFromReplyEmailTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("NUM_OF_TOTAL", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfTotal));
            hData.Add("NUM_OF_COMPLETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfComplete));
            hData.Add("NUM_OF_ERROR", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfError));

            string subject = GetMailSubject(Constants.MailSubject.NotifyCreateSrFromReplyEmailSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyCreateSrFromReplyEmailSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyCreateSrFromReplyEmailSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }
        
        public bool NotifyCreateSrFromReplyEmailFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            return NotifyCreateSrFromReplyEmailFailed(strReceivers, scheduledDate, exception.ToErrorMessage());
        }

        public bool NotifyCreateSrFromReplyEmailFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyCreateSrFromReplyEmailFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyCreateSrFromReplyEmailFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyCreateSrFromReplyEmailFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyReSubmitActivityToCARSystemSuccess(string strReceivers, ReSubmitActivityToCARSystemTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("NUM_OF_TOTAL", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfTotal));
            hData.Add("NUM_OF_COMPLETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfComplete));
            hData.Add("NUM_OF_ERROR", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfError));

            string subject = GetMailSubject(Constants.MailSubject.NotifyReSubmitActivityToCARSystemSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyReSubmitActivityToCARSystemSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyReSubmitActivityToCARSystemSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyReSubmitActivityToCARSystemFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            return NotifyReSubmitActivityToCARSystemFailed(strReceivers, scheduledDate, exception.ToErrorMessage());
        }

        public bool NotifyReSubmitActivityToCARSystemFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyReSubmitActivityToCARSystemFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyReSubmitActivityToCARSystemFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyReSubmitActivityToCARSystemFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyReSubmitActivityToCBSHPSystemSuccess(string strReceivers, ReSubmitActivityToCBSHPSystemTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime));
            hData.Add("SCHEDULE_DATE", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ELAPSED_TIME", string.Format(CultureInfo.InvariantCulture, "{0} (ms)", taskResponse.ElapsedTime));
            hData.Add("NUM_OF_TOTAL", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfTotal));
            hData.Add("NUM_OF_COMPLETE", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfComplete));
            hData.Add("NUM_OF_ERROR", string.Format(CultureInfo.InvariantCulture, "{0} records", taskResponse.NumOfError));

            string subject = GetMailSubject(Constants.MailSubject.NotifyReSubmitActivityToCBSHPSystemSuccess).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyReSubmitActivityToCBSHPSystemSuccess);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyReSubmitActivityToCBSHPSystemSuccess);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyReSubmitActivityToCBSHPSystemFailed(string strReceivers, DateTime scheduledDate, AggregateException exception)
        {
            return NotifyReSubmitActivityToCBSHPSystemFailed(strReceivers, scheduledDate, exception.ToErrorMessage());
        }

        public bool NotifyReSubmitActivityToCBSHPSystemFailed(string strReceivers, DateTime scheduledDate, string errorMsg)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = scheduledDate.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            hData.Add("SCHEDULE_DATE", scheduledDate.FormatDateTime(Constants.DateTimeFormat.ShortDate));
            hData.Add("SCHEDULE_TIME", scheduledDate.FormatDateTime(Constants.DateTimeFormat.FullTime));
            hData.Add("ERROR_MESSAGE", errorMsg);

            string subject = GetMailSubject(Constants.MailSubject.NotifyReSubmitActivityToCBSHPSystemFailed).NamedFormat(new { timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyReSubmitActivityToCBSHPSystemFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyReSubmitActivityToCBSHPSystemFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);
            //Logger.InfoFormat("I:--Message Body--:Detail/{0}", message);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        public bool NotifyExportSRReportFailed(ExportSRTaskResponse taskResponse)
        {
            string senderEmail = WebConfig.GetSenderEmail();
            string senderName = WebConfig.GetSenderName();
            string strReceivers = WebConfig.GetTaskEmailToAddress();
            EmailAddress sender = new EmailAddress(senderEmail, senderName);
            List<EmailAddress> receivers = GetEmailAddresses(strReceivers);
            string schedDateTime = taskResponse.SchedDateTime.FormatDateTime(Constants.DateTimeFormat.DefaultFullDateTime);

            Hashtable hData = new Hashtable();
            hData.Add("REPORT_TYPE", taskResponse.ReportType);
            hData.Add("SCHEDULE_DATETIME", schedDateTime);
            //hData.Add("ERROR_MESSAGE", taskResponse.ToHtml());
            hData.Add("ERROR_MESSAGE", taskResponse.ToString().TextToHtml());

            string subject = GetMailSubject(Constants.MailSubject.NotifyExportSRReportFailed).NamedFormat(new { reporttype = taskResponse.ReportType, timestamp = schedDateTime });
            string templateFilePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetTemplatesDirectory(), TmpNotifyExportSRReportFailed);

            if (!File.Exists(templateFilePath))
            {
                Logger.ErrorFormat("Mail-template ({0}) not found, notify's email can not send", TmpNotifyExportSRReportFailed);
                return false;
            }

            string message = GenText(templateFilePath, hData);

            return SendMail(sender, receivers, null, subject, message, null, null);
        }

        #region "Functions"

        private static string GetMailSubject(string templateSubject)
        {
            try
            {
                if (_mailSubjectDoc == null)
                {
                    string template = string.Format(CultureInfo.InvariantCulture, "{0}/MailSubjectTemplate.xml", GetTemplatesDirectory());
                    if (!File.Exists(template))
                    {
                        throw new FileNotFoundException("Mail subject template file not found.");
                    }

                    _mailSubjectDoc = XDocument.Load(template);
                }

                return (from fn in _mailSubjectDoc.Descendants("template")
                        where fn.Attribute("name").Value.ToUpper(CultureInfo.InvariantCulture) == templateSubject.ToUpper(CultureInfo.InvariantCulture)
                        select fn.Element("subject").Value).FirstOrDefault<string>();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                throw;
            }
        }

        private static List<EmailAddress> GetEmailAddresses(string strReceivers, char separator=';')
        {
            List<EmailAddress> receivers = null;
            IList<object> results = StringHelpers.ConvertStringToList(strReceivers, separator);

            if (results != null)
            {
                receivers = (from str in results
                             select new EmailAddress
                             {
                                 Address = (string)str,
                                 Name = (string)str
                             }).ToList();
            }

            return receivers;
        }

        private static string ConvertListToHtml(List<JobTaskResult> taskResults)
        {
            if (taskResults != null && taskResults.Count > 0)
            {
                const string tmplCol = "<TD CLASS='text-right'>{0}</TD>";
                var tblHeader = new[] { "Total email read", "Found SR", "Job type fax", "Job type KK web site", "Job type email" };

                StringBuilder sb = new StringBuilder();
                sb.Append("<TABLE CLASS='data-list'>\n");

                #region "Header"

                sb.Append("<TR>\n");
                sb.Append("<TH CLASS='top'></TH>\n");
                foreach (var col in tblHeader.Select(x => x))
                {
                    sb.AppendFormat("<TH CLASS='top'>{0}</TH>", col);
                }
                sb.Append("</TR>\n");

                foreach (var result in taskResults)
                {
                    sb.Append("<TR>\n");
                    sb.AppendFormat("<TD CLASS='label'>{0}</TD>", result.Username);
                    sb.AppendFormat(tmplCol, result.TotalEmailRead);
                    sb.AppendFormat(tmplCol, result.NumOfSR);
                    sb.AppendFormat(tmplCol, result.NumOfFax);
                    sb.AppendFormat(tmplCol, result.NumOfKKWebSite);
                    sb.AppendFormat(tmplCol, result.NumOfEmail);
                    sb.Append("</TR>\n");
                }

                #endregion

                sb.Append("</TABLE>");

                return sb.ToString();
            }

            return string.Empty;
        }

        private static string ConvertCISFileListToHtml(ImportCISTaskResponse taskResponse)
        {
            List<object> fileList = taskResponse.FileList;
            List<object> fileErrorList = taskResponse.FileErrorList;
            List<object> numOfTotalList = new List<object> { taskResponse.NumOfTitle, taskResponse.NumOfCor, taskResponse.NumOfIndiv, taskResponse.NumOfProd, taskResponse.NumOfSub, taskResponse.NumOfCountry, taskResponse.NumOfPro, taskResponse.NumOfDis, taskResponse.NumOfSubDis, taskResponse.NumOfPhonetype, taskResponse.NumOfEmailtype, taskResponse.NumOfAddressType, taskResponse.NumOfSubAdd, taskResponse.NumOfSubPhone, taskResponse.NumOfSubMail, taskResponse.NumOfSubType, taskResponse.NumOfCusPhone, taskResponse.NumOfCusEmail };
            List<object> numOfCompleteList = new List<object> { taskResponse.NumOfTitleComplete, taskResponse.NumOfCorComplete, taskResponse.NumOfIndivComplete, taskResponse.NumOfProdComplete, taskResponse.NumOfSubComplete, taskResponse.NumOfCountryComplete, taskResponse.NumOfProvinceComplete, taskResponse.NumOfDistrictComplete, taskResponse.NumOfSubDistrictComplete, taskResponse.NumOfPhonetypeComplete, taskResponse.NumOfEmailtypeComplete, taskResponse.NumOfAddressTypeComplete, taskResponse.NumOfAddressComplete, taskResponse.NumOfPhoneComplete, taskResponse.NumOfEmailComplete, taskResponse.NumOfSubTypeComplete, taskResponse.NumOfCusPhoneComplete, taskResponse.NumOfCusEmailComplete };
            List<object> numOfErrorList = new List<object> { taskResponse.NumOfTitleError, taskResponse.NumOfErrCor, taskResponse.NumOfErrIndiv, taskResponse.NumOfProdError, taskResponse.NumOfErrSub, taskResponse.NumOfCountryError, taskResponse.NumOfProvinceError, taskResponse.NumOfDistrictError, taskResponse.NumOfSubDistrictError, taskResponse.NumOfPhonetypeError, taskResponse.NumOfEmailtypeError, taskResponse.NumOfAddressTypeError, taskResponse.numOfErrAddress, taskResponse.NumOfErrPhone, taskResponse.NumOfErrEmail, taskResponse.NumOfSubTypeError, taskResponse.NumOfCusPhoneError, taskResponse.NumOfCusEmailError };

            StringBuilder sb = new StringBuilder();
            const string tmplCol = "<TD>{0}</TD>";

            if (fileList != null && fileList.Count > 0)
            {
                for (int i = 0; i <= fileList.Count - 1; i++)
                {
                    if (!string.IsNullOrWhiteSpace(fileList[i].ConvertToString()))
                    {
                        sb.Append("<TR>\n");
                        sb.AppendFormat(tmplCol, fileList[i]);
                        sb.AppendFormat(tmplCol, i < numOfTotalList.Count ? numOfTotalList[i] : "-");
                        sb.AppendFormat(tmplCol, i < numOfCompleteList.Count ? numOfCompleteList[i] : "-");
                        sb.AppendFormat(tmplCol, i < numOfErrorList.Count ? numOfErrorList[i] : "-");
                        sb.AppendFormat(tmplCol, i < fileErrorList.Count ? fileErrorList[i] : "-");
                        sb.Append("</TR>\n");
                    }
                }
            }
            else
            {
                sb.Append("<TR>\n");
                sb.AppendFormat("<TD colspan='5' class='text-center'>{0}</TD>", Resource.Msg_NoRecords);
                sb.Append("</TR>\n");
            }

            return sb.ToString();
        }

        private static string GetTemplatesDirectory()
        {
            return HostingEnvironment.MapPath(TmpFolder);
        }

        #endregion
    }
}
