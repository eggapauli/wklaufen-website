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

$calendarDir = "$PSScriptRoot\public\calendar"
mkdir $calendarDir | Out-Null
Push-Location $calendarDir
try
{
    dotnet run --project "$PSScriptRoot\src\WkLaufen.Website.Calendar"
    if ($LASTEXITCODE -ne 0)
    {
        throw "Creating calendar failed."
    }
    Move-Item "$calendarDir\internal.ics" "$calendarDir\internal.ics"
}
finally
{
    Pop-Location
}
