using System;

namespace FormBuilder.Web.ViewModels
{
    public class ApiKeyViewModel
    {
        public string ApiKey { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasApiKey => !string.IsNullOrEmpty(ApiKey);
    }
}