# OrderLagerSystem

A comprehensive order and inventory management system built with .NET 8, Blazor Server, and SQLite.

## Architecture

### Backend (API)
- **Technology**: .NET 8 Web API
- **Database**: SQLite with Entity Framework Core
- **Authentication**: JWT Bearer tokens with ASP.NET Core Identity
- **Documentation**: Swagger/OpenAPI
- **Port**: 5265 (Development)

### Frontend (Client)
- **Technology**: Blazor Server with Interactive Server Components
- **UI Framework**: Bootstrap 5
- **Port**: 5123 (Development)

## User Roles

**1. Admin creates a new user**
- Logs in with administrator privileges
- Navigates to the user management section
- Enters user details and assigns a role (e.g., Order Coordinator)
- Creates the user, who then gains access to the system

**2. Order Coordinator creates orders and manages deliveries**
- Logs in with the "Order Coordinator" role
- Creates a new customer order with selected articles and quantities
- Registers inbound deliveries that increase stock levels
- Handles outbound deliveries by updating order status

**3. Employee registers inbound and outbound deliveries**
- Logs in with the "Employee" role
- Registers incoming goods via the delivery page
- Reduces stock levels when fulfilling outbound deliveries linked to orders

## Features

### User Management
-  Admin creates new users
-  Role-based access control
-  Password updates
-  User status (active/inactive)

### Order Management
-  Create customer orders with articles
-  Order status (Created → Confirmed → Processing → Shipped → Delivered)
-  Order history and tracking
-  External order numbers

### Inventory Management
-  Article catalog with SKU and descriptions
-  Stock balance and stock movements
-  Incoming deliveries (goods receipt)
-  Outgoing deliveries (order fulfillment)
-  Barcode generation for articles

### Reporting
-  Inventory overview with current balance
-  Order history per user
-  Stock movement log

## Quick Start

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone repository**
```bash
git clone https://github.com/your-username/OrderLagerSystem.git
cd OrderLagerSystem
```

2. **Start API**
```bash
cd OrderLagerSystem.Api
dotnet run
```
API will be available at: `http://localhost:5265`

3. **Start Client**
```bash
cd OrderLagerSystem.Client
dotnet run
```
Client will be available at: `http://localhost:5123`


## API Documentation

### Swagger UI
When the API is running in Development mode, visit:
- **Swagger UI**: `http://localhost:5265`
- **OpenAPI JSON**: `http://localhost:5265/swagger/v1/swagger.json`


## Testing

### TestSprite Integration
We use [TestSprite]for automated testing of both frontend and backend components.

### Test Coverage
- **Frontend Testing**: UI components, user interactions, responsive design
- **Backend Testing**: API endpoints, authentication, CRUD operations
- **Integration Testing**: End-to-end workflows and data integrity

### Test Results
- **Frontend**: 55.6% test success rate
- **Backend**: 87.5% API functionality verified
- **Overall**: 65.4% project test coverage

Test reports are available in the `testsprite_tests/` directory.

## Security

### Authentication
- JWT Bearer tokens with 24h expiration
- Role-based authorization (Admin, Order Coordinator, Employee)
- Password requirements: Minimum 6 characters

### CORS
- Configured for Blazor Client
- Allowed origins: `http://localhost:5123`

### Data Protection
- SQLite database with Entity Framework Core
- Database migrations for schema changes
- Seeded data for development and testing

## Monitoring

### Logging
- Structured logging with ILogger
- Error tracking and debugging
- Performance monitoring


## Development

### Branch Strategy
- `main` - Production branch
- `development` - Development branch
- `feature/*` - Feature branches

### Commit Convention
```
feat: add new order status
fix: resolve authentication issue
docs: update API documentation
test: add TestSprite test coverage
```

### Code Quality
- Nullable reference types enabled
- Async/await patterns
- Dependency injection
- Clean architecture principles

## Project Structure

```
OrderLagerSystem/
├── OrderLagerSystem.Api/          # Backend API
│   ├── Controllers/               # API Controllers
│   ├── Services/                  # Business Logic
│   ├── Models/                    # Data Models
│   ├── DTOs/                      # Data Transfer Objects
│   └── Data/                      # Database Context & Migrations
├── OrderLagerSystem.Client/       # Frontend Blazor App
│   ├── Components/                # Razor Components
│   ├── Pages/                     # Application Pages
│   └── Services/                  # Client-side Services
└── testsprite_tests/              # Test Documentation
    ├── internal-backend-api-test-report.md
    ├── testsprite-mcp-test-report.md
    └── test plans (JSON)
```

## Support

### Common Issues

**Issue**: API won't start
**Solution**: Check that port 5265 is available

**Issue**: Database errors
**Solution**: Delete `app.db` and let the system create a new one

**Issue**: Authentication fails
**Solution**: Check JWT settings in `appsettings.json`

**Issue**: Client can't connect to API
**Solution**: Verify API is running on port 5265 and CORS is configured

### Default Credentials
For development and testing:
- **Admin**: `shalan.mourad@datorlager.se` / `Admin123!`
- **Order Coordinator**: `Adel.Ali@datorlager.se` / `Order123!`
- **Employee**: `log.don@datorlager.se` / `Employee123!`

---

**OrderLagerSystem** - A modern order and inventory management system for small and medium businesses.
