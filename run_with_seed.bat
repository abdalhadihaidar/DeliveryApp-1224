@echo off
echo Running the backend with seeded data...
cd /d %~dp0
dotnet run --project src/DeliveryApp.HttpApi.Host/DeliveryApp.HttpApi.Host.csproj