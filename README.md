# Text Check-In Backend System

A modern, scalable Azure Functions-based backend system for text-based check-in functionality with Next.js frontend support.

## **Project Structure**

```
Text-Check-In-Backend/
├── backend/                          # C# Azure Functions Backend
│   ├── src/
│   │   ├── TextCheckIn.Functions/    # Azure Functions (API Layer)
│   │   ├── TextCheckIn.Core/         # Business Logic Layer
│   │   ├── TextCheckIn.Data/         # Data Access Layer
│   │   └── TextCheckIn.Shared/       # Shared Utilities
│   ├── tests/                        # Test Projects
│   └── TextCheckIn.Backend.sln       # Solution File
├── frontend/                         # Next.js Frontend (Future)
├── docs/                            # Documentation
└── README.md                        # This File
```

## **Getting Started**

### **Prerequisites**

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Azure Functions Core Tools v4** - `npm install -g azure-functions-core-tools@4`
- **Visual Studio 2022** or **JetBrains Rider** or **VS Code**
- **SQL Server LocalDB** or **Azure SQL Database**
- **Node.js 18+** (for frontend)

### **Setup Instructions**

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd Text-Check-In-Backend
   ```

2. **Backend Setup**
   ```bash
   cd backend/src/TextCheckIn.Functions
   
   # Copy the local settings template
   cp local.settings.json.template local.settings.json
   
   # Edit local.settings.json with your configuration
   # Configure database, SMS provider, etc.
   
   # Restore packages
   dotnet restore
   
   # Run the Functions locally
   func start
   ```

3. **Database Setup**
   ```bash
   # Install EF Core tools (if not already installed)
   dotnet tool install --global dotnet-ef
   
   # Create and apply migrations
   cd backend/src/TextCheckIn.Data
   dotnet ef migrations add InitialCreate --startup-project ../TextCheckIn.Functions
   dotnet ef database update --startup-project ../TextCheckIn.Functions
   ```

## **Architecture Overview**

### **Clean Architecture Principles**

Our backend follows Clean Architecture patterns:

- **Functions Layer** - HTTP triggers, API endpoints, request/response handling
- **Core Layer** - Business logic, domain models, service interfaces
- **Data Layer** - Entity Framework, repositories, database context
- **Shared Layer** - Common utilities, extensions, constants

### **Dependency Flow**
```
Functions → Core → Data
    ↓        ↓      ↓
  Shared ← Shared ← Shared
```

## **Configuration**

### **Local Development (`local.settings.json`)**

```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    
    // SMS Configuration
    "SmsService:Provider": "Twilio",
    "SmsService:Twilio:AccountSid": "your-sid",
    "SmsService:Twilio:AuthToken": "your-token",
    
    // Database
    "ConnectionStrings:DefaultConnection": "Server=...;Database=...;"
  }
}
```

### **Production Configuration**

Use Azure App Settings or Azure Key Vault for production secrets.

## **Best Practices Implemented**

### **Code Structure**
- **Clean Architecture** - Separation of concerns
- **SOLID Principles** - Single responsibility, open/closed, etc.
- **Dependency Injection** - Built-in .NET DI container
- **Configuration Pattern** - IOptions<T> for settings
- **Repository Pattern** - Data access abstraction

### **Security**
- **Input Validation** - FluentValidation
- **Authentication** - JWT Bearer tokens
- **CORS Configuration** - Proper cross-origin setup
- **Security Headers** - XSS, CSRF protection
- **Error Handling** - No sensitive data exposure

### **Monitoring & Logging**
- **Application Insights** - Telemetry and monitoring
- **Structured Logging** - JSON formatted logs
- **Health Checks** - Endpoint monitoring
- **Performance Counters** - Resource utilization

### **Testing**
- **Unit Tests** - Core business logic testing
- **Integration Tests** - API endpoint testing
- **Test Fixtures** - Reusable test components

### **DevOps**
- **CI/CD Ready** - GitHub Actions workflow templates
- **Infrastructure as Code** - ARM/Bicep templates
- **Environment Configs** - Dev/Staging/Production

## **API Endpoints**

### **Check-In Management**
```
POST   /api/checkins           # Create check-in
GET    /api/checkins           # List check-ins
GET    /api/checkins/{id}      # Get specific check-in
PUT    /api/checkins/{id}      # Update check-in
DELETE /api/checkins/{id}      # Delete check-in
```

### **SMS Notifications**
```
POST   /api/sms/send           # Send SMS notification
GET    /api/sms/status/{id}    # Check SMS delivery status
```

### **Health & Monitoring**
```
GET    /api/health             # Health check endpoint
GET    /api/health/detailed    # Detailed health information
```

## **SMS Provider Integration**

### **Supported Providers**
- **Twilio** (Primary)
- **Azure Communication Services**
- **SendGrid** (Email + SMS)

### **Configuration Example**
```json
{
  "SmsService": {
    "Provider": "Twilio",
    "Twilio": {
      "AccountSid": "your-twilio-sid",
      "AuthToken": "your-twilio-token",
      "FromPhoneNumber": "+1234567890"
    }
  }
}
```

## **Testing Strategy**

### **Unit Tests**
```bash
cd backend/tests/TextCheckIn.Core.Tests
dotnet test
```

### **Integration Tests**
```bash
cd backend/tests/TextCheckIn.Integration.Tests
dotnet test
```

### **Function Tests**
```bash
cd backend/tests/TextCheckIn.Functions.Tests
dotnet test
```

## **Deployment**

### **Local Development**
```bash
cd backend/src/TextCheckIn.Functions
func start --cors "*"
```

### **Azure Deployment**
```bash
# Build and publish
dotnet publish --configuration Release

# Deploy using Azure CLI
az functionapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name myFunctionApp \
  --src publish.zip
```

## **Development Workflow**

1. **Feature Development**
   - Create feature branch from `main`
   - Implement in appropriate layer (Core → Data → Functions)
   - Add unit tests
   - Update documentation

2. **Code Quality**
   - Follow C# coding standards
   - Use async/await patterns
   - Implement proper error handling
   - Add XML documentation

3. **Testing**
   - Write unit tests for Core layer
   - Add integration tests for API endpoints
   - Test SMS functionality with test providers

4. **Review & Merge**
   - Create pull request
   - Code review
   - CI/CD pipeline validation
   - Merge to main

## **Additional Resources**

- [Azure Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [Entity Framework Core Guide](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## **Contributing**

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

## **License**

© Oil Changers. All rights reserved.

---

**Ready to build amazing check-in experiences!**