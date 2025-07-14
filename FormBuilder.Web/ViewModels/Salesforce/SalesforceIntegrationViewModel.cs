using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FormBuilder.Web.ViewModels.Salesforce
{
    public class SalesforceIntegrationViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool IsIntegrated { get; set; }
        public DateTime? LastSyncDate { get; set; }

        // Company Information
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [StringLength(50)]
        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Range(0, 999999999999, ErrorMessage = "Please enter a valid revenue amount")]
        [Display(Name = "Annual Revenue")]
        public decimal? AnnualRevenue { get; set; }

        [Range(1, 999999, ErrorMessage = "Please enter a valid number of employees")]
        [Display(Name = "Number of Employees")]
        public int? NumberOfEmployees { get; set; }

        [Phone]
        [Display(Name = "Company Phone")]
        public string? CompanyPhone { get; set; }

        // Contact Information
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone]
        [Display(Name = "Personal Phone")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? Title { get; set; }

        [StringLength(50)]
        [Display(Name = "Department")]
        public string? Department { get; set; }

        // Address Information
        [StringLength(100)]
        [Display(Name = "Street Address")]
        public string? Street { get; set; }

        [StringLength(50)]
        [Display(Name = "City")]
        public string? City { get; set; }

        
        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Please enter a valid postal code")]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Country")]
        public string? Country { get; set; } = "United States";

        // Static helper for US states
        public static Dictionary<string, string> USStates => new Dictionary<string, string>
        {
            {"", "-- Select State --"},
            {"Alabama", "Alabama"},
            {"Alaska", "Alaska"},
            {"Arizona", "Arizona"},
            {"Arkansas", "Arkansas"},
            {"California", "California"},
            {"Colorado", "Colorado"},
            {"Connecticut", "Connecticut"},
            {"Delaware", "Delaware"},
            {"District of Columbia", "District of Columbia"},
            {"Florida", "Florida"},
            {"Georgia", "Georgia"},
            {"Hawaii", "Hawaii"},
            {"Idaho", "Idaho"},
            {"Illinois", "Illinois"},
            {"Indiana", "Indiana"},
            {"Iowa", "Iowa"},
            {"Kansas", "Kansas"},
            {"Kentucky", "Kentucky"},
            {"Louisiana", "Louisiana"},
            {"Maine", "Maine"},
            {"Maryland", "Maryland"},
            {"Massachusetts", "Massachusetts"},
            {"Michigan", "Michigan"},
            {"Minnesota", "Minnesota"},
            {"Mississippi", "Mississippi"},
            {"Missouri", "Missouri"},
            {"Montana", "Montana"},
            {"Nebraska", "Nebraska"},
            {"Nevada", "Nevada"},
            {"New Hampshire", "New Hampshire"},
            {"New Jersey", "New Jersey"},
            {"New Mexico", "New Mexico"},
            {"New York", "New York"},
            {"North Carolina", "North Carolina"},
            {"North Dakota", "North Dakota"},
            {"Ohio", "Ohio"},
            {"Oklahoma", "Oklahoma"},
            {"Oregon", "Oregon"},
            {"Pennsylvania", "Pennsylvania"},
            {"Rhode Island", "Rhode Island"},
            {"South Carolina", "South Carolina"},
            {"South Dakota", "South Dakota"},
            {"Tennessee", "Tennessee"},
            {"Texas", "Texas"},
            {"Utah", "Utah"},
            {"Vermont", "Vermont"},
            {"Virginia", "Virginia"},
            {"Washington", "Washington"},
            {"West Virginia", "West Virginia"},
            {"Wisconsin", "Wisconsin"},
            {"Wyoming", "Wyoming"}
        };
    }
}