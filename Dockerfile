# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and restore dependencies
COPY ["DeliveryApp.sln", "."]
COPY ["common.props", "."]
COPY ["NuGet.Config", "."]

# Copy all project files
COPY ["src/DeliveryApp.Domain.Shared/DeliveryApp.Domain.Shared.csproj", "src/DeliveryApp.Domain.Shared/"]
COPY ["src/DeliveryApp.Domain/DeliveryApp.Domain.csproj", "src/DeliveryApp.Domain/"]
COPY ["src/DeliveryApp.Application.Contracts/DeliveryApp.Application.Contracts.csproj", "src/DeliveryApp.Application.Contracts/"]
COPY ["src/DeliveryApp.Application/DeliveryApp.Application.csproj", "src/DeliveryApp.Application/"]
COPY ["src/DeliveryApp.EntityFrameworkCore/DeliveryApp.EntityFrameworkCore.csproj", "src/DeliveryApp.EntityFrameworkCore/"]
COPY ["src/DeliveryApp.HttpApi/DeliveryApp.HttpApi.csproj", "src/DeliveryApp.HttpApi/"]
COPY ["src/DeliveryApp.Web/DeliveryApp.Web.csproj", "src/DeliveryApp.Web/"]

# Restore packages for each project in dependency order
RUN dotnet restore "src/DeliveryApp.Domain.Shared/DeliveryApp.Domain.Shared.csproj" && \
    dotnet restore "src/DeliveryApp.Domain/DeliveryApp.Domain.csproj" && \
    dotnet restore "src/DeliveryApp.Application.Contracts/DeliveryApp.Application.Contracts.csproj" && \
    dotnet restore "src/DeliveryApp.Application/DeliveryApp.Application.csproj" && \
    dotnet restore "src/DeliveryApp.EntityFrameworkCore/DeliveryApp.EntityFrameworkCore.csproj" && \
    dotnet restore "src/DeliveryApp.HttpApi/DeliveryApp.HttpApi.csproj" && \
    dotnet restore "src/DeliveryApp.Web/DeliveryApp.Web.csproj"

# Copy all source files
COPY src/ src/
WORKDIR "/src/src/DeliveryApp.Web"

# Build the project
RUN dotnet build "DeliveryApp.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DeliveryApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for potential future use
RUN apt-get update && apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/Logs && chmod 777 /app/Logs

# Create wwwroot/uploads directory for file uploads
RUN mkdir -p /app/wwwroot/uploads/images && chmod 777 /app/wwwroot/uploads/images

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "DeliveryApp.Web.dll"]

