#r "System.Net.Http"

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Web

let getContentString (response: HttpResponseMessage) =
    let srcEncoding = Encoding.GetEncoding "ISO-8859-1"
    let targetEncoding = Encoding.UTF8
    let content = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask |> Async.RunSynchronously
    Encoding.Convert(srcEncoding, targetEncoding, content)
    |> targetEncoding.GetString

let sendRequest (request: HttpRequestMessage) =
    use client = new HttpClient(Timeout=TimeSpan.FromMinutes 10.0)
    try
        let response = client.SendAsync request |> Async.AwaitTask |> Async.RunSynchronously
        if response.IsSuccessStatusCode
        then Choice1Of2 response
        else Choice2Of2 (sprintf "Server returned %d (%O)" (int response.StatusCode) response.StatusCode)
    with :? HttpRequestException as e -> Choice2Of2 e.Message

let get (uri: Uri) =
    use request = new HttpRequestMessage(HttpMethod.Get, uri)
    sendRequest request

let post (uri: Uri) content =
    use request = new HttpRequestMessage(HttpMethod.Post, uri)
    request.Content <- content
    sendRequest request

let postForm (uri: Uri) formParams =
    let urlEncode (x: string) =
        HttpUtility.UrlEncode x

    let content =
        formParams
        |> Seq.map (fun (key: string, value: string) -> sprintf "%s=%s" (urlEncode key) (urlEncode value))
        |> String.concat "&"
    use request = new HttpRequestMessage(HttpMethod.Post, uri)
    request.Content <- new StringContent(content)
    request.Content.Headers.ContentType <- MediaTypeHeaderValue "application/x-www-form-urlencoded"
    sendRequest request

let postXml (uri: Uri) xml =
    use request = new HttpRequestMessage(HttpMethod.Post, uri)
    request.Content <- new StringContent(xml)
    request.Content.Headers.ContentType <- MediaTypeHeaderValue "application/xml"
    sendRequest request
