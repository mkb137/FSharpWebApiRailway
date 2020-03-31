namespace RailwayWebApi.Controllers

open System.Net
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

// Put all our routing here.
[<ApiController>]
type RouteController(logger: ILogger<RouteController>) =
    inherit ControllerBase()


    [<HttpGet>]
    [<Route("/thing/{thingId}")>]
    member __.GetThing(thingId: int): Async<IActionResult> =
        async {
            logger.LogDebug "Get"
            return! ThingEndpoints2.getThing thingId __.HttpContext
        }
