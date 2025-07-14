using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Models;
using FormBuilder.Core.Models.Salesforce;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FormBuilder.Infrastructure.Services
{
    public class SalesforceService : ISalesforceService
    {
        private readonly HttpClient _httpClient;
        private readonly SalesforceConfig _config;
        private readonly ILogger<SalesforceService> _logger;
        private string _accessToken;
        private string _instanceUrl;

        public SalesforceService(
            HttpClient httpClient,
            IOptions<SalesforceConfig> config,
            ILogger<SalesforceService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var tokenUrl = $"{_config.LoginUrl}/services/oauth2/token";
                
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", _config.ClientId),
                    new KeyValuePair<string, string>("client_secret", _config.ClientSecret),
                    new KeyValuePair<string, string>("username", _config.Username),
                    new KeyValuePair<string, string>("password", $"{_config.Password}{_config.SecurityToken}")
                });

                var response = await _httpClient.PostAsync(tokenUrl, formContent);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Salesforce authentication failed: {Content}", content);
                    throw new Exception($"Authentication failed: {content}");
                }

                var tokenResponse = JsonConvert.DeserializeObject<SalesforceTokenResponse>(content);
                _accessToken = tokenResponse.access_token;
                _instanceUrl = tokenResponse.instance_url;

                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Salesforce access token");
                throw;
            }
        }

        public async Task<SalesforceCreateResponse> CreateAccountAsync(SalesforceAccount account)
        {
            await EnsureAuthenticatedAsync();

            // State validation 
            if (string.IsNullOrWhiteSpace(account.BillingState))
            {
                account.BillingState = null;
            }

            var accountUrl = $"{_instanceUrl}/services/data/{_config.ApiVersion}/sobjects/Account";
            var json = JsonConvert.SerializeObject(account, new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore 
            });
            
            _logger.LogDebug("Creating Salesforce Account with data: {Json}", json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.PostAsync(accountUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Salesforce Account creation response: {Status} - {Content}", 
                response.StatusCode, responseContent);

            // Parse response correctly
            return ParseSalesforceResponse(responseContent, response.IsSuccessStatusCode);
        }

        public async Task<SalesforceCreateResponse> CreateContactAsync(SalesforceContact contact, string accountId)
        {
            await EnsureAuthenticatedAsync();

            contact.AccountId = accountId;
            var contactUrl = $"{_instanceUrl}/services/data/{_config.ApiVersion}/sobjects/Contact";
            var json = JsonConvert.SerializeObject(contact, new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore 
            });
            
            _logger.LogDebug("Creating Salesforce Contact with data: {Json}", json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.PostAsync(contactUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Salesforce Contact creation response: {Status} - {Content}", 
                response.StatusCode, responseContent);

            // Parse response correctly
            return ParseSalesforceResponse(responseContent, response.IsSuccessStatusCode);
        }

        public async Task<SalesforceCreateResponse> UpdateAccountAsync(string accountId, SalesforceAccount account)
        {
            await EnsureAuthenticatedAsync();

            // State validation 
            if (string.IsNullOrWhiteSpace(account.BillingState))
            {
                account.BillingState = null;
            }

            var accountUrl = $"{_instanceUrl}/services/data/{_config.ApiVersion}/sobjects/Account/{accountId}";
            
            var json = JsonConvert.SerializeObject(account, new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore 
            });
            
            _logger.LogInformation("Updating Salesforce Account {AccountId} with data: {Json}", accountId, json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            // PATCH request for update
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), accountUrl)
            {
                Content = content
            };
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Salesforce Account update response: {Status} - {Content}", 
                response.StatusCode, responseContent);

            // Salesforce returns 204 No Content for successful updates
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new SalesforceCreateResponse 
                { 
                    success = true, 
                    id = accountId 
                };
            }

            // Parse error response
            return ParseSalesforceResponse(responseContent, response.IsSuccessStatusCode);
        }

        public async Task<SalesforceCreateResponse> UpdateContactAsync(string contactId, SalesforceContact contact)
        {
            await EnsureAuthenticatedAsync();

            var contactUrl = $"{_instanceUrl}/services/data/{_config.ApiVersion}/sobjects/Contact/{contactId}";
            
            var json = JsonConvert.SerializeObject(contact, new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore 
            });
            
            _logger.LogInformation("Updating Salesforce Contact {ContactId} with data: {Json}", contactId, json);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            // PATCH request for update
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), contactUrl)
            {
                Content = content
            };
            
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Salesforce Contact update response: {Status} - {Content}", 
                response.StatusCode, responseContent);

            // Salesforce returns 204 No Content for successful updates
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new SalesforceCreateResponse 
                { 
                    success = true, 
                    id = contactId 
                };
            }

            // Parse error response
            return ParseSalesforceResponse(responseContent, response.IsSuccessStatusCode);
        }

        public async Task<SalesforceAccount> GetAccountAsync(string accountId)
        {
            await EnsureAuthenticatedAsync();

            var accountUrl = $"{_instanceUrl}/services/data/{_config.ApiVersion}/sobjects/Account/{accountId}";
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.GetAsync(accountUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Salesforce account: {Content}", content);
                return null;
            }

            return JsonConvert.DeserializeObject<SalesforceAccount>(content);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await GetAccessTokenAsync();
                return !string.IsNullOrEmpty(_accessToken);
            }
            catch
            {
                return false;
            }
        }

        private async Task EnsureAuthenticatedAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await GetAccessTokenAsync();
            }
        }

        private SalesforceCreateResponse ParseSalesforceResponse(string responseContent, bool isSuccess)
        {
            try
            {
                if (isSuccess)
                {
                    // Success response is an object
                    return JsonConvert.DeserializeObject<SalesforceCreateResponse>(responseContent);
                }
                else
                {
                    // Error response is an array
                    var errors = JsonConvert.DeserializeObject<List<SalesforceError>>(responseContent);
                    return new SalesforceCreateResponse
                    {
                        success = false,
                        errors = errors
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Salesforce response: {Response}", responseContent);
                return new SalesforceCreateResponse
                {
                    success = false,
                    errors = new List<SalesforceError>
                    {
                        new SalesforceError
                        {
                            message = "Failed to parse response: " + ex.Message,
                            errorCode = "PARSE_ERROR"
                        }
                    }
                };
            }
        }
    }
}