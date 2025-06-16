# FormBuilder - Google Forms Clone

A web application for creating customizable forms, quizzes, and surveys.

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

## Features
- User authentication and authorization
- Template creation with 4 question types
- Form filling and submission
- Real-time comments with SignalR
- Admin panel
- Multi-language support
- Light/Dark theme

## Architecture
Clean Architecture with 3 layers following SOLID principles.
