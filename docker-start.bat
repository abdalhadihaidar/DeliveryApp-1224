@echo off
REM Docker Start Script for DeliveryApp Backend (Windows)
REM This script helps you start the application with Docker

echo ==========================================
echo   DeliveryApp Backend - Docker Setup
echo ==========================================
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker is not running. Please start Docker Desktop first.
    exit /b 1
)

echo [OK] Docker is running
echo.

REM Check if docker-compose is available
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] docker-compose is not installed.
    exit /b 1
)

echo [OK] docker-compose is available
echo.

REM Ask user for environment
echo Select environment:
echo 1) Development (default)
echo 2) Production
set /p env_choice="Enter choice [1-2]: "

if "%env_choice%"=="2" (
    echo.
    echo [WARNING] Production mode selected
    echo Make sure you have set all required environment variables!
    echo.
    pause
    
    if exist ".env.docker" (
        echo [INFO] Loading environment variables from .env.docker
        docker-compose --env-file .env.docker up -d
    ) else (
        echo [WARNING] .env.docker file not found. Using default values.
        docker-compose up -d
    )
) else (
    echo.
    echo [INFO] Development mode selected
    docker-compose up -d
)

echo.
echo [INFO] Waiting for services to start...
timeout /t 5 /nobreak >nul

echo.
echo [INFO] Service Status:
docker-compose ps

echo.
echo ==========================================
echo [OK] Services started successfully!
echo ==========================================
echo.
echo [INFO] Web Application: http://localhost:5000
echo [INFO] Swagger UI: http://localhost:5000/swagger
echo [INFO] SQL Server: localhost:1433
echo.
echo [INFO] Useful commands:
echo   - View logs: docker-compose logs -f
echo   - Stop services: docker-compose down
echo   - View status: docker-compose ps
echo.

pause


