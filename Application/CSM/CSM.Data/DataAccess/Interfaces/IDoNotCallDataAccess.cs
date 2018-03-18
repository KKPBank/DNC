using CSM.Entity;
using CSM.Entity.Common;
using CSM.Service.Messages.DoNotCall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CSM.Data.DataAccess.Interfaces
{
    public interface IDoNotCallDataAccess
    {
        List<DoNotCallEntity> SearchDoNotCallList(DoNotCallListSearchFilter searchFilter);
        List<DoNotCallExcelModel> SearchDoNotCallListExcelModel(DoNotCallListSearchFilter searchFilter);
        DoNotCallByCustomerEntity GetDoNotCallCustomerFromCardId(string cardId);
        bool TransactionExists(int customerId);
        int InsertDoNotCallCustomer(DoNotCallByCustomerEntity model);
        int UpdateDoNotCallCustomer(DoNotCallByCustomerEntity model);
        int InsertDoNotCallTelephone(DoNotCallByTelephoneEntity model);
        int UpdateDoNotCallTelephone(DoNotCallByTelephoneEntity model);
        List<DoNotCallHistoryEntity> GetDoNotCallHistoryList(int customerId, Pager pager, int totalLimit);
        DoNotCallTransactionHistoryEntity GetDoNotCallHistoryDetailFromId(int logId);
        DoNotCallByCustomerEntity GetDoNotCallCustomerFromId(int id);
        List<DoNotCallUpdatePhoneNoModel> GenerateUpdatedDoNotCallPhoneList();
        DoNotCallTransactionInfo GetCustomerTransaction(string cardNo, string subscriptTypeCode);
        DoNotCallByTelephoneEntity GetDoNotCallTelephoneFromTransactionId(int id);
        DoNotCallTimePeriodEntity GetExecuteTimePeriodEntity(string executeTime);
        DoNotCallTransactionInfo GetDoNotCallTransactionInfoById(int transactionId);
        DoNotCallFileUploadDetailModel GetFileUploadDetail(int id, Pager pager);
        List<DoNotCallUpdatePhoneNoModel> GetDoNotCallPhoneNoListByPeriod(DateTime fromDateTime, DateTime toDateTime);
        List<DoNotCallFileUploadSearchResultModel> SearchDoNotCallUploadList(DoNotCallLoadListSearchFilter model);
        List<DoNotCallTransactionModel> SearchDoNotCallBlockByTelephoneContact(string phoneNo, string email, Pager pager);
        List<DoNotCallTransactionModel> SearchDoNotCallBlockByCustomerTransactionExact(string cardNo, int cardTypeId, Pager pager);
        List<SubscriptTypeEntity> GetActiveSubscriptionTypes();
        bool IsExistingCustomerTransactionCard(string cardNo, int subscriptionTypeId, int transactionId);
        DoNotCallTransactionEntity FindDoNotCallTransactionById(int transactionId);
        List<DoNotCallProductModel> GetAllProductsForFileUploadCompare();
        int SaveFileUploadModels(List<TB_T_DNC_TRANSACTION> models, TB_T_DNC_LOAD_LIST loadList);
        List<TransactionInfo> SearchExactDoNotCallTransaction(DoNotCallListSearchFilter searchFilter);
        int UpdateDoNotCallCustomerFromInterface(DoNotCallByCustomerEntity model);
    }
}
