param(
    [Parameter(Mandatory=$true)][string]$ooebvUsername,
    [Parameter(Mandatory=$true)][string]$ooebvPassword,
    [Parameter(Mandatory=$true)][string]$facebookAccessToken
)

Push-Location $PSScriptRoot

$rootDir = Resolve-Path "..\.."
dotnet run -- `
    --ooebv-username $ooebvUsername `
    --ooebv-password $ooebvPassword `
    --facebook-access-token $facebookAccessToken

Pop-Location