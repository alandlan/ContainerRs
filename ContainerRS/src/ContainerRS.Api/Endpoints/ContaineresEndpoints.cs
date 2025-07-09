using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using ContainerRS.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ContainerRS.Api.Endpoints
{
    public static class ContaineresEndpoints
    {
        public const string ENDPOINT_NAME_GET_CONTEINER = "GetContainer";

        public static IEndpointRouteBuilder MapConteineresEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup(EndpointConstants.ROUTE_CONTEINERES)
                .RequireAuthorization(builder => builder.RequireRole("Cliente"))
                .WithTags(EndpointConstants.TAG_CONTEINERES)
                .WithOpenApi();

            group
                .MapGetConteinerById();

            return builder;
        }

        public static RouteGroupBuilder MapGetConteinerById(this RouteGroupBuilder builder)
        {
            builder.MapGet("{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] IRepository<Conteiner> repository) =>
            {
                var container = await repository.GetFirstAsync(c => c.Id == id, p => p.Id);
                if (container == null) return Results.NotFound();

                return Results.Ok(ConteinerResponse.From(container));
            })
            .WithName(ENDPOINT_NAME_GET_CONTEINER)
            .WithSummary("Cliente consulta informações de um contêiner")
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ConteinerResponse>(StatusCodes.Status200OK);

            return builder;
        }
    }
}
