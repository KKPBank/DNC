using CSM.Business.Interfaces;
using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Data.DataAccess;
using CSM.Data.DataAccess.Interfaces;
using CSM.Entity;
using CSM.Entity.Common;
using CSM.Service.Messages.DoNotCall;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static CSM.Common.Utilities.Constants;

namespace CSM.Business
{
    public class DoNotCallFacade : IDoNotCallFacade
    {
        private readonly DNCContext _dncContext;
        private readonly CSMContext _csmContext;
        private ICommonFacade _commonFacade;
        private IDoNotCallDataAccess _doNotCallDataAccess;
        private ICommonDataAccess _commonDataAccess;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public DoNotCallFacade()
        {
            _dncContext = new DNCContext();
            _csmContext = new CSMContext();
        }

        public bool TransactionExists(int id)
        {
            return _dncContext.TB_T_DNC_TRANSACTION.Any(x => x.DNC_TRANSACTION_ID == id);
        }

        public List<DoNotCallUpdatePhoneNoModel> GetDoNotCallPhoneNoListByExecuteTime(string executeTime)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);

            DoNotCallTimePeriodEntity period = _doNotCallDataAccess.GetExecuteTimePeriodEntity(executeTime);

            if (period == null) throw new NullReferenceException($"Invalid execute time \"{executeTime}\"");

            List<DoNotCallUpdatePhoneNoModel> result = _doNotCallDataAccess.GetDoNotCallPhoneNoListByPeriod(period.FromDateTime, period.ToDateTime);
            return result;
        }

        public DoNotCallTransactionInfo GetTelephoneTransactionById(int transactionId)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            DoNotCallTransactionInfo transaction = _doNotCallDataAccess.GetDoNotCallTransactionInfoById(transactionId);
            return transaction;
        }

        public DoNotCallTransactionInfo GetCustomerTransaction(string cardNo, string subscriptTypeCode)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            DoNotCallTransactionInfo transaction = _doNotCallDataAccess.GetCustomerTransaction(cardNo, subscriptTypeCode);
            return transaction;
        }

        public byte[] GenerateDoNotCallPhoneListAttachment(List<DoNotCallUpdatePhoneNoModel> dataList)
        {
            DateTime today = DateTime.Today;
            // Generate csv file
            return GenerateDoNotCallUpdateFileStream(dataList, today);
        }

        public void GenerateDoNotCallPhoneListFile(byte[] fileByte, string fileName, bool success)
        {
            _commonFacade = new CommonFacade();

            // Get path
            string folderPath = success ? _commonFacade.GetDoNotCallExportToTotSuccessFilePath() : _commonFacade.GetDoNotCallExportToTotErrorFilePath();
            string filePath = FileHelpers.GenerateFilePath(folderPath, $"{fileName}.csv");

            // Generate csv file
            File.WriteAllBytes(filePath, fileByte);
        }

        public void ExportDoNotCallUpdateFile(List<DoNotCallUpdatePhoneNoModel> dataList)
        {
            DateTime today = DateTime.Today;
            _commonFacade = new CommonFacade();

            // Get path
            string folderPath = _commonFacade.GetDoNotCallInterfaceExportPath();
            string dateStamp = today.FormatDateTime(DateTimeFormat.ExportDate);
            string fileName = $"DNC_{dateStamp}.csv";
            string filePath = FileHelpers.GenerateFilePath(folderPath, fileName);

            // Generate csv file
            GenerateDoNotCallUpdateFile(dataList, today, filePath);

            // Generate .end file
            string endFileName = $"DNC_{dateStamp}.end";
            string endFilePath = FileHelpers.GenerateFilePath(folderPath, endFileName);
            File.WriteAllText(endFilePath, "");
        }

        public List<DoNotCallUpdatePhoneNoModel> GetUpdatedDoNotCallPhoneNoList()
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            List<DoNotCallUpdatePhoneNoModel> result = _doNotCallDataAccess.GenerateUpdatedDoNotCallPhoneList();
            return result;
        }

        public DoNotCallFileUploadDetailModel GetViewUploadFileDetail(int id, Pager pager)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.GetFileUploadDetail(id, pager);
        }

        public List<DoNotCallFileUploadSearchResultModel> SearchDoNotCallUploadList(DoNotCallLoadListSearchFilter model)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.SearchDoNotCallUploadList(model);
        }

        public List<DoNotCallHistoryEntity> GetHistoryDoNotCallListByCustomerId(int customerId, Pager pager, int totalLimit)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            var list = _doNotCallDataAccess.GetDoNotCallHistoryList(customerId, pager, totalLimit);
            CalculateVersionNo(pager, list);
            return list;
        }

        public DoNotCallTransactionHistoryEntity GetDoNotCallHistoryDetail(int logId)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            DoNotCallTransactionHistoryEntity result = _doNotCallDataAccess.GetDoNotCallHistoryDetailFromId(logId);
            return result;
        }

        public DoNotCallByCustomerEntity GetDoNotCallCustomerModelById(int id)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.GetDoNotCallCustomerFromId(id);
        }

        public DoNotCallByTelephoneEntity GetDoNotCallByTelephoneEntity(int id)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.GetDoNotCallTelephoneFromTransactionId(id);
        }

        public List<DoNotCallTransactionModel> SearchDoNotCallCustomerTransactionList(string cardNo, int cardTypeId, Pager pager)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.SearchDoNotCallBlockByCustomerTransactionExact(cardNo, cardTypeId, pager);
        }

        public DoNotCallFileUploadResultModel SaveUploadFile(string fullFilePath, int userId, string userIP, string username, string uploadFileName, string contentType, int maxRowCount)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);

            var result = new DoNotCallFileUploadResultModel();
            var now = DateTime.Now;
            try
            {
                var fi = new FileInfo(fullFilePath);

                _commonDataAccess = new CommonDataAccess(_csmContext);
                using (var package = new ExcelPackage(fi))
                {
                    const string targetWorksheet = "Data";
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets[targetWorksheet];

                    if (worksheet == null)
                        throw new Exception($"Worksheet \"{targetWorksheet}\" not found");

                    var cells = worksheet.Cells;
                    var rows = worksheet.Dimension?.Rows;
                    if (!rows.HasValue)
                        throw new Exception($"Worksheet \"{targetWorksheet}\" has no data");
                    int rowCount = rows.Value;

                    // validate header
                    int headerRow = 1;
                    var templateHeaders = DNC.FileTemplateHeaders;

                    for (int i = headerRow; i < headerRow + templateHeaders.Count(); i++)
                    {
                        var cell = cells[headerRow, i];
                        string cellValue = cell.Value?.ToString()?.GetCleanString();
                        int headerIdex = i - 1;
                        string templateValue = templateHeaders[headerIdex];
                        if (!templateValue.Equals(cellValue, StringComparison.InvariantCultureIgnoreCase))
                        {
                            throw new Exception($"ชื่อ column ไม่ถูกต้อง (cell: {cell.Address})");
                        }
                    }

                    // validate data rows
                    int dataRowCount = rowCount - headerRow;
                    result.DataRowCount = dataRowCount;
                    if (dataRowCount > maxRowCount)
                    {
                        throw new Exception($"Too many records (cannot insert more than {maxRowCount} rows per file");
                    }
                    else if (dataRowCount == 0)
                    {
                        throw new Exception($"Worksheet \"{targetWorksheet}\" has 0 data row");
                    }

                    result.IsValidColumnHeaders = true;

                    // read data
                    int startRowNum = headerRow + 1;
                    int lastRowNum = headerRow + dataRowCount;

                    List<RowErrorResult> errors;
                    List<TB_T_DNC_TRANSACTION> mappedModels;
                    List<TB_T_DNC_LOAD_LIST_DATA> loadListData;
                    bool isValidData = ValidateFileUploadData(cells, startRowNum, lastRowNum, userId, username, userIP, now, out errors, out mappedModels, out loadListData);

                    if (isValidData)
                    {
                        var loadList = new TB_T_DNC_LOAD_LIST
                        {
                            FILE_NAME = uploadFileName,
                            CONTENT_TYPE = contentType,
                            DOCUMENT_NAME = uploadFileName,
                            FILE_URL = fullFilePath,
                            UPLOAD_DATE = now,
                            UPDATE_DATE = now,
                            CREATE_DATE = now,
                            CREATE_USER = userId,
                            UPDATE_USER = userId,
                            STATUS = DigitTrue,
                            TB_T_DNC_LOAD_LIST_DATA = loadListData
                        };

                        int itemCount = SaveValidatedFileUploadModelList(mappedModels, loadList);
                        result.Success = itemCount > 0;
                    }
                    else
                    {
                        result.ErrorResults = errors;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.SystemError = ex.Message;
                return result;
            }
        }

        public bool TryConvertNullableCellValueToBoolean(string input, string trueStr, string falseStr, bool defaultValIfNull, out bool output)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                output = defaultValIfNull;
                return true;
            }

            string cleanInput = input.GetCleanString();
            bool isTrue = trueStr.Equals(cleanInput);
            bool isFalse = falseStr.Equals(cleanInput);
            if (!isTrue && !isFalse)
            {
                output = false;
                return false;
            }

            output = isTrue;
            return true;
        }

        public bool TryConvertCellValueToBoolean(string input, string trueStr, string falseStr, out bool output, bool nullable = true, bool defaultValue = true)
        {
            output = defaultValue;
            if (string.IsNullOrWhiteSpace(input))
            {
                return nullable;
            }

            string cleanInput = input.GetCleanString();
            bool isTrue = trueStr.Equals(cleanInput);
            bool isFalse = falseStr.Equals(cleanInput);
            if (!isTrue && !isFalse)
            {
                return false;
            }

            output = isTrue;
            return true;
        }

        public DoNotCallTransactionEntity FindDoNotCallTransactionById(int transactionId)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.FindDoNotCallTransactionById(transactionId);
        }

        public bool IsExistingCustomerTransactionCardInfo(string cardNo, int subscriptionTypeId, int transactionId)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.IsExistingCustomerTransactionCard(cardNo, subscriptionTypeId, transactionId);
        }

        public List<SubscriptTypeEntity> GetSubscriptTypeSelectList()
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.GetActiveSubscriptionTypes();
        }

        public List<DoNotCallTransactionModel> SearchDoNotCallTelephoneContact(string email, string phoneNo, Pager pager)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.SearchDoNotCallBlockByTelephoneContact(phoneNo, email, pager);
        }

        public int SavePhone(DoNotCallByTelephoneEntity model)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            if (model.BasicInfo.TransactionId > 0)
                return _doNotCallDataAccess.UpdateDoNotCallTelephone(model);
            else
                return _doNotCallDataAccess.InsertDoNotCallTelephone(model);
        }

        public int SaveCustomer(DoNotCallByCustomerEntity model)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            if (model.BasicInfo.TransactionId > 0)
                return _doNotCallDataAccess.UpdateDoNotCallCustomer(model);
            else
                return _doNotCallDataAccess.InsertDoNotCallCustomer(model);
        }

        public List<TransactionInfo> SearchExactDoNotCallTransaction(DoNotCallListSearchFilter searchFilter)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.SearchExactDoNotCallTransaction(searchFilter) ?? new List<TransactionInfo>();
        }

        public List<DoNotCallEntity> SearchDoNotCallList(DoNotCallListSearchFilter searchFilter)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.SearchDoNotCallList(searchFilter) ?? new List<DoNotCallEntity>();
        }

        public DoNotCallByCustomerEntity GetDoNotCallCustomerModelByCardId(string cardId)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            return _doNotCallDataAccess.GetDoNotCallCustomerFromCardId(cardId);
        }

        public byte[] GenerateDoNotCallListExcelFile(DoNotCallListSearchFilter searchFilter)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            List<DoNotCallExcelModel> data = _doNotCallDataAccess.SearchDoNotCallListExcelModel(searchFilter);
            if (data.Count > 0)
            {
                return ExcelHelpers.WriteToExcel(data);
            }
            return null;
        }

        #region Private
        private byte[] GenerateDoNotCallUpdateFileStream(List<DoNotCallUpdatePhoneNoModel> dataList, DateTime today)
        {
            using (var memoryStream = new MemoryStream())
            {
                StringBuilder sb = new StringBuilder();
                string[] headers = new string[]
                {
                    BatchProcess.DataTypeHeader,
                    today.FormatDateTime(DateTimeFormat.StoreProcedureDate),
                    dataList.Count.FormatNumber()
                };

                string headerLine = string.Join(BatchProcess.ColumnSeparator, headers);
                sb.AppendLine(headerLine);

                foreach (var item in dataList)
                {
                    string[] row = new string[]
                    {
                        BatchProcess.DataTypeData,
                        item.PhoneNo,
                        item.Status
                    };
                    string detailLine = string.Join(BatchProcess.ColumnSeparator, row);
                    sb.AppendLine(detailLine);
                }

                byte[] contentAsBytes = Encoding.UTF8.GetBytes(sb.ToString());
                memoryStream.Write(contentAsBytes, 0, contentAsBytes.Length);

                // Set the position to the beginning of the stream.
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }
        }

        private void GenerateDoNotCallUpdateFile(List<DoNotCallUpdatePhoneNoModel> newDataList, DateTime today, string filePath)
        {
            var enCoding = Encoding.GetEncoding("windows-874");
            using (var sw = new StreamWriter(filePath, false, enCoding))
            {
                string[] headers = new string[]
                {
                    BatchProcess.DataTypeHeader,
                    today.FormatDateTime(DateTimeFormat.StoreProcedureDate),
                    newDataList.Count.FormatNumber()
                };

                string headerLine = string.Join(BatchProcess.ColumnSeparator, headers);
                sw.WriteLine(headerLine);

                foreach (var item in newDataList)
                {
                    string[] row = new string[]
                    {
                        BatchProcess.DataTypeData,
                        item.PhoneNo,
                        item.Status
                    };
                    string detailLine = string.Join(BatchProcess.ColumnSeparator, row);
                    sw.WriteLine(detailLine);
                }
            }
        }

        private int SaveValidatedFileUploadModelList(List<TB_T_DNC_TRANSACTION> models, TB_T_DNC_LOAD_LIST loadList)
        {
            return _doNotCallDataAccess.SaveFileUploadModels(models, loadList);
        }

        private bool ValidateFileUploadData(ExcelRange cells, int startRowNum, int lastRowNum, int userId, string username, string userIP, DateTime now,
            out List<RowErrorResult> errors,
            out List<TB_T_DNC_TRANSACTION> resultModels,
            out List<TB_T_DNC_LOAD_LIST_DATA> loadListData)
        {
            int errorRowCount = 0;
            errors = new List<RowErrorResult>();
            resultModels = new List<TB_T_DNC_TRANSACTION>();
            loadListData = new List<TB_T_DNC_LOAD_LIST_DATA>();
            List<DoNotCallProductModel> currentProducts = _doNotCallDataAccess.GetAllProductsForFileUploadCompare();
            _commonDataAccess = new CommonDataAccess(_csmContext);
            List<SubscriptTypeEntity> subscriptionTypeList = _commonDataAccess.GetActiveSubscriptType();

            for (int rowNum = startRowNum; rowNum <= lastRowNum; rowNum++)
            {
                var rowError = new RowErrorResult();
                var errorDict = new Dictionary<string, string>();

                // validate Transaction type
                string transactionType = cells[DNC.FileColumnLetter.TransactionType + rowNum].Value?.ToString()?.GetCleanString();
                bool isTypeCustomer;
                bool canConvertTransactionType = TryConvertCellValueToBoolean(transactionType, DNC.Customer, DNC.Telephone, out isTypeCustomer, nullable: false);
                if (!canConvertTransactionType)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.TransactionType, "กรุณากรอกข้อมูลเป็น Customer หรือ Telephone เท่านั้น");
                }
                // Card No
                string cardNo = cells[DNC.FileColumnLetter.CardNo + rowNum].Value?.ToString();
                bool hasCardNo = !string.IsNullOrWhiteSpace(cardNo);
                if (canConvertTransactionType && isTypeCustomer && !hasCardNo)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.CardNo, "หากระบุเป็น Customer กรุณากรอกข้อมูล เลขบัตรประชาชน/นิติบุคคล และ ชื่อลูกค้า เป็นอย่างน้อย");
                }
                // Subscription Type
                bool requireSubscriptionType = isTypeCustomer || hasCardNo;
                string subscriptionTypeName = cells[DNC.FileColumnLetter.SubscirptionType + rowNum].Value?.ToString().GetCleanString(toLower: false);
                SubscriptTypeEntity subscriptType = subscriptionTypeList.FirstOrDefault(x => x.SubscriptTypeName.Trim().Equals(subscriptionTypeName, StringComparison.CurrentCultureIgnoreCase));
                if (string.IsNullOrWhiteSpace(subscriptionTypeName)) // has no input
                {
                    if (requireSubscriptionType)
                        AddRowError(errorDict, DNC.FileColumnLetter.SubscirptionType, "กรุณาระบุประเภทบัตร");
                }
                else if (subscriptType == null) // input has value but not found in database
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.SubscirptionType, "กรุณากรอกข้อมูล ประเภทบัตร ให้ถูกต้อง ตามข้อมูล Master ของธนาคาร");
                }
                else if (!hasCardNo) // input has value, but no card no
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.CardNo, "กรุณาระบุเลขบัตร");
                }
                else if (subscriptType.SubscriptTypeCode == SubscriptTypeCode.Personal && !ApplicationHelpers.ValidateCardNo(cardNo.GetCleanString()))
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.CardNo, "กรุณากรอกข้อมูล เลขบัตรประชาชน ให้ถูกต้อง");
                }
                // Customer name
                string firstName = cells[DNC.FileColumnLetter.FirstName + rowNum].Value?.ToString()?.GetCleanString();
                if (string.IsNullOrWhiteSpace(firstName))
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.FirstName, "กรุณากรอกข้อมูล ชื่อลูกค้า เป็นอย่างน้อย");
                }
                else if (firstName.Length > MaxLength.FirstName)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.FirstName, string.Format(Resource.ValErr_StringLength, "First Name", MaxLength.FirstName.ToString()));
                }

                // last name
                string lastName = cells[$"{DNC.FileColumnLetter.LastName}{rowNum}"].Value?.ToString()?.GetCleanString();
                var lastNameAttr = new LocalizedRegexAttribute(RegexFormat.EngChar, Resource.ValErr_NoSpecialCharacterEnglish);
                bool hasLastName = !string.IsNullOrWhiteSpace(lastName);
                if (hasLastName && lastName.Length > MaxLength.LastName)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.LastName, string.Format(Resource.ValErr_StringLength, "Last Name", MaxLength.LastName.ToString()));
                }

                // Telephone
                string phoneNo = cells[DNC.FileColumnLetter.PhoneNo + rowNum].Value?.ToString();
                bool hasPhoneNo = !string.IsNullOrWhiteSpace(phoneNo);
                if (hasPhoneNo)
                {
                    // validate format
                    string cleanPhoneNo = phoneNo.GetCleanString();
                    int phoneNoLength = cleanPhoneNo.Length;
                    int maxPhoneLength = MaxLength.DoNotCallPhoneNo;
                    int minPhoneLength = MinLenght.PhoneNo;

                    if (phoneNoLength < minPhoneLength || phoneNoLength > maxPhoneLength || cleanPhoneNo.Any(c => !char.IsDigit(c)))
                    {
                        AddRowError(errorDict, DNC.FileColumnLetter.PhoneNo, $"Telephone ต้องระบุเป็นตัวเลข {minPhoneLength}-{maxPhoneLength} หลักเท่านั้น");
                    }
                }

                // Email 
                string email = cells[DNC.FileColumnLetter.Email + rowNum].Value?.ToString();
                bool hasEmail = !string.IsNullOrWhiteSpace(email);
                if (hasEmail)
                {
                    var emailChecker = new EmailAddressAttribute();
                    bool isValid = emailChecker.IsValid(email.GetCleanString());
                    if (!isValid)
                    {
                        AddRowError(errorDict, DNC.FileColumnLetter.Email, "รูปแบบ email ไม่ถูกต้อง");
                    }
                    else if (email.Length > MaxLength.Email)
                    {
                        AddRowError(errorDict, DNC.FileColumnLetter.Email, $"ความยาว Email ต้องไม่เกิน {MaxLength.Email} ตัวอักษร");
                    }
                }

                // Expire date
                DateTime expireDate = DateTime.Today.AddYears(DNC.DefaultExpireDateAddYear);
                string expireDateStr = cells[DNC.FileColumnLetter.ExpireDate + rowNum].Value?.ToString();
                bool hasExpireDate = !string.IsNullOrWhiteSpace(expireDateStr);
                if (hasExpireDate)
                {
                    DateTime parsedDate;
                    bool isValidExpireDate = DateTime.TryParseExact(expireDateStr, DateTimeFormat.DefaultShortDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
                    if (!isValidExpireDate)
                    {
                        AddRowError(errorDict, DNC.FileColumnLetter.ExpireDate, "ข้อมูลไม่ถูกต้อง วันที่ต้องกรอกเป็น DD/MM/YYYY (เป็นปี ค.ศ.)");
                    }
                    else if (parsedDate.Date < DateTime.Today.Date)
                    {
                        AddRowError(errorDict, DNC.FileColumnLetter.ExpireDate, "วันที่ต้องเป็นวันปัจจุบันหรือวันในอนาคตเท่านั้น");
                    }
                    else
                    {
                        expireDate = parsedDate;
                    }
                }

                // DoNotCall Status
                string status = cells[DNC.FileColumnLetter.DoNotCallStatus + rowNum].Value?.ToString()?.GetCleanString();
                bool isActive;
                bool canConvertStatus = TryConvertNullableCellValueToBoolean(status, DNC.Active, DNC.Inactive, true, out isActive);
                if (!canConvertStatus)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.DoNotCallStatus, "ข้อมูลต้องเป็น Active หรือ Inactive เท่านั้น");
                }

                string blockErrorMessage = $"ข้อมูลต้องเป็น {Yes} หรือ {No} เท่านั้น";
                string emailRequired = "หากต้องการ Block Email ทั้ง Sales, Information กรุณากรอกข้อมูล Email";
                string phoneNoRequired = "หากต้องการ Block Telephone, SMS ทั้ง Sales, Information กรุณากรอกข้อมูล เบอร์โทรศัพท์";

                bool hasAtLeastOneBlock = false;

                // Sales
                string sBlockTel = cells[DNC.FileColumnLetter.SalesBlockTelephone + rowNum].Value?.ToString().GetCleanString();
                bool isBlockSalesTel;
                bool canConvertSalesBlockTel = TryConvertCellValueToBoolean(sBlockTel, yes, no, out isBlockSalesTel);
                if (!canConvertSalesBlockTel)
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockTelephone, blockErrorMessage);
                else if (isBlockSalesTel && !hasPhoneNo)
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockTelephone, phoneNoRequired);
                else
                    hasAtLeastOneBlock = hasAtLeastOneBlock || isBlockSalesTel;

                string sBlockSms = cells[DNC.FileColumnLetter.SalesBlockSMS + rowNum].Value?.ToString().GetCleanString();
                bool isBlockSalesSms;
                bool canConvertSalesBlockSms = TryConvertCellValueToBoolean(sBlockSms, yes, no, out isBlockSalesSms);
                if (!canConvertSalesBlockSms)
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockSMS, blockErrorMessage);
                else if (isBlockSalesSms && !hasPhoneNo)
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockSMS, phoneNoRequired);
                else
                    hasAtLeastOneBlock = hasAtLeastOneBlock || isBlockSalesSms;

                string sBlockEmail = cells[DNC.FileColumnLetter.SalesBlockEmail + rowNum].Value?.ToString().GetCleanString();
                bool isBlockSalesEmail;
                bool canConvertSalesBlockEmail = TryConvertCellValueToBoolean(sBlockEmail, yes, no, out isBlockSalesEmail);
                if (!canConvertSalesBlockEmail)
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockEmail, blockErrorMessage);
                else if (isBlockSalesEmail && !hasEmail)
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockEmail, emailRequired);
                else
                    hasAtLeastOneBlock = hasAtLeastOneBlock || isBlockSalesEmail;

                // Information
                string iBlockTel = cells[DNC.FileColumnLetter.InformationBlockTelephone + rowNum].Value?.ToString().GetCleanString();
                bool isBlockInfoTel;
                bool canConvertInfoBlockTel = TryConvertCellValueToBoolean(iBlockTel, yes, no, out isBlockInfoTel);
                if (!canConvertInfoBlockTel)
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockTelephone, blockErrorMessage);
                else if (isBlockInfoTel && !hasPhoneNo)
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockTelephone, phoneNoRequired);
                else
                    hasAtLeastOneBlock = hasAtLeastOneBlock || isBlockInfoTel;

                string iBlockSms = cells[DNC.FileColumnLetter.InformationBlockSMS + rowNum].Value?.ToString().GetCleanString();
                bool isBlockInfoSms;
                bool canConvertInfoBlockSms = TryConvertCellValueToBoolean(iBlockSms, yes, no, out isBlockInfoSms);
                if (!canConvertInfoBlockSms)
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockSMS, blockErrorMessage);
                else if (isBlockInfoSms && !hasPhoneNo)
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockSMS, phoneNoRequired);
                else
                    hasAtLeastOneBlock = hasAtLeastOneBlock || isBlockInfoSms;

                string iBlockEmail = cells[DNC.FileColumnLetter.InformationBlockEmail + rowNum].Value?.ToString().GetCleanString();
                bool isBlockInfoEmail;
                bool canConvertInfoBlockEmail = TryConvertCellValueToBoolean(iBlockEmail, yes, no, out isBlockInfoEmail);
                if (!canConvertInfoBlockEmail)
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockEmail, blockErrorMessage);
                else if (isBlockInfoEmail && !hasEmail)
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockEmail, emailRequired);
                else
                    hasAtLeastOneBlock = hasAtLeastOneBlock || isBlockInfoEmail;

                // Products
                string sBlockAllProducts = cells[DNC.FileColumnLetter.SalesAllProduct + rowNum].Value?.ToString().GetCleanString();
                bool isBlockSalesAllProducts;
                bool canConvertSalesBlockAllProducts = TryConvertCellValueToBoolean(sBlockAllProducts, yes, no, out isBlockSalesAllProducts);

                string iBlockAllProducts = cells[DNC.FileColumnLetter.InformationAllProduct + rowNum].Value?.ToString().GetCleanString();
                bool isBlockInfoAllProducts;
                bool canConvertInfoBlockAllProducts = TryConvertCellValueToBoolean(iBlockAllProducts, yes, no, out isBlockInfoAllProducts);

                string[] splitters = new string[] { DNC.ProductStringSplitter };
                string salesProducts = cells[DNC.FileColumnLetter.SalesProduct + rowNum].Value?.ToString();
                string infoProducts = cells[DNC.FileColumnLetter.InformationProduct + rowNum].Value?.ToString();

                string[] salesProductNames = salesProducts?.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                string[] infoProductNames = infoProducts?.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

                string productMustBeEmpty = "ต้องระบุ product เป็นค่าว่าง";

                bool hasSalesProducts = salesProductNames?.Length > 0;

                List<int> salesProductIds = new List<int>();
                if (!canConvertSalesBlockAllProducts)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesAllProduct, blockErrorMessage);
                }
                else if (isBlockSalesAllProducts && hasSalesProducts) // block all, but insert products
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesAllProduct, productMustBeEmpty);
                }
                else if (hasSalesProducts)
                {
                    ValidateProducts(currentProducts, errorDict, salesProductNames, salesProductIds, DNC.FileColumnLetter.SalesProduct);
                }

                List<int> infoProductIds = new List<int>();
                bool hasInfoProducts = infoProductNames?.Length > 0;
                if (!canConvertInfoBlockAllProducts)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationAllProduct, blockErrorMessage);
                }
                else if (isBlockInfoAllProducts && hasInfoProducts)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationProduct, productMustBeEmpty);
                }
                else if (hasInfoProducts)
                {
                    ValidateProducts(currentProducts, errorDict, infoProductNames, infoProductIds, DNC.FileColumnLetter.InformationProduct);
                }

                if (!hasAtLeastOneBlock)
                {
                    AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockTelephone, $"ต้องเลือก block เป็น {Yes} อย่างน้อย 1 รายการ");
                    AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockTelephone, $"ต้องเลือก block เป็น {Yes} อย่างน้อย 1 รายการ");
                }

                // Map model
                if (errorDict.Count == 0)
                {
                    string fromSystem = cells[$"{DNC.FileColumnLetter.System}{rowNum}"].Value?.ToString()?.Trim() ?? SystemName.DoNotCall;
                    string remark = cells[$"{DNC.FileColumnLetter.Remark}{rowNum}"].Value?.ToString();
                    string statusStr = isActive ? DigitTrue : DigitFalse;
                    string transactionTypeDigit = isTypeCustomer ? DNC.TransactionTypeCustomer : DNC.TransactionTypeTelephone;
                    string infoBlockAll = isBlockInfoAllProducts ? DigitTrue : DigitFalse;
                    string infoBlockEmail = isBlockInfoEmail ? DigitTrue : DigitFalse;
                    string infoBlockSMS = isBlockInfoSms ? DigitTrue : DigitFalse;
                    string infoBlockTel = isBlockInfoTel ? DigitTrue : DigitFalse;
                    string salesBlockAll = isBlockSalesAllProducts ? DigitTrue : DigitFalse;
                    string salesBlockEmail = isBlockSalesEmail ? DigitTrue : DigitFalse;
                    string salesBlockSMS = isBlockSalesSms ? DigitTrue : DigitFalse;
                    string salesBlockTel = isBlockSalesTel ? DigitTrue : DigitFalse;
                    int? subscriptionTypeId = subscriptType != null ? subscriptType.SubscriptTypeId : null;

                    loadListData.Add(new TB_T_DNC_LOAD_LIST_DATA
                    {
                        ROW_NO = rowNum,
                        PHONE_NO = phoneNo,
                        CARD_NO = cardNo,
                        SUBSCRIPT_TYPE_ID = subscriptionTypeId,
                        FIRST_NAME = firstName,
                        LAST_NAME = lastName,
                        EXPIRY_DATE = expireDate,
                        SALES_BLOCK_TELEPHONE = salesBlockTel,
                        SALES_BLOCK_SMS = salesBlockSMS,
                        SALES_BLOCK_EMAIL = salesBlockEmail,
                        SALES_BLOCK_ALL_PRODUCT = salesBlockAll,
                        INFORMATION_BLOCK_TELEPHONE = infoBlockTel,
                        INFORMATION_BLOCK_SMS = infoBlockSMS,
                        INFORMATION_BLOCK_EMAIL = infoBlockEmail,
                        INFORMATION_BLOCK_ALL_PRODUCT = infoBlockAll,
                        STATUS = statusStr,
                        TRANS_TYPE = transactionTypeDigit,
                        CREATE_DATE = now,
                        CREATE_USER = userId,
                        UPDATE_DATE = now,
                        UPDATE_USER = userId,
                        EMAIL = email
                    });

                    var existingCustomer = isTypeCustomer ? resultModels.Where(x => x.TRANS_TYPE == DNC.TransactionTypeCustomer).SingleOrDefault(x => x.CARD_NO == cardNo) : null;
                    if (existingCustomer != null)
                    {
                        string cardNoMsg = $"(Card No: {existingCustomer.CARD_NO})";
                        string errorMsg = $"Conflict Customer Data {cardNoMsg}";

                        if (existingCustomer.FIRST_NAME != firstName)
                            AddRowError(errorDict, DNC.FileColumnLetter.FirstName, errorMsg);
                        if (existingCustomer.LAST_NAME != lastName)
                            AddRowError(errorDict, DNC.FileColumnLetter.LastName, errorMsg);
                        if (existingCustomer.EXPIRY_DATE != expireDate)
                            AddRowError(errorDict, DNC.FileColumnLetter.ExpireDate, errorMsg);
                        if (existingCustomer.STATUS != statusStr)
                            AddRowError(errorDict, DNC.FileColumnLetter.DoNotCallStatus, errorMsg);
                        if (existingCustomer.REMARK != remark)
                            AddRowError(errorDict, DNC.FileColumnLetter.Remark, errorMsg);
                        if (existingCustomer.FROM_SYSTEM != fromSystem)
                            AddRowError(errorDict, DNC.FileColumnLetter.System, errorMsg);

                        var currentactivityType = existingCustomer.TB_T_DNC_ACTIVITY_TYPE.First();
                        if (currentactivityType.INFORMATION_BLOCK_ALL_PRODUCT != infoBlockAll)
                            AddRowError(errorDict, DNC.FileColumnLetter.InformationAllProduct, errorMsg);
                        if (currentactivityType.INFORMATION_BLOCK_EMAIL != infoBlockEmail)
                            AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockEmail, errorMsg);
                        if (currentactivityType.INFORMATION_BLOCK_SMS != infoBlockSMS)
                            AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockSMS, errorMsg);
                        if (currentactivityType.INFORMATION_BLOCK_TELEPHONE != infoBlockTel)
                            AddRowError(errorDict, DNC.FileColumnLetter.InformationBlockTelephone, errorMsg);
                        if (currentactivityType.SALES_BLOCK_ALL_PRODUCT != salesBlockAll)
                            AddRowError(errorDict, DNC.FileColumnLetter.SalesAllProduct, errorMsg);
                        if (currentactivityType.SALES_BLOCK_EMAIL != salesBlockEmail)
                            AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockEmail, errorMsg);
                        if (currentactivityType.SALES_BLOCK_SMS != salesBlockSMS)
                            AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockSMS, errorMsg);
                        if (currentactivityType.SALES_BLOCK_TELEPHONE != salesBlockTel)
                            AddRowError(errorDict, DNC.FileColumnLetter.SalesBlockTelephone, errorMsg);

                        if (hasEmail && existingCustomer.TB_T_DNC_EMAIL.Any(x => x.EMAIL == email))
                            AddRowError(errorDict, DNC.FileColumnLetter.Email, $"Duplicated Customer Email {cardNoMsg}");
                        if (hasPhoneNo && existingCustomer.TB_T_DNC_PHONE_NO.Any(x => x.PHONE_NO == phoneNo))
                            AddRowError(errorDict, DNC.FileColumnLetter.PhoneNo, $"Duplicated Customer Phone No {cardNoMsg}");

                        var currentInfoProducts = currentactivityType.TB_T_DNC_ACTIVITY_PRODUCT.Where(x => x.TYPE == ActivityProductTypeInformation);
                        int infoProductCount = currentInfoProducts.Count();
                        var currentSalesProducts = currentactivityType.TB_T_DNC_ACTIVITY_PRODUCT.Where(x => x.TYPE == ActivityProductTypeSales);
                        int salesProductCount = currentInfoProducts.Count();

                        if (hasInfoProducts && infoProductIds.Count != infoProductCount && currentInfoProducts.Any(x => !infoProductIds.Contains(x.PRODUCT_ID)))
                            AddRowError(errorDict, DNC.FileColumnLetter.PhoneNo, $"Mismatch Customer Information Products {cardNoMsg}");
                        if (hasSalesProducts && salesProductIds.Count != salesProductCount && currentSalesProducts.Any(x => !salesProductIds.Contains(x.PRODUCT_ID)))
                            AddRowError(errorDict, DNC.FileColumnLetter.PhoneNo, $"Mismatch Customer Sales Products {cardNoMsg}");

                        if (errorDict.Count == 0)
                        {
                            if (hasPhoneNo)
                            {
                                var item = new TB_T_DNC_PHONE_NO
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    CREATE_USERNAME = username,
                                    UPDATE_DATE = now,
                                    UPDATE_USER = userId,
                                    UPDATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    PHONE_NO = phoneNo
                                };

                                var itemHst = new TB_T_DNC_TRANSACTION_HIS_PHONE
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    PHONE_NO = phoneNo,
                                    CREATE_USERNAME = username
                                };

                                existingCustomer.TB_T_DNC_PHONE_NO.Add(item);
                                existingCustomer.TB_T_DNC_TRANSACTION_HIS.First().TB_T_DNC_TRANSACTION_HIS_PHONE.Add(itemHst);
                            }

                            if (hasEmail)
                            {
                                var item = new TB_T_DNC_EMAIL
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    UPDATE_DATE = now,
                                    UPDATE_USER = userId,
                                    UPDATE_USERNAME = username,
                                    CREATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    EMAIL = email
                                };

                                var itemHst = new TB_T_DNC_TRANSACTION_HIS_EMAIL
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    CREATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    EMAIL = email
                                };

                                existingCustomer.TB_T_DNC_EMAIL.Add(item);
                                existingCustomer.TB_T_DNC_TRANSACTION_HIS.First().TB_T_DNC_TRANSACTION_HIS_EMAIL.Add(itemHst);
                            }
                        }
                    }
                    else
                    {
                        var transaction = new TB_T_DNC_TRANSACTION
                        {
                            TRANS_TYPE = transactionTypeDigit,
                            CARD_NO = cardNo,
                            CREATE_DATE = now, // will be replaced for existing customer
                            CREATE_USER = userId, // will be replaced for existing customer
                            CREATE_USERNAME = username,
                            UPDATE_USERNAME = username,
                            UPDATE_DATE = now,
                            UPDATE_USER = userId,
                            DNC_TRANSACTION_ID = 0,
                            EFFECTIVE_DATE = now.Date,
                            EXPIRY_DATE = expireDate,
                            FIRST_NAME = firstName,
                            LAST_NAME = lastName,
                            FROM_SYSTEM = fromSystem,
                            REMARK = remark,
                            STATUS = statusStr,
                            SUBSCRIPT_TYPE_ID = subscriptionTypeId,
                            TB_T_DNC_ACTIVITY_TYPE = new List<TB_T_DNC_ACTIVITY_TYPE>(),
                            TB_T_DNC_PHONE_NO = new List<TB_T_DNC_PHONE_NO>(),
                            TB_T_DNC_EMAIL = new List<TB_T_DNC_EMAIL>(),
                            TB_T_DNC_TRANSACTION_HIS = new List<TB_T_DNC_TRANSACTION_HIS>()
                        };

                        var activityType = new TB_T_DNC_ACTIVITY_TYPE
                        {
                            CREATE_DATE = now,
                            CREATE_USER = userId,
                            UPDATE_USERNAME = username,
                            UPDATE_DATE = now,
                            UPDATE_USER = userId,
                            CREATE_USERNAME = username,
                            INFORMATION_BLOCK_ALL_PRODUCT = infoBlockAll,
                            INFORMATION_BLOCK_EMAIL = infoBlockEmail,
                            INFORMATION_BLOCK_SMS = infoBlockSMS,
                            INFORMATION_BLOCK_TELEPHONE = infoBlockTel,
                            SALES_BLOCK_ALL_PRODUCT = salesBlockAll,
                            SALES_BLOCK_EMAIL = salesBlockEmail,
                            SALES_BLOCK_SMS = salesBlockSMS,
                            SALES_BLOCK_TELEPHONE = salesBlockTel,
                            TB_T_DNC_ACTIVITY_PRODUCT = new List<TB_T_DNC_ACTIVITY_PRODUCT>()
                        };

                        var transHis = new TB_T_DNC_TRANSACTION_HIS
                        {
                            TRANS_TYPE = transactionTypeDigit,
                            CARD_NO = cardNo,
                            CREATE_DATE = now,
                            CREATE_USER = userId,
                            CREATE_USERNAME = username,
                            DNC_TRANSACTION_ID = 0,
                            EFFECTIVE_DATE = now.Date,
                            EXPIRY_DATE = expireDate,
                            FIRST_NAME = firstName,
                            LAST_NAME = lastName,
                            FROM_SYSTEM = fromSystem,
                            REMARK = remark,
                            STATUS = statusStr,
                            SUBSCRIPT_TYPE_ID = subscriptionTypeId,
                            INFORMATION_BLOCK_ALL_PRODUCT = infoBlockAll,
                            INFORMATION_BLOCK_EMAIL = infoBlockEmail,
                            INFORMATION_BLOCK_SMS = infoBlockSMS,
                            INFORMATION_BLOCK_TELEPHONE = infoBlockTel,
                            SALES_BLOCK_ALL_PRODUCT = salesBlockAll,
                            SALES_BLOCK_EMAIL = salesBlockEmail,
                            SALES_BLOCK_SMS = salesBlockSMS,
                            SALES_BLOCK_TELEPHONE = salesBlockTel,
                            TB_T_DNC_TRANSACTION_HIS_EMAIL = new List<TB_T_DNC_TRANSACTION_HIS_EMAIL>(),
                            TB_T_DNC_TRANSACTION_HIS_PHONE = new List<TB_T_DNC_TRANSACTION_HIS_PHONE>(),
                            TB_T_DNC_TRANSACTION_HIS_PRODUCT = new List<TB_T_DNC_TRANSACTION_HIS_PRODUCT>()
                        };

                        if (hasPhoneNo)
                        {
                            var item = new TB_T_DNC_PHONE_NO
                            {
                                CREATE_DATE = now,
                                CREATE_USER = userId,
                                CREATE_USERNAME = username,
                                UPDATE_DATE = now,
                                UPDATE_USER = userId,
                                UPDATE_USERNAME = username,
                                DELETE_STATUS = DeleteFlagFalse,
                                PHONE_NO = phoneNo
                            };

                            var itemHst = new TB_T_DNC_TRANSACTION_HIS_PHONE
                            {
                                CREATE_DATE = now,
                                CREATE_USER = userId,
                                CREATE_USERNAME = username,
                                DELETE_STATUS = DeleteFlagFalse,
                                PHONE_NO = phoneNo,
                            };

                            transHis.TB_T_DNC_TRANSACTION_HIS_PHONE.Add(itemHst);
                            transaction.TB_T_DNC_PHONE_NO.Add(item);
                        }

                        if (hasEmail)
                        {
                            var item = new TB_T_DNC_EMAIL
                            {
                                CREATE_DATE = now,
                                CREATE_USER = userId,
                                CREATE_USERNAME = username,
                                UPDATE_DATE = now,
                                UPDATE_USER = userId,
                                UPDATE_USERNAME = username,
                                DELETE_STATUS = DeleteFlagFalse,
                                EMAIL = email
                            };

                            var itemHst = new TB_T_DNC_TRANSACTION_HIS_EMAIL
                            {
                                CREATE_DATE = now,
                                CREATE_USER = userId,
                                CREATE_USERNAME = username,
                                DELETE_STATUS = DeleteFlagFalse,
                                EMAIL = email
                            };

                            transHis.TB_T_DNC_TRANSACTION_HIS_EMAIL.Add(itemHst);
                            transaction.TB_T_DNC_EMAIL.Add(item);
                        }

                        if (hasSalesProducts)
                        {
                            foreach (var id in salesProductIds)
                            {
                                var product = new TB_T_DNC_ACTIVITY_PRODUCT
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    CREATE_USERNAME = username,
                                    UPDATE_DATE = now,
                                    UPDATE_USER = userId,
                                    UPDATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    TYPE = ActivityProductTypeSales,
                                    PRODUCT_ID = id
                                };

                                var itemHst = new TB_T_DNC_TRANSACTION_HIS_PRODUCT
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    CREATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    TYPE = ActivityProductTypeSales,
                                    PRODUCT_ID = id
                                };

                                transHis.TB_T_DNC_TRANSACTION_HIS_PRODUCT.Add(itemHst);
                                activityType.TB_T_DNC_ACTIVITY_PRODUCT.Add(product);
                            }
                        }

                        if (hasInfoProducts)
                        {
                            foreach (var id in infoProductIds)
                            {
                                var product = new TB_T_DNC_ACTIVITY_PRODUCT
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    CREATE_USERNAME = username,
                                    UPDATE_DATE = now,
                                    UPDATE_USER = userId,
                                    UPDATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    TYPE = ActivityProductTypeInformation,
                                    PRODUCT_ID = id
                                };

                                var itemHst = new TB_T_DNC_TRANSACTION_HIS_PRODUCT
                                {
                                    CREATE_DATE = now,
                                    CREATE_USER = userId,
                                    CREATE_USERNAME = username,
                                    DELETE_STATUS = DeleteFlagFalse,
                                    TYPE = ActivityProductTypeInformation,
                                    PRODUCT_ID = id
                                };

                                transHis.TB_T_DNC_TRANSACTION_HIS_PRODUCT.Add(itemHst);
                                activityType.TB_T_DNC_ACTIVITY_PRODUCT.Add(product);
                            }
                        }

                        transaction.TB_T_DNC_ACTIVITY_TYPE.Add(activityType);
                        transaction.TB_T_DNC_TRANSACTION_HIS.Add(transHis);

                        resultModels.Add(transaction);

                    }
                }

                // Summary
                if (errorDict.Count > 0)
                {
                    errorRowCount++;
                    if (errorRowCount > DNC.MaxErrorRowCount)
                        return false;

                    rowError.RowNum = rowNum;
                    rowError.CellErrorDict = errorDict;
                    errors.Add(rowError);
                }
            }

            return errorRowCount == 0;
        }

        private void ValidateProducts(List<DoNotCallProductModel> currentProducts, Dictionary<string, string> errorDict, string[] infoProductNames, List<int> infoProductIds, string colLetter)
        {
            List<string> invalidNames = new List<string>();
            List<string> duplicatedProducts = new List<string>();
            foreach (string productName in infoProductNames)
            {
                var product = currentProducts.FirstOrDefault(x => x.Name.Equals(productName, StringComparison.InvariantCultureIgnoreCase));
                if (product != null)
                {
                    int productId = product.Id;
                    if (infoProductIds.Contains(productId) && !duplicatedProducts.Any(x => x == productName))
                    {
                        duplicatedProducts.Add(productName);
                    }
                    infoProductIds.Add(product.Id);
                }
                else
                {
                    invalidNames.Add(productName);
                }
            }

            if (invalidNames.Count > 0)
            {
                AddRowError(errorDict, colLetter, $"ไม่พบ product: {string.Join(", ", invalidNames)}");
            }

            if (duplicatedProducts.Count > 0)
            {
                AddRowError(errorDict, colLetter, $"ข้อมูล product ซ้ำ: {string.Join(", ", duplicatedProducts)}");
            }
        }

        private void AddRowError(Dictionary<string, string> dict, string key, string newValue)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, newValue);
            else
                dict[key] += $", {newValue}";
        }

        private static void CalculateVersionNo(Pager pager, List<DoNotCallHistoryEntity> list)
        {
            int totalItem = pager.TotalRecords;
            int start = totalItem - (pager.PageSize * (pager.PageNo - 1));
            int currentNo = start;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].VersionNo = currentNo;
                currentNo--;
            }
        }

        #endregion Private

        #region "IDisposable"

        private bool _disposed = false;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_dncContext != null) { _dncContext.Dispose(); }
                    if (_commonFacade != null) { _commonFacade.Dispose(); }
                    if (_lock != null) { _lock.Dispose(); }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int SaveCustomerFromInterface(DoNotCallByCustomerEntity model)
        {
            _doNotCallDataAccess = new DoNotCallDataAccess(_dncContext, _csmContext);
            if (model.BasicInfo.TransactionId > 0)
                return _doNotCallDataAccess.UpdateDoNotCallCustomerFromInterface(model);
            else
                return _doNotCallDataAccess.InsertDoNotCallCustomer(model);
        }

        #endregion
    }
}
