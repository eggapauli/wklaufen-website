$client = New-Object System.Net.WebClient
$client.Credentials = New-Object System.Net.NetworkCredential($env:FTP_USER, $env:FTP_PASSWORD)
$client.UploadFile("ftp://$env:FTP_HOST/tmp/deploy/app.zip", "$pwd\app.zip")
$client.UploadFile("ftp://$env:FTP_HOST/web/publish.php", "$pwd\src\publish.php")
Invoke-WebRequest -Uri "http://wk-laufen.at/publish.php?package-name=app.zip" | Out-Null
