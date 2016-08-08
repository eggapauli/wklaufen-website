#r "System.Net.Http"

open System
open System.IO
open System.Net.Http

let downloadPaket =
    let fileName = Path.Combine(__SOURCE_DIRECTORY__, "paket.exe")
    if not <| File.Exists fileName then
        //let url = Uri "http://fsprojects.github.io/Paket/stable"
        use client = new HttpClient()
        //let latestVersionUrl = client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously
        let latestVersionUrl = "https://github.com/fsprojects/Paket/releases/download/2.60.0/paket.exe"
        use sourceStream = client.GetStreamAsync (Uri latestVersionUrl) |> Async.AwaitTask |> Async.RunSynchronously
        use targetStream = File.OpenWrite fileName
        sourceStream.CopyToAsync targetStream |> Async.AwaitTask |> Async.RunSynchronously

#r "paket.exe"
open Paket

let installPaketDependencies =
    let options = InstallerOptions.Default
    let dependencies = Dependencies.Locate __SOURCE_DIRECTORY__
    let dependenciesFile = DependenciesFile.ReadFromFile dependencies.DependenciesFile
    let lockFile =
        DependenciesFile.FindLockfile dependencies.DependenciesFile
        |> fun file -> LockFile.LoadFrom file.FullName
    InstallProcess.Install(options, false, dependenciesFile, lockFile)

let downloadComposer =
    let setupFileName = Path.Combine(__SOURCE_DIRECTORY__, "composer-setup.php")
    let mainFileName = Path.Combine(__SOURCE_DIRECTORY__, "composer.phar")
    try
        if not <| File.Exists mainFileName then
            if not <| File.Exists setupFileName then
                use client = new HttpClient()
                let installerUrl = "https://getcomposer.org/installer"
                use sourceStream = client.GetStreamAsync (Uri installerUrl) |> Async.AwaitTask |> Async.RunSynchronously
                use targetStream = File.OpenWrite setupFileName
                sourceStream.CopyToAsync targetStream |> Async.AwaitTask |> Async.RunSynchronously
            let expectedHash =
                use client = new HttpClient()
                let sigUrl = "https://composer.github.io/installer.sig"
                use sourceStream = client.GetStreamAsync (Uri sigUrl) |> Async.AwaitTask |> Async.RunSynchronously
                use sourceStreamReader = new StreamReader(sourceStream)
                sourceStreamReader.ReadToEnd().Trim()
            let actualHash =
                let data = File.ReadAllBytes setupFileName
                use hashFn = new System.Security.Cryptography.SHA384Managed()
                hashFn.ComputeHash data
                |> Seq.map (sprintf "%02x")
                |> String.concat ""
            if actualHash = expectedHash
            then
                let pathToPhpExe = "php.exe"
                use p = System.Diagnostics.Process.Start(pathToPhpExe, "composer-setup.php")
                p.WaitForExit()
            else
                actualHash |> printfn "Actual hash:   <%s>"
                expectedHash |> printfn "Expected hash: <%s>"
                failwith "Error while installing composer: Invalid installer signature"
    finally
        File.Delete setupFileName

