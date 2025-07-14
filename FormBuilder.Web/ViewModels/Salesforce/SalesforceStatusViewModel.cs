namespace FormBuilder.Web.ViewModels.Salesforce
{
    public class SalesforceStatusViewModel
    {
        public bool IsIntegrated { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string? AccountId { get; set; }
        public string? ContactId { get; set; }
    }
}