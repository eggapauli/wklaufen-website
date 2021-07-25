scoop install 7zip brave php

$phpPath = scoop prefix php
Copy-Item "$phpPath/php.ini-development" "$phpPath/php.ini"
Add-Content -Path "$phpPath/php.ini" -Value "extension_dir = ext"
Add-Content -Path "$phpPath/php.ini" -Value "extension=php_openssl.dll"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$signatureResponse = Invoke-WebRequest https://composer.github.io/installer.sig
$signature = [System.Text.Encoding]::UTF8.GetString($signatureResponse.Content).Trim()
php -r "copy('https://getcomposer.org/installer', 'composer-setup.php');"
php -r "if (hash_file('sha384', 'composer-setup.php') === '$signature') { echo 'Installer verified'; } else { echo 'Installer corrupt'; unlink('composer-setup.php'); } echo PHP_EOL;"
mkdir bin
php composer-setup.php --install-dir=bin --filename=composer
php -r "unlink('composer-setup.php');"
Push-Location src/WkLaufen.Website.Server
php ../../bin/composer install
Pop-Location

dotnet tool restore
.\.paket\paket.exe update --group DataLoader Selenium.WebDriver.ChromeDriver
dotnet restore
yarn install --frozen-lockfile
