# DeliveryApp.Web Network Access Setup

This guide will help you configure DeliveryApp.Web to be accessible on your local network.

## Changes Made

The following files have been modified to enable network access:

1. **Properties/launchSettings.json** - Updated to bind to all network interfaces (0.0.0.0)
2. **appsettings.json** - Updated SelfUrl to use local network IP
3. **appsettings.Development.json** - Updated SelfUrl to use local network IP
4. **Program.cs** - Added explicit binding to all network interfaces

## Your Network Configuration

- **Local IP Address**: 192.168.1.102
- **Port**: 44356
- **Access URL**: https://192.168.1.102:44356

## Setup Instructions

### 1. Configure Windows Firewall (Required)

You need to run the firewall configuration script as Administrator:

1. Right-click on PowerShell and select "Run as Administrator"
2. Navigate to the backend_v1_3 directory
3. Run the firewall configuration script:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\configure_firewall.ps1
```

Alternatively, you can manually add firewall rules:

1. Open Windows Defender Firewall with Advanced Security
2. Add Inbound Rule:
   - Rule Type: Port
   - Protocol: TCP
   - Port: 44356
   - Action: Allow the connection
   - Profile: Domain, Private, Public
   - Name: "DeliveryApp.Web HTTPS"

### 2. Run the Application

Navigate to the DeliveryApp.Web project directory and run:

```bash
dotnet run
```

Or use Visual Studio to run the project.

### 3. Access from Other Devices

Once the application is running, other devices on your network can access it at:

```
https://192.168.1.102:44356
```

## Troubleshooting

### SSL Certificate Issues

If you encounter SSL certificate warnings, you can:

1. **For Development**: Accept the certificate warning in your browser
2. **For Production**: Install a proper SSL certificate

### Connection Issues

1. **Check Firewall**: Ensure the firewall rules are properly configured
2. **Check Network**: Ensure all devices are on the same network
3. **Check IP Address**: Verify your IP address hasn't changed (run `ipconfig` to check)

### Port Already in Use

If port 44356 is already in use, you can:

1. Change the port in all configuration files
2. Or stop the service using that port

## Security Considerations

- This configuration is suitable for development and local network testing
- For production deployment, consider:
  - Using a reverse proxy (nginx, IIS)
  - Implementing proper SSL certificates
  - Configuring authentication and authorization
  - Restricting access to specific IP ranges

## Verification

To verify the setup is working:

1. Run the application
2. Open a browser on another device on the same network
3. Navigate to `https://192.168.1.102:44356`
4. The application should load successfully

## Notes

- The application will now be accessible from any device on your local network
- Make sure your antivirus software doesn't block the connection
- If using a corporate network, check with your IT department for any restrictions 