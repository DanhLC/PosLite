Add-Migration Init
Update-Database

dotnet publish -c Release -o ./publish
cd path\to\publish
dotnet PosLite.dll

Å® Create localsite
Windows Defender Firewall with Advanced Security
Inbound Rules > New Rule
Port Å® Next
TCP Å® 5000
Allow the connection Å® Next
ASP.NET Localhost
Finish

Please notice about the port number 5000, you can change it in the appsettings.json file. It's cannot work the same port with other application.