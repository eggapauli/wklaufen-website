Push-Location $PSScriptRoot\src\WkLaufen.Website.DataLoader

try
{
    dotnet run -- `
        --ooebv-username ${env:ooebv-username} `
        --ooebv-password ${env:ooebv-password} `
        --facebook-access-token ${env:facebook-access-token} `
        --public-calendar-url ${env:public-calendar-url} `
        --internal-calendar-url ${env:internal-calendar-url} `

    if ($LASTEXITCODE -ne 0)
    {
        throw "Loading data failed."
    }
}
finally
{
    Pop-Location
}
