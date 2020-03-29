module BlogDemos

open Railway



let notZero (x: int): Async<int option> =
    async {
        printfn "notZero - x = %d" x
        match x with
        | 0 -> return None
        | _ -> return Some(x)
    }

let positive (x: int): Async<int option> =
    async {
        printfn "positive - x = %d" x
        match x with
        | _ when x < 0 -> return None
        | _ -> return Some(x)
    }

let testBind() =
    async {
        let a = async.Return(Option<int>.Some 1)
        let b = async.Return(Option<int>.None)

        let! r1 = bind notZero a // r1 = Some 1
        let! r2 = bind notZero b // r2 = None - "notZero" is never called.
        printfn "r1 = %A, r2 = %A" r1 r2
    }

let testBind2() =
    async {
        let a = async.Return(Option<int>.Some 1)
        let b = async.Return(Option<int>.None)

        let! r1 = a >>= notZero // r1 = Some 1
        let! r2 = b >>= notZero // r2 = None - "notZero" is never called.
        printfn "r1 = %A, r2 = %A" r1 r2
    }

let testCompose() =
    async {
        let a = async.Return(Option<int>.Some 1)
        let b = async.Return(Option<int>.Some 0)
        let c = async.Return(Option<int>.Some -1)
        let d = async.Return(Option<int>.None)


        let! r1 = a >>= (compose notZero positive) // Some 1
        let! r2 = b >>= (compose notZero positive) // None - "notZero" and "positive" are called
        let! r3 = c >>= (compose notZero positive) // None - "notZero" is called
        let! r4 = d >>= (compose notZero positive) // None - neither are called
        printfn "r1 = %A, r2 = %A, r3 = %A, r4 = %A" r1 r2 r3 r4
    }

let testCompose2() =
    async {
        let a = async.Return(Option<int>.Some 1)
        let b = async.Return(Option<int>.Some 0)
        let c = async.Return(Option<int>.Some -1)
        let d = async.Return(Option<int>.None)


        let! r1 = a >>= (notZero >=> positive) // Some 1
        let! r2 = b >>= (notZero >=> positive) // None - "notZero" and "positive" are called
        let! r3 = c >>= (notZero >=> positive) // None - "notZero" is called
        let! r4 = d >>= (notZero >=> positive) // None - neither are called
        printfn "r1 = %A, r2 = %A, r3 = %A, r4 = %A" r1 r2 r3 r4
    }

let testTryThen() =
    async {
        printfn "testTryThen"
        let a = async.Return(Option<int>.Some 1)
        let b = async.Return(Option<int>.Some 0)
        let c = async.Return(Option<int>.Some -1)
        let d = async.Return(Option<int>.None)

        let! r1 = a >>= (tryThen notZero positive) // Some 1 "notZero" succeeds
        let! r2 = b >>= (tryThen notZero positive) // Some 0 "notZero" fails, "positive" succeeds
        let! r3 = c >>= (tryThen notZero positive) // Some -1 "notZero" succeeds
        let! r4 = d >>= (tryThen notZero positive) // None - both bypassed
        printfn "r1 = %A, r2 = %A, r3 = %A, r4 = %A" r1 r2 r3 r4
    }

let testTryThen2() =
    async {
        printfn "testTryThen2"
        let a = async.Return(Option<int>.Some 1)
        let b = async.Return(Option<int>.Some 0)
        let c = async.Return(Option<int>.Some -1)
        let d = async.Return(Option<int>.None)

        let! r1 = a >>= (notZero <|> positive) // Some 1 "notZero" succeeds
        let! r2 = b >>= (notZero <|> positive) // Some 0 "notZero" fails, "positive" succeeds
        let! r3 = c >>= (notZero <|> positive) // Some -1 "notZero" succeeds
        let! r4 = d >>= (notZero <|> positive) // None - both bypassed
        printfn "r1 = %A, r2 = %A, r3 = %A, r4 = %A" r1 r2 r3 r4
    }

let printValue (x: int) = async { printfn " print x = %d" x }


let testTee() =
    async {
        printfn "testTee"
        let a = async.Return(Option<int>.Some 1)
        let! r1 = a >>= (notZero >=> (tap printValue) >=> positive) // Some 1
        printfn " r1 = %A" r1
    }

let runAsync() =
    async {
        printfn "runAsync"
        do! testTryThen2()
    }

let run() =
    printfn "run"
    testTee() |> Async.RunSynchronously
