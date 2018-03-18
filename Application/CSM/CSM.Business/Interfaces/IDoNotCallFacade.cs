using CSM.Data.DataAccess;
using CSM.Entity;
using CSM.Entity.Common;
using CSM.Service.Messages.DoNotCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CSM.Business.Interfaces
{
    public interface IDoNotCallFacade: IDisposable
    {
        List<DoNotCallEntity> SearchDoNotCallList(DoNotCallListSearchFilter searchFilter);
        byte[] GenerateDoNotCallListExcelFile(DoNotCallListSearchFilter searchFilter);
        DoNotCallByCustomerEntity GetDoNotCallCustomerModelByCardId(string cardId);
        int SaveCustomer(DoNotCallByCustomerEntity model);
        int SavePhone(DoNotCallByTelephoneEntity model);
        DoNotCallByTelephoneEntity GetDoNotCallByTelephoneEntity(int id);
        DoNotCallByCustomerEntity GetDoNotCallCustomerModelById(int id);
        DoNotCallTransactionHistoryEntity GetDoNotCallHistoryDetail(int logId);
        List<DoNotCallHistoryEntity> GetHistoryDoNotCallListByCustomerId(int customerId, Pager pager, int totalLimit);
        List<DoNotCallTransactionModel> SearchDoNotCallCustomerTransactionList(string cardNo, int cardTypeId, Pager pager);
        List<DoNotCallTransactionModel> SearchDoNotCallTelephoneContact(string email, string phoneNo, Pager pager);
        List<SubscriptTypeEntity> GetSubscriptTypeSelectList();
        bool IsExistingCustomerTransactionCardInfo(string cardNo, int subscriptionTypeId, int transactionId);
        DoNotCallTransactionEntity FindDoNotCallTransactionById(int transactionId);
        DoNotCallFileUploadResultModel SaveUploadFile(string fullFilePath, int userId, string userIP, string username, string uploadFileName, string contentType, int maxRowCount);
        List<DoNotCallFileUploadSearchResultModel> SearchDoNotCallUploadList(DoNotCallLoadListSearchFilter model);
        DoNotCallFileUploadDetailModel GetViewUploadFileDetail(int id, Pager pager);
        List<DoNotCallUpdatePhoneNoModel> GetUpdatedDoNotCallPhoneNoList();
        void ExportDoNotCallUpdateFile(List<DoNotCallUpdatePhoneNoModel> newDataList);
        List<TransactionInfo> SearchExactDoNotCallTransaction(DoNotCallListSearchFilter searchFilter);
        DoNotCallTransactionInfo GetCustomerTransaction(string cardNo, string subscriptTypeCode);
        bool TransactionExists(int id);
        DoNotCallTransactionInfo GetTelephoneTransactionById(int transactionId);
        List<DoNotCallUpdatePhoneNoModel> GetDoNotCallPhoneNoListByExecuteTime(string executeTime);
        byte[] GenerateDoNotCallPhoneListAttachment(List<DoNotCallUpdatePhoneNoModel> newDataList);
        void GenerateDoNotCallPhoneListFile(byte[] fileByte, string fileName, bool success);
        int SaveCustomerFromInterface(DoNotCallByCustomerEntity entity);
    }
}
