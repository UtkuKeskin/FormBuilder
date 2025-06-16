namespace FormBuilder.Web.Constants
{
    public static class AppConstants
    {
        // Pagination
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 50;
        
        // File Upload
        public const int MaxImageSizeMB = 5;
        public const string AllowedImageExtensions = ".jpg,.jpeg,.png,.webp";
        
        // Cache Keys
        public const string TagCloudCacheKey = "tag_cloud";
        public const string PopularTemplatesCacheKey = "popular_templates";
        public const int CacheExpirationMinutes = 30;
        
        // Roles
        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        
        // Question Limits
        public const int MaxQuestionsPerType = 4;
        public const int TotalQuestionTypes = 4;
        
        // UI
        public const string DefaultTheme = "light";
        public const string DefaultLanguage = "en";
        
        // Template
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 1000;
        public const int MaxQuestionLength = 500;
        
        // Forms
        public const int MaxStringAnswerLength = 500;
        public const int MaxTextAnswerLength = 5000;
    }
}
