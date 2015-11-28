namespace WkLaufen.Website.Extensions

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.InterfaceGenerator

module Definition =
    let Event =
        Class "Event2"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor T<string>?typeArg |> WithInline "new Event($typeArg)"
        ]

    let Element =
        Class "Element2"
        |=> Inherits T<Dom.Element>
        |+> Instance [
            "querySelector" => (T<string> ^-> T<Dom.Element>)
            "querySelectorAll" => (T<string> ^-> T<Dom.NodeList>)
            "innerHTML" =@ T<string>
        ]

    let Document =
        Class "Document2"
        |=> Inherits T<Dom.Document>
        |+> Instance [
            "querySelector" => (T<string> ^-> T<Dom.Element>)
            "querySelectorAll" => (T<string> ^-> T<Dom.NodeList>)
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

    let Assembly =
        Assembly [
            Namespace "WebSharper.JavaScript" [
                Window
            ]
 
            Namespace "WebSharper.JavaScript.Dom" [
                 Event
                 Element
                 Document
            ]

            Namespace "WebSharper.JQuery" [
                JQuerySlickConfig
                JQuerySlick
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
