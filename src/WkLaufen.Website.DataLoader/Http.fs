module Http

open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text

let getContentStringFromISO88591 (response: HttpResponseMessage) = async {
    let srcEncoding = Encoding.GetEncoding "ISO-8859-1"
    let targetEncoding = Encoding.UTF8
    let! content = response.Content.ReadAsByteArrayAsync() |> Async.AwaitTask
    return
        Encoding.Convert(srcEncoding, targetEncoding, content)
        |> targetEncoding.GetString
}

let getContentString (response: HttpResponseMessage) =
    response.Content.ReadAsStringAsync()
    |> Async.AwaitTask

let private contentToString = function
    | null -> "<empty>"
    | (content: HttpContent) ->
        content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously

let private requestToString (request: HttpRequestMessage) =
    [
        yield "## Request"
        yield sprintf "%O %O" request.Method request.RequestUri
        yield "### Headers"
        yield! request.Headers
            |> Seq.cast<System.Collections.Generic.KeyValuePair<string, string seq>>
            |> Seq.map (fun t -> sprintf "* %s: %s" t.Key (t.Value |> String.concat "|"))
        yield "### Content"
        yield contentToString request.Content
    ]
    |> String.concat Environment.NewLine

let sendRequest (request: HttpRequestMessage) = async {
    let responseToString (response: HttpResponseMessage) =
        [
            yield "## Response"
            yield sprintf "Status code: %d (%O)" (int response.StatusCode) response.StatusCode
            yield "### Content"
            yield contentToString response.Content
        ]
        |> String.concat Environment.NewLine

    use client = new HttpClient(Timeout=TimeSpan.FromMinutes 10.0)
    return!
        try async {
            let requestString = requestToString request
            let! response = client.SendAsync request |> Async.AwaitTask
            let responseString = responseToString response
            return
                if response.IsSuccessStatusCode
                then Choice1Of2 response
                else Choice2Of2 (requestString + System.Environment.NewLine + responseString)
            }
        with :? HttpRequestException as e -> async { return Choice2Of2 e.Message }
}

let get (uri: Uri) = async {
    use request = new HttpRequestMessage(HttpMethod.Get, uri)
    return! sendRequest request
}

let private createPostRequestMessage (uri: Uri) =
    new HttpRequestMessage(HttpMethod.Post, uri)

let post (uri: Uri) content = async {
    use request = createPostRequestMessage uri
    request.Content <- content
    return! sendRequest request
}

let postEmpty (uri: Uri) = async {
    use request = createPostRequestMessage uri
    return! sendRequest request
}

let postForm (uri: Uri) formParams = async {
    let urlEncode (x: string) =
        // HttpUtility.UrlEncode x
        Uri.EscapeDataString x

    let content =
        formParams
        |> Seq.map (fun (key: string, value: string) -> sprintf "%s=%s" (urlEncode key) (urlEncode value))
        |> String.concat "&"
    use request = createPostRequestMessage uri
    request.Content <- new StringContent(content)
    request.Content.Headers.ContentType <- MediaTypeHeaderValue "application/x-www-form-urlencoded"
    return! sendRequest request
}

let postXml (uri: Uri) xml = async {
    use request = createPostRequestMessage uri
    request.Content <- new StringContent(xml)
    request.Content.Headers.ContentType <- MediaTypeHeaderValue "application/xml"
    return! sendRequest request
}

let uploadImageMultipart (uri: Uri) (imagePath: string) = async {
    use request = createPostRequestMessage uri

    let content = new MultipartFormDataContent()
    let fileName = Path.GetFileName imagePath
    content.Add(new StringContent(fileName), "Filename")
    content.Add(new StringContent("*.jpg;*.jpeg;*.png;*.gif"), "fileext")
    content.Add(new StringContent("/uploads"), "folder")
    content.Add(new StreamContent(File.OpenRead(imagePath)), "Filedata", fileName)
    content.Add(new StringContent("Submit Query"), "Upload")
    request.Content <- content

    return! sendRequest request
}

let createClientWithCookies (cookies: Cookie list) =
    let cookieContainer = CookieContainer()
    cookies
    |> List.iter cookieContainer.Add
    let handler = new HttpClientHandler(CookieContainer = cookieContainer)
    new HttpClient(handler)
