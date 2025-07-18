﻿using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using ContainerRS.Api.Extensions;
using ContainerRS.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ContainerRS.Api.Endpoints
{
    public static class LocacoesEndpoints
    {
        public static IEndpointRouteBuilder MapLocacoesEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup(EndpointConstants.ROUTE_LOCACOES)
                .RequireAuthorization(builder => builder.RequireRole("Cliente"))
                .WithTags(EndpointConstants.TAG_LOCACAO)
                .WithOpenApi();

            group
                .MapGetLocacoes();

            return builder;
        }
        public static RouteGroupBuilder MapGetLocacoes(this RouteGroupBuilder builder)
        {
            builder.MapGet("", async (
                HttpContext context,
                [FromServices] IRepository<Locacao> repository) =>
            {
                var clienteId = context.GetClientId();
                if (clienteId == null) return Results.Unauthorized();

                var locacoes = await repository.GetWhereAsync(l => l.ClienteId == clienteId.Value);

                return Results.Ok(locacoes.Select(LocacaoResponse.From));
            })
            .WithSummary("Listar Locações");

            return builder;
        }
    }
}
