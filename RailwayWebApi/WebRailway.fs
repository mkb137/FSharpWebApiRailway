module WebRailway

open System.Net
open Microsoft.AspNetCore.Mvc

// Data passed between our web pipelines.
type PipelineMessage<'a> =
    // The result can be a data payload to be passed down the pipeline
    | Continuation of 'a
    // ...or a response to be returned.
    | Response of IActionResult

// A response with a code
let response (code: HttpStatusCode) = Response(StatusCodeResult((int) code))

// A response with a code and a message.
let response' (code: HttpStatusCode) (message: string) =
    Response
        (ContentResult
            (StatusCode = Option.toNullable (Some((int) code)), Content = message, ContentType = "text/plain"))

// A helper method to create an async response from an IActionResult
let respond (result: IActionResult): Async<PipelineMessage<'a>> = async.Return(Response(result))

// A helper method to create an async response from a status code.
let respond' (code: HttpStatusCode): Async<PipelineMessage<'a>> = async.Return(response code)

// A helper method to create an async response from a status code and message.
let respond'' (code: HttpStatusCode) (message: string): Async<PipelineMessage<'a>> =
    async.Return(response' code message)

// A helper method to create an async continuation from a value.
let inline continuation (x: 'a): Async<PipelineMessage<'a>> = async.Return(Continuation(x))

// Monadic bind
// Takes a function and some async Result value.
// If the value represents a Success result, calls the function with the value.
// If the value represents a Failure result, returns the response directly.
let bind (f: 'a -> Async<PipelineMessage<'b>>) (a: Async<PipelineMessage<'a>>) =
    async {
        // Get the value of "a"
        let! p = a
        match p with
        // If a response, return the result directly (casting it to the output type).
        | Response f -> return Response(f)
        // If a success...
        | Continuation s ->
            // Call the function, getting a new result
            let r = f s
            // Return the result asynchronously
            return! r
    }

// Monadic compose
// This chains two functions together.
// If the first returns a Continuation, it will be passed to the second and that value will be returned.
// If the first returns a Response, it will shortcut and return the Response without calling the second.
let compose (first: 'a -> Async<PipelineMessage<'b>>) (second: 'b -> Async<PipelineMessage<'c>>): 'a -> Async<PipelineMessage<'c>> =
    fun x -> bind second (first x)

// Monadic try
// Given two functions:
// If the first function returns a Continuation, it will be returned (to presumably be continued to the function after it in the pipeline).
// If the first function returns a Response, the second function will be tried.
// Naturally, both functions must have the same signature.
let inline tryThen (a: 'a -> Async<PipelineMessage<'b>>) (b: 'a -> Async<PipelineMessage<'b>>): 'a -> Async<PipelineMessage<'b>> =
    fun x ->
        async {
            let! e = a x
            match e with
            | Response _ -> return! b x
            | r -> return r
        }

// A non-async version of tryThen
let inline concatenate a b =
    fun x ->
        match a x with
        | Response _ -> b x
        | r -> r

// Lets us execute some code that doesn't return a value without interrupting the flow.
let tap (f: 'a -> Async<unit>) =
    fun x ->
        async {
            match x with
            | Continuation c -> do! f c
            | _ -> ()
            return x
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
