@echo off
echo Configuring Windows Firewall for DeliveryApp.Web...
echo.

REM Add inbound rule for HTTPS port 44356
netsh advfirewall firewall add rule name="DeliveryApp.Web HTTPS" dir=in action=allow protocol=TCP localport=44356

REM Add outbound rule for HTTPS port 44356
netsh advfirewall firewall add rule name="DeliveryApp.Web HTTPS Outbound" dir=out action=allow protocol=TCP localport=44356

echo.
echo Firewall rules added successfully!
echo You can now access DeliveryApp.Web from other devices on your network at:
echo https://192.168.1.102:44356
echo.
pause 