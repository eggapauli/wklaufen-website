module GTag

open Fable.Core

let trackingId = "UA-123409256-2"

[<Emit("gtag('config', $0, {'page_path': $1})")>]
let setPage (gaTrackingId: string) (pagePath: string): unit = jsNative
