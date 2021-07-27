dotnet fable .\src\WkLaufen.Website\ --run yarn --cwd .\src\WkLaufen.Website build
dotnet run --project .\src\WkLaufen.Website.ServerGenerator

Copy-Item .\src\WkLaufen.Website.Server\*.php .\public -Force
Copy-Item .\src\WkLaufen.Website.Server\vendor .\public -Recurse -Force
7z a app.zip .\public\.
