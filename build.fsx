#r @"packages\FAKE\tools\FakeLib.dll"
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"

open System.Drawing
open System.Drawing.Drawing2D
open System.Drawing.Imaging
open Fake
open FSharp.Data

let mainProjectDir = "WkLaufen.Website"
let outputDir = mainProjectDir @@ "bin" @@ "html"

type ResizeDefinition = JsonProvider<"""{ "Path": "members\\image.jpg", "Width": 500, "Height": 1000 }""">

let resizeDefinitionFileName = "resize.txt"
let resizeDefinitionFilePath = mainProjectDir @@ "assets" @@ "resize.txt"

let slnFile = FindFirstMatchingFile "*.sln" "."
Target "Clean" <| fun () ->
    let setParams (p: MSBuildParams) =
        { p with
            Targets = ["Clean"]
            Properties =
                [
                    "Configuration", "Release"
                ]
        }
    build setParams slnFile

    DeleteFile resizeDefinitionFilePath

Target "Build" <| fun () ->
    let setParams (p: MSBuildParams) =
        { p with
            Targets = ["Rebuild"]
            Properties =
                [
                    "Configuration", "Release"
                ]
        }
    build setParams slnFile

Target "ResizeImages" <| fun() ->
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

        use wrapMode = new ImageAttributes()
        wrapMode.SetWrapMode(WrapMode.TileFlipXY)
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode)

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
        resize (mainProjectDir @@ def.Path) (def.Width, def.Height) targetPath
    )

Target "CopyAssets" <| fun() ->
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

Target "Default" DoNothing

"Build" <== ["Clean"]
"ResizeImages" <== ["Build"]
"CopyAssets" <== ["ResizeImages"]
"Default" <== ["CopyAssets"]

RunTargetOrDefault "Default"
