# SmarterASP Deployment Guide for DeliveryApp

This guide explains how to deploy the DeliveryApp to SmarterASP hosting.

## Prerequisites

1. SmarterASP hosting account with .NET Core support
2. SQL Server database on SmarterASP (or external SQL Server)
3. Visual Studio or .NET CLI installed locally

## Configuration Files

### Environment-Specific Settings

- **Development**: `appsettings.json` - Uses local SQL Server
- **Production**: `appsettings.Production.json` - Uses remote SQL Server
- **Deployment**: `appsettings.Deployment.json` - Optimized for SmarterASP hosting

### Database Connection

The deployment configuration uses the remote SQL Server:
```
Server: SQL6032.site4now.net
Database: db_abd52c_sa
Username: db_abd52c_sa_admin
Password: RUN404error
```

## Deployment Steps

### Method 1: Using Deployment Scripts

1. **Run the deployment script:**
   ```bash
   # PowerShell
   .\deploy_to_smarterasp.ps1
   
   # Or Batch file
   .\deploy_to_smarterasp.bat
   ```

2. **Upload to SmarterASP:**
   - Navigate to the `src\DeliveryApp.Web\bin\Publish` folder
   - Upload all contents to your SmarterASP hosting directory
   - Ensure the folder structure is maintained

### Method 2: Manual Deployment

1. **Build the project:**
   ```bash
   cd src\DeliveryApp.Web
   dotnet publish --configuration Release --output "bin\Publish"
   ```

2. **Copy configuration files:**
   - Copy `web.config` to the publish directory
   - Copy `appsettings.Deployment.json` to the publish directory

3. **Upload to SmarterASP:**
   - Upload all files from the `bin\Publish` directory

### Method 3: Using Visual Studio Publish Profile

1. **Use existing publish profile:**
   - Right-click on `DeliveryApp.Web` project
   - Select "Publish"
   - Choose "abdalhadihaidar-002-subsite2 - FTP" profile
   - Click "Publish"

## Important Configuration Notes

### web.config Settings

The `web.config` file includes:
- ASP.NET Core module configuration
- Environment variable settings (`ASPNETCORE_ENVIRONMENT=Production`)
- Security headers
- URL rewriting rules
- Static content MIME types

### Environment Variables

The following environment variables are set in `web.config`:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://+:80`

### Database Configuration

Make sure your SmarterASP SQL Server database:
1. Has the correct credentials
2. Allows connections from the hosting server
3. Has the necessary permissions for the application user

## Troubleshooting

### Common Issues

1. **Database Connection Errors:**
   - Verify database credentials in `appsettings.Deployment.json`
   - Check if the SQL Server allows remote connections
   - Ensure the database exists and is accessible

2. **Application Startup Issues:**
   - Check the `Logs` folder on the server
   - Verify all required files are uploaded
   - Check the `web.config` configuration

3. **Permission Issues:**
   - Ensure the application pool has proper permissions
   - Check file permissions on the hosting directory

### Log Files

The application creates logs in the following locations:
- `Logs/logs.txt` - Application logs
- `Logs/stdout` - IIS stdout logs (if enabled)

### Testing the Deployment

1. **Check application startup:**
   - Visit your domain (e.g., `http://backend.waselsy.com`)
   - Check for any error pages

2. **Test API endpoints:**
   - Visit `http://backend.waselsy.com/swagger`
   - Test basic API functionality

3. **Database connectivity:**
   - Check application logs for database connection errors
   - Verify data can be read/written

## Security Considerations

1. **Database Credentials:**
   - Keep database credentials secure
   - Consider using environment variables for sensitive data

2. **Application Secrets:**
   - JWT secrets and encryption keys should be kept secure
   - Consider using Azure Key Vault or similar services for production

3. **Headers:**
   - Security headers are configured in `web.config`
   - Additional security measures can be added as needed

## Maintenance

### Regular Tasks

1. **Monitor logs** for errors and performance issues
2. **Update dependencies** regularly
3. **Backup database** regularly
4. **Test deployment** after any configuration changes

### Updates

When updating the application:
1. Build and test locally first
2. Use the deployment scripts to create a new package
3. Upload the new files to SmarterASP
4. Monitor logs for any issues

## Support

For issues with this deployment:
1. Check the application logs first
2. Verify all configuration files are correct
3. Test database connectivity
4. Check SmarterASP hosting status and support documentation


