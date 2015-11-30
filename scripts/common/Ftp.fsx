//#load "Choice.fsx"
//#load "retry.fsx"

open System
open System.IO
open System.Net
open System.Text.RegularExpressions

type Credential = {
    Username: string
    Password: string
}

let executeRequest (url: Uri) ftpMethod credential successStatus requestFn =
    printfn "Executing %s at %O" ftpMethod url
    let fn() =
        let request = WebRequest.Create url :?> FtpWebRequest
        request.Method <- ftpMethod
        request.Credentials <- NetworkCredential(credential.Username, credential.Password)
        requestFn request
        try
            let response = request.GetResponse()
            let ftpResponse = response :?> FtpWebResponse

            if ftpResponse.StatusCode <> successStatus
            then failwithf "Error while executing %s at %O: %s" ftpMethod url ftpResponse.StatusDescription

            ftpResponse
        with :? WebException as exn
            when (ftpMethod = WebRequestMethods.Ftp.DeleteFile
                || ftpMethod = WebRequestMethods.Ftp.RemoveDirectory)
                && (exn.Response :?> FtpWebResponse).StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable -> exn.Response :?> FtpWebResponse

    let retryCount = 10
    let timeout = TimeSpan.FromSeconds 10.
    Retry.executeExn (List.replicate retryCount timeout) fn

let uploadFile (dirUrl: Uri) credential filePath =
    let url = Uri(dirUrl, Path.GetFileName filePath)
    let requestFn (req: FtpWebRequest) =
        let fileContents =  File.ReadAllBytes filePath
        req.ContentLength <- int64 fileContents.Length

        do
            use requestStream = req.GetRequestStream()
            requestStream.Write(fileContents, 0, fileContents.Length)

    executeRequest url WebRequestMethods.Ftp.UploadFile credential FtpStatusCode.ClosingData requestFn

let executeSimpleRequest url ftpMethod credential successStatus =
    executeRequest url ftpMethod credential successStatus ignore

let createDirectory (url: Uri) credential =
    executeSimpleRequest url WebRequestMethods.Ftp.MakeDirectory credential FtpStatusCode.PathnameCreated

type DirectoryListingEntry = {
    IsDirectory: bool
    Name: string
}

let parseDirectoryListing row =
    let m = Regex.Match(row, "(?<=\d{2}:\d{2}\s+).*$")
    if m.Success
    then Some { IsDirectory = row.StartsWith "d"; Name = m.Value }
    else None

let rec deleteDirectory url credential =
    let cleanDirectory (url: Uri) credential =
        executeSimpleRequest url WebRequestMethods.Ftp.ListDirectoryDetails credential FtpStatusCode.OpeningData
        |> Choice.map (fun listResponse ->
            use reader = new StreamReader(listResponse.GetResponseStream())
            reader.ReadToEnd()
        )
        |> Choice.map (fun content ->
            content.Split([| "\r\n" |], StringSplitOptions.RemoveEmptyEntries)
            |> Array.toList
            |> List.choose parseDirectoryListing
            |> List.filter (fun x -> x.Name <> "." && x.Name <> "..")
            |> List.partition (fun row -> row.IsDirectory)
        )
        |> Choice.bind (fun (dirs, files) ->
            let deleteFileResults =
                files
                |> List.map (fun f ->
                    let url = Uri(url, f.Name)
                    executeSimpleRequest url WebRequestMethods.Ftp.DeleteFile credential FtpStatusCode.FileActionOK
                )

            let deleteDirResults =
                dirs
                |> List.map (fun d ->
                    let url = Uri(url, d.Name + "/")
                    deleteDirectory url credential
                )
            deleteFileResults @ deleteDirResults
            |> Choice.ofList
        )

    cleanDirectory url credential
    |> Choice.bind (fun _ ->
        executeSimpleRequest url WebRequestMethods.Ftp.RemoveDirectory credential FtpStatusCode.FileActionOK
    )

let uploadDirectory dirPath (url: Uri) credential =
    let rec uploadDir dirPath url credential =
        createDirectory url credential
        |> Choice.bind (fun _ ->
            Directory.GetFiles dirPath
            |> Seq.map (uploadFile url credential)
            |> Seq.toList
            |> Choice.ofList
        )
        |> Choice.bind (fun _ ->
            Directory.GetDirectories dirPath
            |> Seq.map (fun subDir ->
                let subDirUrl = Uri(url, sprintf "%s/" <| Path.GetFileName subDir)
                uploadDir subDir subDirUrl credential
            )
            |> Seq.toList
            |> Choice.ofList
        )
        |> Choice.map ignore

    let baseTmpUrl = Uri(url, "../upload/")
    deleteDirectory baseTmpUrl credential
    |> Choice.bind (fun _ -> createDirectory baseTmpUrl credential)
    |> Choice.bind (fun _ ->
        let tmpUrl = Uri(baseTmpUrl, sprintf "%O/" (Guid.NewGuid()))
        uploadDir dirPath tmpUrl credential
        |> Choice.bind (fun _ -> deleteDirectory url credential)
        |> Choice.bind (fun _ ->
            let renameSourceUrl = Uri(tmpUrl.ToString().TrimEnd('/'))
            let renameTargetName = url.Segments |> Array.last |> fun x -> x.Trim '/'
            let requestFn (r: FtpWebRequest) = r.RenameTo <- "../" + renameTargetName
            executeRequest renameSourceUrl WebRequestMethods.Ftp.Rename credential FtpStatusCode.FileActionOK requestFn
        )
    )
    |> Choice.map ignore
