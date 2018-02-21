Push-Location $PSScriptRoot\src\WkLaufen.Website

try
{
    dotnet fable yarn-build
    if ($LASTEXITCODE -ne 0)
    {
        throw "Building website failed."
    }
}
finally
{
    Pop-Location
}
