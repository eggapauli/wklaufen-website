namespace website_fsharp

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

    let Main() =
        Info.init()

        let initCarousels (root: JQuery) =
            let carousels = JQuery.Of(".carousel:not(.slick-initialized)", root)
            // Width might not be calculated correctly
            // with a visible scrollbar
            carousels.Css("overflow", "hidden").Ignore
            Slick.Init(carousels, JQuerySlickConfig(Draggable=false, Infinite=false, AdaptiveHeight=true))
            carousels.Css("overflow", "").Ignore

        let rec rewriteMenuItemLinks(root: JQuery) =
            JQuery.Of("a.info-link", root)
                .Each(fun elem ->
                    let jqElem = JQuery.Of elem
                    jqElem.Click(fun _ evt ->
                        evt.PreventDefault()
                        jqElem.AddClass("loading").Ignore
                        let url = jqElem.Attr "href"
                        async {
                            try
                                try
                                    let! content = Info.loadAndShow url
                                    initPage content
                                with x -> () //console.error("Failed to load page", url, x)
                            finally
                                jqElem.RemoveClass("loading").Ignore
                        }
                        |> Async.Start
                    ).Ignore
                ).Ignore

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
    
        JQuery.Of ".content-background" |> initPage
        async {
            try
                let! content = Info.loadPageFromUrl()
                initPage content
            with x -> () // console.error("Failed to load page from url", x));
        }
        |> Async.Start

        Span []

