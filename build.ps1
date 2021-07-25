dotnet fable .\src\WkLaufen.Website\ --run yarn --cwd .\src\WkLaufen.Website build
dotnet run --project .\src\WkLaufen.Website.ServerGenerator

Copy-Item .\src\WkLaufen.Website.Server\*.php .\public
Copy-Item -Recurse .\src\WkLaufen.Website.Server\vendor .\public
7z a app.zip .\public\.
