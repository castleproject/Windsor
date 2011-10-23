// Learn more about F# at http://fsharp.net

module Module1

open Castle.Transactions

let (|Option|) (maybe : _ Maybe) = if maybe.HasValue then Some(maybe.Value) else None