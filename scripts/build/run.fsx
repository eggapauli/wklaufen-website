#I @"..\..\"
#r @"packages\FAKE\tools\FakeLib.dll"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#load @"..\common\retry.fsx"
#load @"..\common\Choice.fsx"
#load @"..\common\DataModels.fsx"
#load @"..\common\Ftp.fsx"
#load @"..\common\Http.fsx"
#load @"..\common\Json.fsx"
#load @"..\common\OOEBV.fsx"
#load @"..\facebook\Facebook.fsx"
#load @".\DownloadHelper.fsx"
#load @".\Members.fsx"
#load @".\News.fsx"

open System
open System.Drawing
open System.Drawing.Drawing2D
open System.Drawing.Imaging
open System.IO
open Fake
open Fake.NpmHelper
open FSharp.Data
open DownloadHelper

let ooebvUsername = getBuildParam "ooebv-username"
let ooebvPassword = getBuildParam "ooebv-password"
let facebookAccessToken = getBuildParam "facebook-access-token"

let uploadUrl = getBuildParam "upload-url" |> Uri
let uploadCredentials = {
    Ftp.Credential.Username = getBuildParam "upload-username"
    Ftp.Credential.Password = getBuildParam "upload-password"
}

let mainProjectDir = "WkLaufen.Website"
let outputDir = mainProjectDir @@ "bin" @@ "html"
let imageBaseDir = mainProjectDir @@ "assets" @@ "images"
let dataDir = mainProjectDir @@ "data"

type ResizeDefinition = JsonProvider<"""{ "Path": "members\\image.jpg", "Width": 500, "Height": 1000 }""">
let resizeDefinitionFileName = "resize.txt"
let resizeDefinitionFilePath = mainProjectDir @@ "assets" @@ "resize.txt"

let slnFile = FindFirstMatchingFile "*.sln" "."
Target "Clean" <| fun () ->
    // Cleaning the project fails on Azure because https://github.com/intellifactory/websharper/issues/504
    //let setParams (p: MSBuildParams) =
    //    { p with
    //        Targets = ["Clean"]
    //        Properties =
    //            [
    //                "Configuration", "Release"
    //            ]
    //    }
    //build setParams slnFile

    DeleteFile resizeDefinitionFilePath

Target "DownloadMembers" <| fun () ->
    Members.download(ooebvUsername, ooebvPassword)
    |> Choice.bind ((List.map (Members.tryDownloadImage imageBaseDir)) >> Choice.ofList)
    |> Choice.map (List.map Members.getJson)
    |> Choice.map (saveEntries (dataDir @@ "members.json"))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded members."
    | Choice2Of2 x -> failwithf "Error while downloading members. %s" x

Target "DownloadNews" <| fun () ->
    News.download facebookAccessToken
    |> Choice.bind ((List.map (News.downloadImages imageBaseDir)) >> Choice.ofList)
    |> Choice.map (List.map News.getJson)
    |> Choice.map (saveEntries (dataDir @@ "news.json"))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded news."
    | Choice2Of2 x -> failwithf "Error while downloading news. %s" x

Target "DownloadNpmDependencies" <| fun () ->
    let setParams (x: NpmParams) =
        { x with
            Command = Install Standard
            WorkingDirectory = mainProjectDir
        }
    Npm setParams

Target "Build" <| fun () ->
    let setParams (p: MSBuildParams) =
        { p with
            Targets = ["Build"]
            Properties =
                [
                    "Configuration", "Release"
                ]
        }
    build setParams slnFile

Target "ResizeImages" <| fun () ->
    let resize sourcePath (width, height) targetPath =
        use image = Image.FromFile sourcePath
    
        let destRect = new Rectangle(0, 0, width, height)
        use destImage = new Bitmap(width, height)

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution)

        use graphics = Graphics.FromImage destImage
        graphics.CompositingMode <- CompositingMode.SourceCopy
        graphics.CompositingQuality <- CompositingQuality.HighQuality
        graphics.InterpolationMode <- InterpolationMode.HighQualityBicubic
        graphics.SmoothingMode <- SmoothingMode.HighQuality
        graphics.PixelOffsetMode <- PixelOffsetMode.HighQuality

        let ratio =
            let widthRatio = (float image.Width) / (float width)
            let heightRatio = (float image.Height) / (float height)
            Math.Min (widthRatio, heightRatio)

        let srcWidth = (float width) * ratio |> int
        let srcHeight = (float height) * ratio |> int
        let srcX = (image.Width - srcWidth) / 2
        let srcY = (image.Height - srcHeight) / 2

        use wrapMode = new ImageAttributes()
        wrapMode.SetWrapMode(WrapMode.TileFlipXY)
        graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel, wrapMode)

        ensureDirectory (directory targetPath)
        destImage.Save targetPath
    
    resizeDefinitionFilePath
    |> ReadFile
    |> Seq.map ResizeDefinition.Parse
    |> Seq.distinctBy (fun def -> def.Path, def.Width, def.Height)
    |> Seq.iter (fun def ->
        tracefn "Resizing %s to Width = %d, Height = %d" def.Path def.Width def.Height

        let fileName = sprintf "%s_%dx%d%s" (fileNameWithoutExt def.Path) def.Width def.Height (ext def.Path)
        let targetPath = outputDir @@ (directory def.Path) @@ fileName
        try
            resize (mainProjectDir @@ def.Path) (def.Width, def.Height) targetPath
        with e -> eprintfn "Can't resize %s: %O" def.Path e; reraise()
    )

Target "CopyAssets" <| fun () ->
    !! ("assets/**/*")
    -- ("assets/" + resizeDefinitionFileName)
    -- ("assets/images/*/**/*.*")
    ++ ("node_modules/slick-carousel/slick/slick.min.js")
    ++ ("node_modules/slick-carousel/slick/slick.css")
    ++ ("node_modules/slick-carousel/slick/slick-theme.css")
    ++ ("node_modules/slick-carousel/slick/ajax-loader.gif")
    ++ ("node_modules/slick-carousel/slick/fonts/slick.woff")
    ++ ("node_modules/slick-carousel/slick/fonts/slick.ttf")
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
    Ftp.uploadDirectory outputDir uploadUrl uploadCredentials
    |> function
    | Choice1Of2 () -> printfn "Successfully uploaded build"
    | Choice2Of2 message -> failwith "Error while uploading build: One or more Ftp requests failed"

Target "Default" DoNothing

"DownloadMembers" <== ["Clean"]
"DownloadNews" <== ["Clean"]
"DownloadNpmDependencies" <== ["Clean"]
"Build" <== ["DownloadMembers"; "DownloadNews"; "DownloadNpmDependencies"]
"ResizeImages" <== ["Build"]
"CopyAssets" <== ["Build"]
"AddHtAccessFile" <== ["Build"]
"FullBuild" <== ["ResizeImages"; "CopyAssets"; "AddHtAccessFile"]
"Upload" <== ["FullBuild"]
"Default" <== ["Upload"]

RunTargetOrDefault "Default"
