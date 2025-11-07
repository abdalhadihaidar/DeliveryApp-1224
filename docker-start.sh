#!/bin/bash

# Docker Start Script for DeliveryApp Backend
# This script helps you start the application with Docker

set -e

echo "=========================================="
echo "  DeliveryApp Backend - Docker Setup"
echo "=========================================="
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker Desktop first."
    exit 1
fi

echo "✅ Docker is running"
echo ""

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Error: docker-compose is not installed."
    exit 1
fi

echo "✅ docker-compose is available"
echo ""

# Ask user for environment
echo "Select environment:"
echo "1) Development (default)"
echo "2) Production"
read -p "Enter choice [1-2]: " env_choice

case $env_choice in
    2)
        echo ""
        echo "⚠️  Production mode selected"
        echo "Make sure you have set all required environment variables!"
        echo ""
        read -p "Press Enter to continue or Ctrl+C to cancel..."
        
        if [ -f ".env.docker" ]; then
            echo "📄 Loading environment variables from .env.docker"
            docker-compose --env-file .env.docker up -d
        else
            echo "⚠️  Warning: .env.docker file not found. Using default values."
            docker-compose up -d
        fi
        ;;
    *)
        echo ""
        echo "🔧 Development mode selected"
        docker-compose up -d
        ;;
esac

echo ""
echo "⏳ Waiting for services to start..."
sleep 5

echo ""
echo "📊 Service Status:"
docker-compose ps

echo ""
echo "=========================================="
echo "✅ Services started successfully!"
echo "=========================================="
echo ""
echo "🌐 Web Application: http://localhost:5000"
echo "📚 Swagger UI: http://localhost:5000/swagger"
echo "🗄️  SQL Server: localhost:1433"
echo ""
echo "📋 Useful commands:"
echo "  - View logs: docker-compose logs -f"
echo "  - Stop services: docker-compose down"
echo "  - View status: docker-compose ps"
echo ""


