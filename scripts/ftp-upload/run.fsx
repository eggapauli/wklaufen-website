#load @"..\common\Choice.fsx"
#load @"..\common\CommandLine.fsx"
#load @"..\common\Ftp.fsx"

open System

match CommandLine.getArg "upload-dir", CommandLine.getArg "upload-url", CommandLine.getArg "username", CommandLine.getArg "password" with
| Some uploadDir, Some uploadUrl, Some username, Some password ->
    Some (uploadDir, Uri uploadUrl, { Ftp.Credential.Username = username; Ftp.Credential.Password = password })
| _ -> None
|> Choice.ofOption "One or more of the following arguments couldn't be found: \"upload-dir\", \"upload-url\", \"username\", \"password\""
|> Choice.bind(fun (uploadDir, uploadUrl, credentials) ->
    Ftp.uploadDirectory uploadDir uploadUrl credentials
    |> Choice.mapError (fun () -> "One or more Ftp requests failed")
)
|> function
| Choice1Of2 () -> printfn "Successfully uploaded build"
| Choice2Of2 message -> failwithf "Error while uploading build: %s" message
