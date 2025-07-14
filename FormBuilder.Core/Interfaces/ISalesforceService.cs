using System.Threading.Tasks;
using FormBuilder.Core.Models.Salesforce;

namespace FormBuilder.Core.Interfaces
{
    public interface ISalesforceService
    {
        Task<string> GetAccessTokenAsync();
        Task<SalesforceCreateResponse> CreateAccountAsync(SalesforceAccount account);
        Task<SalesforceCreateResponse> CreateContactAsync(SalesforceContact contact, string accountId);
        Task<SalesforceAccount> GetAccountAsync(string accountId);
        Task<bool> TestConnectionAsync();

        Task<SalesforceCreateResponse> UpdateAccountAsync(string accountId, SalesforceAccount account);
        Task<SalesforceCreateResponse> UpdateContactAsync(string contactId, SalesforceContact contact);

        
    }
}