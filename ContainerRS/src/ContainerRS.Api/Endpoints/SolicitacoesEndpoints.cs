using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using ContainerRS.Api.Extensions;
using ContainerRS.Api.Requests;
using ContainerRS.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ContainerRS.Api.Endpoints
{
    public static class SolicitacoesEndpoints
    {
        public const string ENDPOINT_NAME_GET_SOLICITACAO = "GetSolicitacao";

        public static IEndpointRouteBuilder MapSolicitacoesEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup(EndpointConstants.ROUTE_SOLICITACOES)
                .RequireAuthorization(policy => policy.RequireRole("Cliente"))
                .WithTags(EndpointConstants.TAG_LOCACAO)
                .WithOpenApi();

            group
                .MapGetSolicitacaoById()
                .MapGetSolicitacoes()
                .MapPostSolicitacao()
                .MapDeleteSolicitacao();

            return builder;
        }
        public static RouteGroupBuilder MapGetSolicitacaoById(this RouteGroupBuilder builder)
        {
            builder.MapGet("{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] IRepository<Solicitacao> repository) =>
            {
                var solicitacao = await repository.GetFirstAsync(s => s.Id == id, s => s.Id);
                if (solicitacao == null) return Results.NotFound();

                return Results.Ok(SolicitacaoResponse.From(solicitacao));
            })
            .WithName(ENDPOINT_NAME_GET_SOLICITACAO)
            .WithSummary("Get a specific solicitation by ID")
            .WithDescription("Retrieves a solicitation based on the provided ID.")
            .Produces<SolicitacaoResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapGetSolicitacoes(this RouteGroupBuilder builder)
        {
            builder.MapGet("", async (
                HttpContext context,
                [FromServices] IRepository<Solicitacao> repository) =>
            {
                var clienteId = context.GetClientId();
                if (clienteId == null) return Results.Unauthorized();

                var solicitacoes = await repository.GetWhereAsync(
                    s => s.ClienteId == clienteId.Value && s.Status.Status.Equals("Ativa"));

                return Results.Ok(solicitacoes.Select(SolicitacaoResponse.From));
            })
            .WithSummary("Lista as solicitações ativas do cliente")
            .Produces<IEnumerable<SolicitacaoResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            return builder;
        }

        public static RouteGroupBuilder MapPostSolicitacao(this RouteGroupBuilder builder)
        {
            builder.MapPost("", async (
                [FromBody] SolicitacaoRequest request,
                HttpContext context,
                [FromServices] IRepository<Solicitacao> repository) =>
            {
                var clienteId = context.GetClientId();
                if (clienteId == null) return Results.Unauthorized();

                var solicitacao = new Solicitacao
                {
                    ClienteId = clienteId.Value,
                    Descricao = request.Descricao,
                    QuantidadeEstimada = request.QuantidadeEstimada,
                    Finalidade = request.Finalidade,
                    DataInicioOperacao = request.Periodo.DataInicioOperacao,
                    DisponibilidadePrevia = request.Periodo.DisponibilidadePrevia,
                    DuracaoPrevistaLocacao = request.Periodo.QuantidadeDias,
                };

                if (request.Localizacao.EnderecoId.HasValue)
                {
                    solicitacao.EnderecoId = request.Localizacao.EnderecoId.Value;
                }

                await repository.AddAsync(solicitacao);

                return Results.CreatedAtRoute(ENDPOINT_NAME_GET_SOLICITACAO, new { id = solicitacao.Id }, SolicitacaoResponse.From(solicitacao));
            })
            .WithSummary("Cria uma nova solicitação")
            .Produces<SolicitacaoResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);

            return builder;
        }

        public static RouteGroupBuilder MapDeleteSolicitacao(this RouteGroupBuilder builder)
        {
            builder.MapDelete("{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] IRepository<Solicitacao> repository) =>
            {
                var solicitacao = await repository.GetFirstAsync(s => s.Id == id, s => s.Id);

                if (solicitacao == null) return Results.NotFound();

                solicitacao.Status = StatusSolicitacao.Cancelada;
                await repository.UpdateAsync(solicitacao);

                return Results.NoContent();
            })
            .WithSummary("Remove uma solicitação")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }
    }
}
