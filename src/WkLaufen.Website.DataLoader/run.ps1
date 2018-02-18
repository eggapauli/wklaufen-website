param(
    [Parameter(Mandatory=$true)][string]$ooebvUsername,
    [Parameter(Mandatory=$true)][string]$ooebvPassword,
    [Parameter(Mandatory=$true)][string]$facebookAccessToken
)

Push-Location $PSScriptRoot

$rootDir = Resolve-Path "..\.."
dotnet run -- `
    --root-dir $rootDir `
    --data-dir "src\WkLaufen.Website\generated" `
    --image-dir "images" `
    --deploy-dir "public" `
    --ooebv-username $ooebvUsername `
    --ooebv-password $ooebvPassword `
    --facebook-access-token $facebookAccessToken

Pop-Location