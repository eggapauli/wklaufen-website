namespace WkLaufen.Website.Extensions

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.InterfaceGenerator

module Definition =
    let EventConfig =
        Pattern.Config "EventConfig"
            {
                Optional =
                    [
                        "bubbles", T<bool>
                        "cancelable", T<bool>
                    ]
                Required = []
            }

    let Event =
        Class "Event2"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor (T<string>?typeArg * EventConfig?config) |> WithInline "new Event($typeArg, $config)"
        ]

    let Element =
        Class "Element2"
        |=> Inherits T<Dom.Element>
        |+> Instance [
            "querySelector" => (T<string> ^-> T<Dom.Element>)
            "querySelectorAll" => (T<string> ^-> T<Dom.NodeList>)
            "innerHTML" =@ T<string>
        ]

    let AElement =
        Class "AElement"
        |=> Inherits Element
        |+> Instance [
            "protocol" =? T<string>
            "hostname" =? T<string>
            "port" =? T<string>
            "pathname" =? T<string>
            "search" =? T<string>
            "hash" =? T<string>
            "host" =? T<string>
        ]

    let Document =
        Class "Document2"
        |=> Inherits T<Dom.Document>
        |+> Instance [
            "querySelector" => (T<string> ^-> T<Dom.Element>)
            "querySelectorAll" => (T<string> ^-> T<Dom.NodeList>)
            "title" =@ T<string>
        ]

    let Window =
        Class "Window2"
        |=> Inherits T<Window>
        |+> Instance [
            "addEventListener" => ((T<string> * T<Dom.Event -> unit>) ^-> T<unit>)
        ]

    let JQuerySlickConfig =
        Pattern.Config "JQuerySlickConfig"
            {
                Optional =
                    [
                        "draggable", T<bool>
                        "infinite", T<bool>
                        "adaptiveHeight", T<bool>
                    ]
                Required = []
            }

    let JQuerySlick =
        Class "Slick"
        |+> Static [
            "init" => ((T<JQuery>?elem * JQuerySlickConfig?config) ^-> T<unit>) |> WithInline "$elem.slick($config)"
            "do" => ((T<JQuery>?elem * T<string>?command * T<int>?slide * T<bool>?dontAnimate) ^-> T<unit>) |> WithInline "$elem.slick($command, $slide, $dontAnimate)"
        ]

    let TooltipsterConfig =
        Pattern.Config "TooltipsterConfig"
            {
                Optional =
                    [
                        "content", T<string>
                        "contentAsHTML", T<bool>
                        "trigger", T<string>
                        "side", T<string[]>
                        "theme", T<string[]>
                    ]
                Required = []
            }

    let TooltipsterStatus =
        Pattern.Config "TooltipsterStatus"
            {
                Optional = []
                Required =
                    [
                        "destroyed", T<bool>
                        "Destroying", T<bool>
                        "Enabled", T<bool>
                        "Open", T<bool>
                        "State", T<string>
                    ]
            }

    let Tooltipster =
        Class "Tooltipster"
        |+> Static [
            "create" => (T<JQuery>?target * TooltipsterConfig?config ^-> TSelf) |> WithInline "$target.tooltipster($config)"
            "open" => (T<JQuery>?target ^-> T<unit>) |> WithInline "$target.tooltipster('open')"
            "destroy" => (T<JQuery>?target ^-> T<unit>) |> WithInline "$target.tooltipster('destroy')"
            "enable" => (T<JQuery>?target ^-> T<unit>) |> WithInline "$target.tooltipster('enable')"
            "disable" => (T<JQuery>?target ^-> T<unit>) |> WithInline "$target.tooltipster('disable')"
            "status" => (T<JQuery>?target ^-> TooltipsterStatus) |> WithInline "$target.tooltipster('status')"
            "setContent" => (T<JQuery>?target * T<string>?content ^-> T<unit>) |> WithInline "$target.tooltipster('content', $content)"
        ]

    let MomentInstance =
        Class "MomentInstance"

    let Moment =
        Class "Moment"
        |+> Static [
            "locale" => (T<string>?locale ^-> T<unit>) |> WithInline "window.moment.locale($locale)"
            "moment" => (T<unit>?dummy ^-> MomentInstance) |> WithInline "window.moment()"
            "moment" => (T<int>?year * T<int>?month * T<int>?day ^-> MomentInstance) |> WithInline "window.moment([$year, $month, $day])"
            "fromNow" => (MomentInstance?m ^-> T<string>) |> WithInline "$m.fromNow(true)"
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.JavaScript" [
                Window
            ]
 
            Namespace "WebSharper.JavaScript.Dom" [
                EventConfig
                Event
                Element
                Document
                AElement
            ]

            Namespace "WebSharper.JQuery" [
                JQuerySlickConfig
                JQuerySlick
            ]

            Namespace "ThirdParty" [
                TooltipsterConfig
                TooltipsterStatus
                Tooltipster

                Moment
                MomentInstance
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
