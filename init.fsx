#r "System.Net.Http"

open System
open System.IO
open System.Net.Http

let commandLineArgs =
    fsi.CommandLineArgs
    |> Array.skip 1
    |> Array.pairwise
    |> Map.ofArray

let pathToPhpExe =
    commandLineArgs
    |> Map.tryFind "php-exe-path"
    |> function
    | Some x -> x
    | None -> "php.exe"

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
    printfn "Downloading composer if necessary"
    let setupFileName = Path.Combine(__SOURCE_DIRECTORY__, "composer-setup.php")
    let mainFileName = Path.Combine(__SOURCE_DIRECTORY__, "composer.phar")
    try
        if File.Exists mainFileName
        then printfn "%s already exists" mainFileName
        else
            printfn "%s doesn't exist" mainFileName
            if File.Exists setupFileName
            then printfn "%s already exists" setupFileName
            else
                printfn "%s doesn't exist" setupFileName
                use client = new HttpClient()
                let installerUrl = "https://getcomposer.org/installer"
                use sourceStream = client.GetStreamAsync (Uri installerUrl) |> Async.AwaitTask |> Async.RunSynchronously
                use targetStream = File.OpenWrite setupFileName
                sourceStream.CopyToAsync targetStream |> Async.AwaitTask |> Async.RunSynchronously
                printfn "Downloaded %s" setupFileName
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
                printfn "Hash is ok. Executing %s" setupFileName
                let startInfo = System.Diagnostics.ProcessStartInfo(pathToPhpExe, setupFileName)
                startInfo.RedirectStandardOutput <- true
                startInfo.RedirectStandardError <- true
                startInfo.UseShellExecute <- false
                use p = System.Diagnostics.Process.Start startInfo
                use d0 = p.OutputDataReceived.Subscribe(fun p -> printfn "%s" p.Data)
                use d1 = p.ErrorDataReceived.Subscribe(fun p -> eprintfn "%s" p.Data)
                p.BeginOutputReadLine()
                p.BeginErrorReadLine()
                
                p.WaitForExit()
                
                if p.ExitCode = 0
                then printfn "Successfully executed %s" setupFileName
                else failwithf "Error while executing %s. Exit code was %d" setupFileName p.ExitCode
            else
                actualHash |> printfn "Actual hash:   <%s>"
                expectedHash |> printfn "Expected hash: <%s>"
                failwith "Error while installing composer: Invalid installer signature"
    finally
        File.Delete setupFileName

