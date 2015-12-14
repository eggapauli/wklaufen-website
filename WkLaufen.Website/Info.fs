namespace WkLaufen.Website

open System
open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.Html.Client


module Constants =
    open WebSharper.Core.Macros

    type TimeStampGenerator() =
        interface IGenerator with
            member this.Body =
                let result = DateTime.Now.Ticks.ToString()
                <@@ fun () -> result @@>
                |> QuotationBody

    [<Generated(typeof<TimeStampGenerator>)>]
    let TimeStamp() = X<string>

[<JavaScript>]
module Array =
    let fromNodeList (nodes: Dom.NodeList) =
        Seq.init nodes.Length nodes.Item
        |> Seq.cast<Dom.Element>
        |> Seq.toArray

[<JavaScript>]
module Info =
    let private contentSelector = ".content-background"

    let private visibilityCssClass = "visible"
    let private pageIdAttributeName = "data-id"
    let private windowTitleAttributeName = "data-window-title"

    let private getBackground() =
        JQuery.Of "body > div.info-background"

    let private getAllLoadedContent() =
        sprintf "%s[%s]" contentSelector pageIdAttributeName
        |> JQuery.Of

    let private getLoadedContent dataId =
        sprintf "%s[%s=\"%s\"]" contentSelector pageIdAttributeName dataId
        |> JQuery.Of

    let private getMainContent() =
        JQuery.Of(contentSelector)
            .Filter(fun item ->
                item.HasAttribute pageIdAttributeName |> not
            )
            .First()

    let private showInfo dataId =
        getAllLoadedContent()
            .Add(getMainContent())
            .Each(fun item -> JQuery.Of(item).RemoveClass(visibilityCssClass).Ignore)
            .Ignore

        let content = getLoadedContent dataId
        getBackground()
            .Add(content)
            .Each(fun item -> JQuery.Of(item).AddClass(visibilityCssClass).Ignore)
            .Ignore

        (JS.Document :?> Dom.Document2).Title <- content.Attr windowTitleAttributeName

        content

    let private closeInfo() =
        let background = getBackground()
        let content = getAllLoadedContent().Filter(fun c -> JQuery.Of(c).HasClass visibilityCssClass)
        let mainContent = getMainContent()

        content
            .Add(background)
            .Each(fun item -> JQuery.Of(item).RemoveClass(visibilityCssClass).Ignore)
            .Ignore

        mainContent.AddClass(visibilityCssClass).Ignore
        (JS.Document :?> Dom.Document2).Title <- mainContent.Attr windowTitleAttributeName
        
        let event = Dom.Event2("close-info", Dom.EventConfig(Bubbles = true))
        content.Each(fun c -> c.DispatchEvent event |> ignore).Ignore

    let private getOrCreateBackground() =
        match getBackground() with
        | x when x.Length = 0 ->
            JQuery.Of("<div></div>")
                .AddClass("info-background")
                .Click(fun el evt -> JS.Window.History.Back())
                .AppendTo("body")
        | x -> x
    
    let private prepareAndAddContent (content: JQuery) dataId windowTitle =
        content
            .AddClass("overlay")
            .Attr(pageIdAttributeName, dataId)
            .Attr(windowTitleAttributeName, windowTitle)
            .InsertAfter(getMainContent())
            .Ignore
    
    let private getNewNodes (root: Dom.Element) (selector: string) idFn =
        let throwIfAnyNodeHasNoId (nodes: Dom.Element array) =
            nodes
            |> Array.exists (fun n -> idFn n = JS.Undefined)
            |> function
            | true -> failwith "One or more nodes don't have an id"
            | false -> ()
        
        let existingNodes =
            (JS.Document :?> Dom.Document2).QuerySelectorAll selector
            |> Array.fromNodeList
        throwIfAnyNodeHasNoId existingNodes
        
        let newNodes =
            (root :?> Dom.Element2).QuerySelectorAll selector
            |> Array.fromNodeList
        throwIfAnyNodeHasNoId newNodes
        
        newNodes
        |> Array.filter (fun newNode ->
            let newNodeId = idFn newNode
            existingNodes
            |> Array.forall(fun existingNode -> idFn existingNode <> newNodeId)
        )
    
    let private addReferences (root: Dom.Element) =
        let stylesheets = getNewNodes root "link[rel=\"stylesheet\"]" (fun n -> n.GetAttribute "href")
        let stylesheetPromises =
            stylesheets
            |> Array.map (fun node ->
                Async.FromContinuations <| fun (ok, _, _) ->
                    JQuery
                        .Of(node.CloneNode false)
                        .On("load", fun elem evt -> ok())
                        .AppendTo("head")
                        .Ignore
            )
            |> Array.toList
        
        let scripts = getNewNodes root "script" (fun n -> n.GetAttribute "src")
        let scriptPromises =
            scripts
            |> Array.map (fun node ->
                Async.FromContinuations <| fun (ok, fail, _) ->
                    let url = node.GetAttribute "src"
                    JQuery.GetScript(url)
                        .Done(fun _ -> ok())
                        .Fail(fun x -> sprintf "Ajax request to \"%s\" failed" url |> exn |> fail)
                    |> ignore
            )
            |> Array.toList

        Async.Parallel (stylesheetPromises @ scriptPromises)
        |> Async.Ignore
    
    let private load url =
        let content = getLoadedContent url
        if content.Length > 0
        then 
            async { return content }
        else
            async {
                let! response = Async.FromContinuations <| fun (ok, ko, _) ->
                    JQuery.Ajax(
                        JQuery.AjaxSettings(
                            Url = url + "?rand=" + Constants.TimeStamp(),
                            Type = As<JQuery.RequestType> "GET",
                            ContentType = "text/html",
                            Success = (fun (result, _, _) -> ok (result :?> string)),
                            Error = (fun (jqXHR, _, _) -> ko (System.Exception(jqXHR.ResponseText)))))
                    |> ignore
                let root = JS.Document.CreateElement "html"
                (root :?> Dom.Element2).InnerHTML <- response

                do! addReferences root

                let background = getOrCreateBackground()
                let content =
                    (root :?> Dom.Element2).QuerySelector contentSelector
                    |> JQuery.Of
                content.RemoveClass(visibilityCssClass).Ignore
                let windowTitle = (root :?> Dom.Element2).QuerySelector("head title").TextContent
                prepareAndAddContent content url windowTitle
                return content
            }

    let private loadAndShowImpl url = async {
        let! content = load url
        do! Async.Sleep 50
        return showInfo url
    }
    
    let init() =
        getMainContent()
            .Attr(windowTitleAttributeName, (JS.Document :?> Dom.Document2).Title)
            .Ignore

        (JS.Window :?> Window2).AddEventListener("popstate", fun (e: Dom.Event) ->
            closeInfo()
            let dataId = JS.Window.Location.Hash
            if String.IsNullOrEmpty dataId
            then
                e.PreventDefault()
            else
                async {
                    let url = dataId.Substring 1
                    try
                        do! loadAndShowImpl url |> Async.Ignore
                    with x -> () //console.error("Failed to load page", url, x));
                }
                |> Async.Start
        )
    
    let loadAndShow url = async {
        let loadedContent = getLoadedContent url
        if loadedContent.HasClass visibilityCssClass
        then return loadedContent
        else
            let! content = loadAndShowImpl url
            JS.Window.History.PushState(JS.Undefined, JS.Undefined, "#" + url)
            return content
    }
    
    let loadPageFromUrl() =
        let hash = JS.Window.Location.Hash
        if String.IsNullOrEmpty hash |> not
        then
            JS.Window.History.ReplaceState(JS.Undefined, JS.Undefined, JS.Window.Location.Pathname)
            loadAndShow(hash.Substring 1)
        else
            async { return JQuery.Of([||]) }
