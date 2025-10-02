# DeliveryApp.HttpApi.Host

This is a dedicated API-only host for the DeliveryApp project. It provides the same API functionality as the main `DeliveryApp.Web` project but without the web UI components.

## Purpose

- **API-Only Host**: This project serves as a dedicated API server without web UI components
- **Separate Port**: Runs on port 44357 (different from the main web app on 44356)
- **Same Functionality**: Provides all the same API endpoints as the main web application
- **Swagger Documentation**: Includes Swagger/OpenAPI documentation at `/swagger`

## Configuration

- **Port**: 44357 (HTTPS) / 44358 (HTTP)
- **Database**: Uses the same database as the main application
- **Authentication**: JWT Bearer token authentication
- **CORS**: Configured to allow cross-origin requests

## Usage

### Running the API Host

```bash
# Navigate to the project directory
cd src/DeliveryApp.HttpApi.Host

# Run the application
dotnet run
```

### Accessing the API

- **Swagger UI**: https://localhost:44357/swagger
- **API Base URL**: https://localhost:44357/api
- **Token Endpoint**: https://localhost:44357/connect/token

### API Endpoints

All API endpoints are available at the same paths as in the main web application:

- `/api/app/restaurant` - Restaurant management
- `/api/app/order` - Order management
- `/api/app/user` - User management
- And all other application endpoints

## Relationship with DeliveryApp.Web

Both projects share the same:
- Application logic (`DeliveryApp.Application`)
- API contracts (`DeliveryApp.HttpApi`)
- Database context (`DeliveryApp.EntityFrameworkCore`)
- Domain models and business logic

The main difference is that `DeliveryApp.Web` includes web UI components while `DeliveryApp.HttpApi.Host` is API-only.

## Development

When developing, you can run both projects simultaneously:
- `DeliveryApp.Web` on port 44356 (with web UI)
- `DeliveryApp.HttpApi.Host` on port 44357 (API only)

This allows you to test both the web interface and the API independently.
