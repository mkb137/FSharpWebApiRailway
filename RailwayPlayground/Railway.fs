module Railway

// A value representing an async success.
// On success, turn a value into an async option value indicating the success of an async call.
let inline succeed (x: 'a): Async<'a option> = async.Return(Some x)

// A value representing an async failure.
// Returns an async option value of None, indicating the failure of an async call.
let fail<'a> : Async<'a option> = async.Return(Option<'a>.None)

// A function guaranteed to fail.
let never: 'a -> Async<'b option> = fun x -> fail

// Monadic bind
// Given a function and some async optional value, we unwrap the value and,
// if it's not None, pass it to the function.
// If the value is None, it shortcuts and returns None directly.
let bind (f: 'a -> Async<'b option>) (a: Async<'a option>) =
    async {
        // Get the value of "a"
        let! p = a
        match p with
        // If None, stop here, returning None.
        | None -> return None
        // If has a value..
        | Some q ->
            // Call the first function, "f"
            let r = f q
            // Return the result asynchronously
            return! r
    }

// Monadic compose
// This chains two functions together.
// If the first returns Some value, it will be passed to the second and that value will be returned.
// If the first returns None, it will shortcut and return None without calling the second.
let compose (first: 'a -> Async<'b option>) (second: 'b -> Async<'c option>): 'a -> Async<'c option> =
    fun x -> bind second (first x)


// Monadic try
// Given two functions:
// If the first function returns Some value, it will be returned.
// If the first function returns None, the second function will be tried.
// Naturally, both functions must have the same signature.
let inline tryThen (a: 'a -> Async<'b option>) (b: 'a -> Async<'b option>): 'a -> Async<'b option> =
    fun x ->
        async {
            let! e = a x
            match e with
            | None -> return! b x
            | r -> return r
        }

// A non-async version of tryThen
let inline concatenate a b =
    fun x ->
        match a x with
        | None -> b x
        | r -> r

// Lets us execute some code that doesn't return a value without interrupting the flow.
let tap (f: 'a -> Async<unit>) =
    fun x ->
        async {
            do! f (x)
            return Some(x)
        }

// Bind operator.  The operator indicates that we're feeding an optional value into some function.
let (>>=) a b =
    bind b a

// Compose operator.  The operator indicates that we're pipelining two functions together.
let (>=>) a b =
    compose a b

// TryThen operator
let inline (<|>) a b =
    tryThen a b

// Concatenate operator
let inline (@@) a b =
    concatenate a b
