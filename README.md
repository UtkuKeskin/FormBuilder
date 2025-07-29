# FormBuilder - Google Forms Clone

A web application for creating customizable forms, quizzes, and surveys with enterprise integrations.

## Tech Stack
- ASP.NET Core 8.0 MVC
- Entity Framework Core
- PostgreSQL
- Bootstrap 5
- jQuery
- SignalR
- Cloudinary (for image storage)

## Projects Structure
- **FormBuilder.Web**: MVC Web Application
- **FormBuilder.Core**: Domain Models and Interfaces
- **FormBuilder.Infrastructure**: Data Access and External Services

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL
- VS Code or Visual Studio

### Setup
1. Clone the repository
2. Copy `appsettings.Development.json.example` to `appsettings.Development.json`
3. Update connection string and Cloudinary credentials
4. Run migrations: `dotnet ef database update`
5. Run the application: `dotnet run`

## Core Features
- User authentication and authorization
- Template creation with 4 question types
- Form filling and submission
- Real-time comments with SignalR
- Admin panel
- Multi-language support (EN/RU)
- Light/Dark theme

## Enterprise Integrations

### 1. Salesforce Integration
- **CRM Export**: Export user data to Salesforce as Account and Contact records
- **User Profile Action**: Dedicated form in user profile for Salesforce data collection
- **REST API**: Full integration with Salesforce REST API
- **Bidirectional Sync**: Create and update operations supported

### 2. Odoo ERP Integration
- **API Token System**: Secure token-based authentication for external access
- **Template Analytics**: Aggregated data export for template statistics
- **Custom Odoo Module**: Dedicated module for FormBuilder data import
- **Statistics Dashboard**: View template performance and response analytics
- **Read-only Viewer**: Safe data access for business intelligence

### 3. Power Automate Integration
- **Support Ticket System**: Help button available on all pages
- **Dropbox Integration**: Automatic JSON file upload to Dropbox
- **Email Notifications**: Automated email alerts to administrators
- **Mobile Push Notifications**: Real-time mobile alerts via Power Automate app
- **Priority-based Routing**: Different handling for High/Average/Low priority tickets

## Demo Video
🎥 **Full Integration Demo**: [Watch on YouTube](https://youtu.be/uBWVy2KhCe0?si=8YKioyC0zO0yqA5l)

The demo video showcases all three enterprise integrations:
- Salesforce CRM data export workflow
- Odoo ERP analytics import process  
- Power Automate support ticket automation with mobile notifications

## Architecture
Clean Architecture with 3 layers following SOLID principles:
- **Presentation Layer**: MVC Controllers and Views
- **Business Layer**: Core domain logic and interfaces
- **Data Layer**: Entity Framework and external service implementations

## Deployment
- **Live Application**: [FormBuilder on Render](https://formbuilder-app.onrender.com)
- **Database**: PostgreSQL on Render
- **File Storage**: Cloudinary CDN
- **Integrations**: Salesforce Developer Org, Local Odoo Instance, Microsoft Power Automate

## API Endpoints
- `/api/v1/templates` - Template management
- `/api/v1/forms` - Form submission handling
- `/api/support/ticket` - Support ticket creation
- `/api/analytics/{token}` - Odoo integration endpoint

## Configuration
Required configuration in `appsettings.json`:
```json
{
  "Salesforce": {
    "LoginUrl": "https://login.salesforce.com",
    "ClientId": "your_client_id",
    "ClientSecret": "your_client_secret"
  },
  "Dropbox": {
    "AppKey": "your_app_key",
    "AppSecret": "your_app_secret",
    "RefreshToken": "your_refresh_token"
  }
}
```

## Contributing
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License
This project is licensed under the MIT License.
