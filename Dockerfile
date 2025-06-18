# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY FormBuilder.sln .
COPY FormBuilder.Web/FormBuilder.Web.csproj FormBuilder.Web/
COPY FormBuilder.Core/FormBuilder.Core.csproj FormBuilder.Core/
COPY FormBuilder.Infrastructure/FormBuilder.Infrastructure.csproj FormBuilder.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/FormBuilder.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:$PORT

ENTRYPOINT ["dotnet", "FormBuilder.Web.dll"]
