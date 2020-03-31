module AsyncResult

type Result<'TSuccess,'TFailure> =
    | Ok of 'TSuccess
    | Error of 'TFailure
    
// Async bind
// See https://github.com/fsharp/fsharp/blob/master/src/fsharp/FSharp.Core/result.fs
let bind f x = async {
    let! x' = x
    match x' with
    | Error e -> return Error e
    | Ok x'' -> return! f x''
}
    
// Like bind, but where the function doesn't care about the OK argument.
let bind' f x = async {
    let! x' = x
    match x' with
    | Error e -> return Error e
    | Ok _ -> return! f
}

// Async map
// See https://github.com/fsharp/fsharp/blob/master/src/fsharp/FSharp.Core/result.fs
let map f x = async {
    let! x' = x
    match x' with
    | Error e -> return Error e
    | Ok x'' ->
        let! r = f x''
        return Ok( r )
}

// Like map, but doesn't care about the input value.
let map' f x = async {
    let! x' = x
    match x' with
    | Error e -> return Error e
    | Ok _ ->
        let! r = f
        return Ok( r )
}

// Async mapError
// See https://github.com/fsharp/fsharp/blob/master/src/fsharp/FSharp.Core/result.fs
let mapError f x = async {
    let! x' = x
    match x' with
    | Error e ->
        let! r = f e
        return Error( r )
    | Ok ok ->
        return Ok ok
}

// Async compose
let compose f1 f2 =
    fun x -> bind f2 (f1 x)

// Like compose, but where the second function doesn't care about the OK argument.
let compose' f1 f2 =
    fun x -> bind' f2 (f1 x)
    
// Async tryThen
let tryThen f1 f2 =
    fun x ->
        async {
            let! r = f1 x
            match r with
            | Error _ -> return! f2 x
            | ok -> return ok
        }

// Async tap
let tap f =
    fun x ->
        async {
            match x with
            | Ok ok -> do! f ok
            | _ -> ()
            return x
        }

// bind operator
let (>>=) a b =
    bind b a

// compose operator
let (>=>) a b =
    compose a b

// compose' operator
let (>->) a b =
    compose' a b

// tryThen operator
let inline (<|>) a b =
    tryThen a b

// If Error and OK are of the same type, returns the enclosed value.
let coalesce r = async {
    let! r' = r
    match r' with
    | Error e -> return e
    | Ok ok -> return ok
}