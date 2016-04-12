#r "System.Net.Http"

open System
open System.IO
open System.Net.Http

if not <| File.Exists "paket.exe" then
    //let url = Uri "http://fsprojects.github.io/Paket/stable"
    use client = new HttpClient()
    //let latestVersionUrl = client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously
    let latestVersionUrl = "https://github.com/fsprojects/Paket/releases/download/2.60.0/paket.exe"
    use sourceStream = client.GetStreamAsync (Uri latestVersionUrl) |> Async.AwaitTask |> Async.RunSynchronously
    use targetStream = File.OpenWrite "paket.exe"
    sourceStream.CopyToAsync targetStream |> Async.AwaitTask |> Async.RunSynchronously

#r "paket.exe"

open Paket

let options = InstallerOptions.Default
let dependencies = Dependencies.Locate __SOURCE_DIRECTORY__
let dependenciesFile = DependenciesFile.ReadFromFile dependencies.DependenciesFile
let lockFile =
    DependenciesFile.FindLockfile dependencies.DependenciesFile
    |> fun file -> LockFile.LoadFrom file.FullName
InstallProcess.Install(options, false, dependenciesFile, lockFile)
