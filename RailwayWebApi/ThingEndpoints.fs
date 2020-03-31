module ThingEndpoints

open System.Data
open System.Net
open System.Net
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc
open WebRailway

type Thing =
    { Id: int
      Name: string }

// Opens a database connection.
let openConnection(): Async<IDbConnection> =
    async {
        // Fake it.  We won't attempt to use it in this demo.
        return null }

// Gets the user's role
let getRole (connection: IDbConnection) (context: HttpContext) =
    async {
        printfn "getRole"
        // Pretend we got this from the DB.
        // For testing purposes, if the Id is X99 pretend the user doesn't have access.
        if context.Request.Path.Value.EndsWith("99") then return Some "user"
        else return Some "admin"
    }

let ensureUserHasRole (connection: IDbConnection) (role: string) (context: HttpContext) =
    async {
        printfn "ensureUserHasRole - role = '%s'" role
        // Get the user's roles
        let! userRole = getRole connection context
        match userRole with
        // If the user's role matches the given role, continue.
        | Some r when r = role -> return Continuation(context)
        // If the user's role does not match the given role, the user is unauthorized.
        | _ -> return (response HttpStatusCode.Unauthorized)
    }

let responseOrError (message: Async<PipelineMessage<'a>>) =
    async {
        // Get the message
        let! m = message
        printfn "responseOrError - m = %A" m
        match m with
        // If a response, return it.
        | Response r -> return r
        // We have nothing left to handle a continuation so return an error.
        | _ -> return (StatusCodeResult((int) HttpStatusCode.InternalServerError) :> IActionResult)
    }

// Fetches a thing from the store by its Id
let fetchThingById (connection: IDbConnection) (thingId: int) (ignoredContinuationData) =
    async {
        printfn "fetchThingById"
        match thingId with
        | 0 ->
            // Pretend we couldn't find it.
            return Continuation(None)
        | _ ->
            // Pretend we got this from the DB
            return Continuation (Some ( { Id = thingId; Name = "test" } ) )
    }



// If the given object has a value, continues.  If not, returns 404 NotFound
let ensureFound (value: 'a option) =
    printfn "ensureFound - value = %A" value
    match value with
    | Some s -> continuation s
    | None -> respond' HttpStatusCode.NotFound

let formatResult (value: 'a) =
    async {
        printfn "formatResult - value = %A" value
        return Response(JsonResult(value))
    }

// GET /thing/{thingId}
let getThing (thingId: int) (context: HttpContext): Async<IActionResult> =
    async {
        // Create a DB connection
        let! connection = openConnection()
        let response =
            ((continuation context) // Seed the pipeline with the HTTP context
             // Create the response pipeline
             >>= (
             // Ensure the user has the given role
             (ensureUserHasRole connection "admin")
             >=> (fetchThingById connection thingId)
             >=> ensureFound
             >=> formatResult)
             // If we don't have a response by this point, return an error.
             |> responseOrError)
        // Return the final response
        return! response
    }
