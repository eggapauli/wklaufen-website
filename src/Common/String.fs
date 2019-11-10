module String

open System

let equalsIgnoreCase (s1: string) s2 =
    s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase)
