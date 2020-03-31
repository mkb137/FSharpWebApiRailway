module ThingEndpoints

open Microsoft.AspNetCore.Http
open System.Data
open System.Net
open AsyncResult
open Microsoft.AspNetCore.Mvc


type Thing =
    { Id: int
      Name: string }

// Opens a database connection.
let openConnection(): Async<IDbConnection> =
    async {
        // Fake it.  We won't attempt to use it in this demo.
        return null
    }

// Gets a user's role.
let getRole ( connection : IDbConnection ) ( context : HttpContext ) =
    async {
        // Pretend we got this from the DB.
        // For testing purposes, if the ID is *99 pretend the user doesn't have admin access.
        if context.Request.Path.Value.EndsWith("99") then return Ok "user"
        else return Ok "admin"
    }

// Checks that the user has the required role.
let ensureUserHasRole requiredRole userRole =
    async {
        if userRole = requiredRole then return Ok()
        else return Error( HttpStatusCode.Unauthorized )
    }
    
// Fetches a thing from the store by its Id
let fetchThingById (connection: IDbConnection) (thingId: int) () =
    async {
        match thingId with
        | 0 ->
            // Pretend we couldn't find it.
            return Ok( None )
        | _ ->
            // Pretend we got this from the DB
            return Ok( Some ( { Id = thingId; Name = "test" } ) )
    }
    
let ensureFound ( value : 'a option ) = async {
    match value with
    | Some value' -> return Ok( value' )
    | None -> return Error( HttpStatusCode.NotFound )
}

let toJsonResult ( value : 'a ) =
    async {
        return ( JsonResult( value ):> IActionResult )
    }    

let statusCodeToErrorResult ( code : HttpStatusCode ) = async {
    return ( StatusCodeResult( (int)code ) :> IActionResult )
}

// GET /thing/{thingId}
let getThing (thingId: int) (context: HttpContext): Async<IActionResult> =
    async {
        // Create a DB connection
        let! connection = openConnection()
        // Get the result
        let! result =
            // Starting with the context...
            context |> (
                // Get the user's role
                ( getRole connection )
                // Ensure the user is an admin.  
                >=> ( ensureUserHasRole "admin" )
                // Fetch the thing by ID
                >=> ( fetchThingById connection thingId ) 
                // Ensure if was found
                >=> ensureFound
                // Convert it to JSON
                >> ( map toJsonResult )
                // Map the error HttpStatusCode to an error StatusCodeResult
                >> ( mapError statusCodeToErrorResult )
                // Coalese the OK and Error into one IAction result
                >> coalesce
            )
        // Return the result
        return result
}