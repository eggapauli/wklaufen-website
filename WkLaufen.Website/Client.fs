namespace WkLaufen.Website

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.Html.Client

[<JavaScript>]
module Client =
    let CheckRedirect () =
        let expr = RegExp @"/([^/]+\.html(?:.*))$"
        match expr.Exec JS.Window.Location.Pathname with
        | null -> ()
        | urlMatch ->
            JS.Window.History.ReplaceState(JS.Undefined, JS.Undefined , sprintf "/#%s" urlMatch.[0])
            JS.Window.Location.Reload()
        Span []

    let BMF2017Register() =
        let rootId = "#bmf-2017-register"

        let toggleShowDay day show =
            let elems = JQuery.Of(sprintf ".show_on_%s" day, rootId)
            if show
            then elems.Show("slow").Ignore
            else elems.Hide("slow").Ignore

        let doc = JQuery.Of JS.Document

        doc
            .On("change", sprintf "%s input[name='participation-days[]']" rootId, fun s _ ->
                let sender = JQuery.Of s
                sender.Is(":checked")
                |> toggleShowDay (sender.Val() |> string)
            )
            .Ignore

        let getInputFields() =
            JQuery.Of("input[type!=submit],textarea", rootId)

        doc
            .On("submit", sprintf "%s form" rootId, fun form event ->
                event.PreventDefault()
                let submitButton = JQuery.Of("input[type=submit]", rootId)
                async {
                    try
                        submitButton
                            .Attr("disabled", "disabled")
                            .Ignore

                        let! response = Async.FromContinuations <| fun (ok, ko, _) ->
                            JQuery.Ajax(
                                JQuery.AjaxSettings(
                                    Url = JQuery.Of(form).Attr "action",
                                    Type = As<JQuery.RequestType> "POST",
                                    Data = JQuery.Of(form).Serialize(),
                                    ContentType = "application/x-www-form-urlencoded",
                                    Success = (fun (result, _, _) -> ok (result :?> string)),
                                    Error = (fun (jqXHR, _, _) -> ko (System.Exception jqXHR.ResponseText))
                                )
                            )
                            |> ignore
                        Console.Log ("Success", response)
                        JQuery.Of(".success", rootId)
                            .Show("slow")
                            .Ignore

                    with e ->
                        Console.Error ("Error", e.Message)

                        submitButton
                            .RemoveAttr("disabled")
                            .Ignore

                        try
                            let errorClass = "error"
                            getInputFields()
                                .RemoveClass(errorClass)
                                .Ignore

                            let response = Json.Deserialize e.Message
                            response
                            |> Map.iter (fun inputName error ->
                                let target = JQuery.Of(sprintf "input[name='%s']" inputName, rootId)
                                target.AddClass(errorClass).Ignore
                                ThirdParty.Tooltipster.SetContent(target, error)
                            )
                            let scrollContainer = JQuery.Of(".scroll-container", rootId)
                            let firstErrorTop = JQuery.Of("." + errorClass).First().Position().Top
                            let additionalOffset = 30
                            let scrollTop = scrollContainer.ScrollTop() + (int firstErrorTop) - additionalOffset
                            scrollContainer
                                .Animate(New ["scrollTop" => scrollTop])
                                .Ignore
                        with e -> Console.Log e
                }
                |> Async.Start
            )
            .Ignore

        JS.Document.AddEventListener("data-loaded", (fun (evt: Dom.Event) ->
            let config =
                ThirdParty.TooltipsterConfig(
                    Trigger = "click",
                    Theme = [| "tooltipster-shadow"; "tooltipster-error" |]
                )
            ThirdParty.Tooltipster.Create(getInputFields(), config) |> ignore
        ), false)

    let Main() =
        Info.init()

        let initCarousels (root: JQuery) =
            let carousels = JQuery.Of(".carousel:not(.slick-initialized)", root)
            // Width might not be calculated correctly
            // with a visible scrollbar
            carousels.Css("overflow", "hidden").Ignore
            Slick.Init(carousels, JQuerySlickConfig(Draggable=false, Infinite=false, AdaptiveHeight=true))
            carousels.Css("overflow", "").Ignore

        let rec rewriteMenuItemLink(elem: JQuery) =
            elem.Click(fun _ evt ->
                evt.PreventDefault()
                elem.AddClass("loading").Ignore
                let url = elem.Attr "href"
                async {
                    try
                        try
                            let! content = Info.loadAndShow url
                            initPage content
                        with x -> () //console.error("Failed to load page", url, x)
                    finally
                        elem.RemoveClass("loading").Ignore
                }
                |> Async.Start
            ).Ignore

        and rewriteMenuItemLinks(root: JQuery) =
            JQuery.Of("a", root)
                .Filter(fun elem -> (elem :?> Dom.AElement).Host = JS.Window.Location.Host)
                .Each(fun elem -> JQuery.Of elem |> rewriteMenuItemLink)
                .Ignore

        and initPage(root: JQuery) =
            let initializedCssClass = "initialized"
            if not <| root.HasClass initializedCssClass
            then
                root.AddClass(initializedCssClass).Ignore
                initCarousels root
                rewriteMenuItemLinks root
            ()
        ()

        JS.Document.AddEventListener("close-info", (fun (evt: Dom.Event) ->
            Slick.Do(JQuery.Of(".carousel", evt.Target :?> Dom.Element), "slickGoTo", 0, true)
        ), false)
    
        JQuery.Of ".impressum" |> rewriteMenuItemLink
        JQuery.Of ".content-background" |> initPage
        async {
            try
                let! content = Info.loadPageFromUrl()
                initPage content
            with x -> () // console.error("Failed to load page from url", x));
        }
        |> Async.Start

        BMF2017Register()

        Span []
