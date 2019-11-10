
[<AutoOpen>]
module Operators

open System.IO

let (@@) a b = Path.Combine(a, b)

let flip fn a b = fn b a
