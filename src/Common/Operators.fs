
[<AutoOpen>]
module Operators

open System.IO

let (@@) a b = Path.Combine(a, b)
