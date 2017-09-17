#if INTERACTIVE
#I @"..\"
#I @"..\..\"
#I @"..\Common"
#r @"packages\FAKE\tools\FakeLib.dll"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#r @"packages\ImageProcessor\lib\net45\ImageProcessor.dll"
#load "Retry.fsx"
#load "Async.fsx"
#load "Choice.fsx"
#load "DataModels.fsx"
#load "Ftp.fsx"
#load "Http.fsx"
#load "Json.fsx"
#load "OOEBV.fsx"
#load "Facebook.fsx"
#load "DownloadHelper.fsx"
#load "Members.fsx"
#load "Contests.fsx"
#load "News.fsx"
#endif

#if COMPILED
module Main
#endif

open System
open System.IO
open Fake
open Fake.NpmHelper
open FSharp.Data
open DownloadHelper
open ImageProcessor

let rec findFirstMatchingFileInParentDirectory pattern dir =
    match TryFindFirstMatchingFile pattern dir with
    | Some file -> file
    | None ->
        Path.GetDirectoryName dir
        |> findFirstMatchingFileInParentDirectory pattern

let slnFile = findFirstMatchingFileInParentDirectory "WkLaufen.Website.sln" __SOURCE_DIRECTORY__
let rootDir = directory slnFile
let mainProjectDir = rootDir @@ "WkLaufen.Website"
let outputDir = mainProjectDir @@ "bin" @@ "html"
let imageBaseDir = mainProjectDir @@ "assets" @@ "images"
let dataDir = mainProjectDir @@ "data"

type ResizeDefinition = JsonProvider<"""{ "Path": "members\\image.jpg", "Width": 500, "Height": 1000 }""">
let resizeDefinitionFileName = "resize.txt"
let resizeDefinitionFilePath = mainProjectDir @@ "assets" @@ resizeDefinitionFileName

Target "Clean" <| fun () ->
    CleanDir outputDir
    DeleteFile resizeDefinitionFilePath

Target "DownloadMembers" <| fun () ->
    let ooebvUsername = getBuildParam "ooebv-username"
    let ooebvPassword = getBuildParam "ooebv-password"

    Members.download(ooebvUsername, ooebvPassword)
    |> Async.bind (
        Choice.bindAsync (
            List.map (Members.tryDownloadImage imageBaseDir)
            >> Async.ofList
            >> (Async.map Choice.ofList)
        )
    )
    |> Async.RunSynchronously
    |> Choice.map (List.map Members.getJson)
    |> Choice.map (saveEntries (dataDir @@ "members.json"))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded members."
    | Choice2Of2 x -> failwithf "Error while downloading members. %s" x

Target "DownloadContests" <| fun () ->
    let ooebvUsername = getBuildParam "ooebv-username"
    let ooebvPassword = getBuildParam "ooebv-password"

    Contests.download(ooebvUsername, ooebvPassword)
    |> Async.RunSynchronously
    |> Choice.map (List.map Contests.getJson)
    |> Choice.map (saveEntries (dataDir @@ "contests.json"))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded contests."
    | Choice2Of2 x -> failwithf "Error while downloading contests. %s" x

Target "DownloadNews" <| fun () ->
    let facebookAccessToken = getBuildParam "facebook-access-token"

    News.download facebookAccessToken
    |> Async.bind (Choice.bindAsync ((List.map (News.downloadImages imageBaseDir)) >> Async.ofList >> Async.map Choice.ofList))
    |> Async.RunSynchronously
    |> Choice.map (
        List.map News.getJson
        >> saveEntries (dataDir @@ "news.json")
    )
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded news."
    | Choice2Of2 x -> failwithf "Error while downloading news. %s" x

Target "DownloadNpmDependencies" <| fun () ->
    let setParams (x: NpmParams) =
        { x with
            Command = Install Standard
            WorkingDirectory = mainProjectDir
            NpmFilePath = rootDir @@ x.NpmFilePath
        }
    Npm setParams

Target "Build" <| fun () ->
    let setParams (p: MSBuildParams) =
        { p with
            Targets = ["Build"]
            Properties =
                [
                    "Configuration", environVarOrDefault "build-config" "Release"
                ]
        }
    build setParams slnFile

Target "ResizeImages" <| fun () ->
    let resize (sourcePath: string) (width, height) (targetPath: string) =
        use imageFactory = new ImageFactory()
        imageFactory.Load(sourcePath) |> ignore

        let resizeLayer =
            let widthRatio = (float imageFactory.Image.Width) / (float width)
            let heightRatio = (float imageFactory.Image.Height) / (float height)
            let resizeSize =
                if widthRatio > heightRatio
                then Drawing.Size(0, height)
                else Drawing.Size(width, 0)
            Imaging.ResizeLayer(resizeSize)
        imageFactory.Resize(resizeLayer) |> ignore

        let cropRect =
            let srcX = (imageFactory.Image.Width - width) / 2
            let srcY = (imageFactory.Image.Height - height) / 2
            Drawing.Rectangle(srcX, srcY, width, height)
        imageFactory.Crop(cropRect) |> ignore

        imageFactory.Save(targetPath) |> ignore

    resizeDefinitionFilePath
    |> ReadFile
    |> Seq.map ResizeDefinition.Parse
    |> Seq.distinctBy (fun def -> def.Path, def.Width, def.Height)
    |> Seq.map (fun def ->
        tracefn "Resizing %s to Width = %d, Height = %d" def.Path def.Width def.Height

        let fileName = sprintf "%s_%dx%d%s" (fileNameWithoutExt def.Path) def.Width def.Height (ext def.Path)
        let targetPath = outputDir @@ (directory def.Path) @@ fileName
        
        try
            resize (mainProjectDir @@ def.Path) (def.Width, def.Height) targetPath
            Choice1Of2 targetPath
        with e -> Choice2Of2 (sprintf "Can't resize %s: %O" def.Path e)
    )
    |> Seq.toList
    |> Choice.ofList
    |> function
    | Choice1Of2 _ -> ()
    | Choice2Of2 error -> failwithf "ERROR: %s" error

Target "CopyAssets" <| fun () ->
    !! ("assets/**/*")
    -- ("assets/" + resizeDefinitionFileName)
    -- ("assets/images/*/**/*.*")
    ++ ("node_modules/moment/min/moment-with-locales.min.js")
    ++ ("node_modules/slick-carousel/slick/slick.min.js")
    ++ ("node_modules/slick-carousel/slick/slick.css")
    ++ ("node_modules/slick-carousel/slick/slick-theme.css")
    ++ ("node_modules/slick-carousel/slick/ajax-loader.gif")
    ++ ("node_modules/slick-carousel/slick/fonts/slick.woff")
    ++ ("node_modules/slick-carousel/slick/fonts/slick.ttf")
    ++ ("node_modules/tooltipster/dist/js/tooltipster.bundle.min.js")
    ++ ("node_modules/tooltipster/dist/css/tooltipster.bundle.min.css")
    ++ ("node_modules/tooltipster/dist/css/plugins/tooltipster/sideTip/themes/tooltipster-sideTip-shadow.min.css")
    |> SetBaseDir mainProjectDir
    |> Seq.singleton
    |> CopyWithSubfoldersTo outputDir
    
Target "AddHtAccessFile" <| fun () ->
    let htAccessPath = outputDir @@ ".htaccess"
    let contentLines = [
        "AddType 'text/html; charset=UTF-8' .html"
        "AddType 'application/font-woff; charset=utf-8' .woff"
    ]
    File.WriteAllLines(htAccessPath, contentLines)

Target "FullBuild" DoNothing

Target "Upload" <| fun () ->
    let uploadUrl = getBuildParam "upload-url" |> Uri
    let tempUrl = getBuildParam "temp-url" |> Uri
    let uploadCredentials = {
        Ftp.Credential.Username = getBuildParam "upload-username"
        Ftp.Credential.Password = getBuildParam "upload-password"
    }

    Ftp.uploadDirectory outputDir uploadUrl tempUrl uploadCredentials
    |> function
    | Choice1Of2 () -> printfn "Successfully uploaded build"
    | Choice2Of2 () -> failwith "Error while uploading build"

Target "Default" DoNothing

"DownloadMembers" <== ["Clean"]
"DownloadContests" <== ["Clean"]
"DownloadNews" <== ["Clean"]
"DownloadNpmDependencies" <== ["Clean"]
"Build" <== ["DownloadMembers"; "DownloadContests"; "DownloadNews"; "DownloadNpmDependencies"]
"ResizeImages" <== ["Build"]
"CopyAssets" <== ["Build"]
"AddHtAccessFile" <== ["Build"]
"FullBuild" <== ["ResizeImages"; "CopyAssets"; "AddHtAccessFile"]
"Upload" <== ["FullBuild"]
"Default" <== ["Upload"]

#if INTERACTIVE
RunTargetOrDefault "Default"
#endif
