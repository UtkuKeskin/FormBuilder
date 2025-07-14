namespace FormBuilder.Core.Models.Salesforce
{
    public class SalesforceTokenResponse
    {
        public string access_token { get; set; }
        public string instance_url { get; set; }
        public string id { get; set; }
        public string token_type { get; set; }
        public string issued_at { get; set; }
        public string signature { get; set; }
    }

    public class SalesforceAccount
    {
        public string Name { get; set; }
        public string Type { get; set; } = "Customer";
        public string Industry { get; set; }
        public decimal? AnnualRevenue { get; set; }
        public int? NumberOfEmployees { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string BillingStreet { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }
    }

    public class SalesforceContact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string AccountId { get; set; }
    }

    public class SalesforceCreateResponse
    {
        public string id { get; set; }
        public bool success { get; set; }
        public List<SalesforceError> errors { get; set; }
    }

    public class SalesforceUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<SalesforceError> Errors { get; set; }
    }


    public class SalesforceError
    {
        public string message { get; set; }
        public string errorCode { get; set; }
        public List<string> fields { get; set; }
    }
}