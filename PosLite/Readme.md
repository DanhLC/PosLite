Add-Migration Init
Update-Database

�� Create localsite
dotnet publish -c Release -o ./publish
cd path\to\publish
dotnet PosLite.dll

Windows Defender Firewall with Advanced Security
Inbound Rules > New Rule
Port �� Next
TCP �� 5000
Allow the connection �� Next
ASP.NET Localhost
Finish

shell:startup

Please notice about the port number 5000, you can change it in the appsettings.json file. It's cannot work the same port with other application.