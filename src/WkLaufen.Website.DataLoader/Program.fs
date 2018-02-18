module Main

open System
open System.IO
open SixLabors.ImageSharp

let (@@) a b = System.IO.Path.Combine(a, b)

let equalsIgnoreCase (s1: string) s2 =
    s1.Equals(s2, StringComparison.OrdinalIgnoreCase)

let downloadMembers credentials dataDir imageBaseDir =
    Directory.CreateDirectory dataDir |> ignore

    Members.download credentials
    |> Async.bind (
        Choice.bindAsync (
            List.map (Members.tryDownloadImage imageBaseDir)
            >> Async.ofList
            >> (Async.map Choice.ofList)
        )
    )
    |> Async.RunSynchronously
    |> Choice.map Members.serialize
    |> Choice.map (fun s -> File.WriteAllText(dataDir @@ "Members.fs", s))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded members."
    | Choice2Of2 x -> failwithf "Error while downloading members. %s" x

let downloadNews accessToken dataDir imageBaseDir =
    News.download accessToken
    |> Async.bind (Choice.bindAsync ((List.map (News.downloadImages imageBaseDir)) >> Async.ofList >> Async.map Choice.ofList))
    |> Async.RunSynchronously
    |> Choice.map News.serialize
    |> Choice.map (fun s -> File.WriteAllText(dataDir @@ "News.fs", s))
    |> function
    | Choice1Of2 () -> printfn "Successfully downloaded news."
    | Choice2Of2 x -> failwithf "Error while downloading news. %s" x

type ResizeOptions = {
    Width: int option
    Height: int option
    Crop: bool
}

let resizeImages sourceDir targetDir =
    let resize (sourcePath: string) resizeOptions (targetPath: string) =
        let computeSize desiredSize (imageWidth, imageHeight) =
            match desiredSize with
            | Some width, Some height ->
                width, height
            | Some width, None ->
                let ratio = (float imageWidth) / (float width)
                let height = Math.Round((float imageHeight) / ratio) |> int
                width, height
            | None, Some height ->
                let ratio = (float imageHeight) / (float height)
                let width = Math.Round((float imageWidth) / ratio) |> int
                width, height
            | None, None ->
                imageWidth, imageHeight

        let computeSizeNoCrop (desiredWidthOpt, desiredHeightOpt) (imageWidth, imageHeight) =
            let desiredWidth =
                match desiredWidthOpt with
                | Some x -> x
                | None -> imageWidth
            let desiredHeight =
                match desiredHeightOpt with
                | Some x -> x
                | None -> imageHeight

            let widthRatio = (float imageWidth) / (float desiredWidth)
            let heightRatio = (float imageHeight) / (float desiredHeight)
            let ratio = Math.Max(Math.Max(widthRatio, heightRatio), 1.)
            let width = Math.Round((float imageWidth) / ratio) |> int
            let height = Math.Round((float imageHeight) / ratio) |> int
            width, height

        use image = Image.Load sourcePath

        let width, height =
            let actualSize = (image.Width, image.Height)
            let desiredSize = (resizeOptions.Width, resizeOptions.Height)
            if resizeOptions.Crop
            then computeSize desiredSize actualSize
            else computeSizeNoCrop desiredSize actualSize

        printfn "Resizing %s to Width = %d, Height = %d" sourcePath width height

        // let resizeLayer =
        //     let widthRatio = (float image.Width) / (float width)
        //     let heightRatio = (float image.Height) / (float height)
        //     let resizeSize =
        //         if widthRatio > heightRatio
        //         then Drawing.Size(0, height)
        //         else Drawing.Size(width, 0)
        //     Imaging.ResizeLayer resizeSize
        // image.Resize resizeLayer |> ignore

        let cropRect =
            let srcX = (image.Width - width) / 2
            let srcY = (image.Height - height) / 2
            SixLabors.Primitives.Rectangle(srcX, srcY, width, height)

        image.Mutate(fun x -> x.Resize(width, height).Crop(cropRect) |> ignore)

        image.Save targetPath |> ignore

    let defaultResizeOptions = {
        Width = None
        Height = None
        Crop = true
    }

    let fileHasExtension extensions file =
        let fileExt = Path.GetExtension file
        extensions
        |> Seq.exists (fun ext -> equalsIgnoreCase ext fileExt) 

    let getImages dir resizeOptions =
        Directory.EnumerateFiles(sourceDir @@ dir)
        |> Seq.filter (fileHasExtension [ ".png"; ".jpg"; ".jpeg" ])
        |> Seq.map (fun f -> f, resizeOptions)

    [
        yield! getImages "menu-items" { defaultResizeOptions with Width = Some 150; Height = Some 100 }
        yield! getImages "member-groups" { defaultResizeOptions with Width = Some 200; Height = Some 130 }
        yield! getImages "members" { defaultResizeOptions with Width = Some 200; Height = Some 270 }
        yield!
            [ "600.jpg"; "31180.jpg" ]
            |> List.map (fun f ->
                (sourceDir @@ "members" @@ f)
                , { defaultResizeOptions with Width = Some 110; Height = Some 160 }
            )
        yield! getImages "pages" { defaultResizeOptions with Width = Some 1000; Height = Some 600 }
        yield! getImages "news" { defaultResizeOptions with Width = Some 940; Height = Some 480; Crop = false }
    ]
    |> List.iter (fun (path, resizeOptions) ->
        let widthName = match resizeOptions.Width with Some x -> sprintf "w%d" x | None -> ""
        let heightName = match resizeOptions.Height with Some x -> sprintf "h%d" x | None -> ""

        let fileName = sprintf "%s_%s%s%s" (Path.GetFileNameWithoutExtension path) widthName heightName (Path.GetExtension path)
        let targetPath = targetDir @@ (Path.GetDirectoryName path |> Path.GetFileName) @@ fileName

        resize (sourceDir @@ path) resizeOptions targetPath
    )

let tryGetArg args name =
    args
    |> Seq.skipWhile (fun v -> equalsIgnoreCase v ("--" + name) |> not)
    |> Seq.truncate 2
    |> Seq.tryLast

[<EntryPoint>]
let main argv =
    match
        tryGetArg argv "root-dir",
        tryGetArg argv "data-dir",
        tryGetArg argv "image-dir",
        tryGetArg argv "deploy-dir",
        tryGetArg argv "ooebv-username",
        tryGetArg argv "ooebv-password",
        tryGetArg argv "facebook-access-token" with
    | Some rootDir, Some dataDir, Some imageDir, Some deployDir, Some ooebvUsername, Some ooebvPassword, Some facebookAccessToken ->
        let absDataDir = rootDir @@ dataDir
        let absImageDir = rootDir @@ imageDir
        let absDeployDir = rootDir @@ deployDir
        downloadMembers (ooebvUsername, ooebvPassword) absDataDir absImageDir
        downloadNews facebookAccessToken absDataDir absImageDir
        resizeImages absImageDir absDeployDir
        0
    | _ ->
        eprintfn "Usage: dotnet run -- --root-dir <path> --data-dir <path> --image-dir <path> --deploy-dir <path> --ooebv-username <username> --ooebv-password <password> --facebook-access-token <access-token>"
        1
