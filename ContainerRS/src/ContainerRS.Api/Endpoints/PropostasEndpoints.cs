using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using ContainerRS.Api.Requests;
using ContainerRS.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace ContainerRS.Api.Endpoints
{
    public static class PropostasEndpoints
    {
        public const string ENDPOINT_NAME_GET_PROPOSTA = "GetProposta";

        public static IEndpointRouteBuilder MapPropostasEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup(EndpointConstants.ROUTE_SOLICITACOES)
                .WithTags(EndpointConstants.TAG_LOCACAO)
                .WithOpenApi();

            group
                .MapPostProposta()
                .MapGetPropostas()
                .MapGetPropostaById()
                .MapPatchAcceptProposta()
                .MapPatchRejectProposta()
                .MapPostComentarioProposta();

            return builder;
        }

        public static RouteGroupBuilder MapPostProposta(this RouteGroupBuilder builder)
        {
            builder.MapPost("{id:guid}/proposals", async (
                [FromRoute] Guid id,
                [FromForm] PropostaRequest request,
                [FromServices] IRepository<Solicitacao> repoSolicitacao,
                [FromServices] IRepository<Proposta> repoProposta) =>
            {
                var solicitacao = await repoSolicitacao.GetFirstAsync(s => s.Id == id, s => s.Id);

                if (solicitacao is null) return Results.NotFound($"Solicitação com ID {id} não encontrada.");

                var proposta = new Proposta()
                {
                    Id = Guid.NewGuid(),
                    ValorTotal = request.ValorTotal,
                    DataCriacao = DateTime.UtcNow,
                    DataExpiracao = request.DataExpiracao,
                    NomeArquivo = request.Arquivo.FileName,
                    ClienteId = solicitacao.ClienteId,
                };

                await repoProposta.AddAsync(proposta);

                return Results.CreatedAtRoute(ENDPOINT_NAME_GET_PROPOSTA, new { id = proposta.Id }, PropostaResponse.From(proposta));
            })
            .RequireAuthorization(policy => policy.RequireRole("Suporte"))
            .WithSummary("Vendedor envia proposta para solicitação")
            .Produces<PropostaResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapGetPropostaById(this RouteGroupBuilder builder)
        {
            builder.MapGet("{id:guid}/proposals/{propostaId:guid}", async (
                [FromRoute] Guid id,
                [FromRoute] Guid propostaId,
                [FromServices] IRepository<Proposta> repoProposta) =>
            {
                var proposta = await repoProposta.GetFirstAsync(p => p.Id == propostaId && p.SolicitacaoId == id, p => p.Id);
                if (proposta is null) return Results.NotFound($"Proposta com ID {propostaId} não encontrada para a solicitação com ID {id}.");

                return Results.Ok(PropostaResponse.From(proposta));
            })
            .WithName(ENDPOINT_NAME_GET_PROPOSTA)
            .RequireAuthorization(policy => policy.RequireRole("Cliente"))
            .WithSummary("Obter proposta por ID")
            .Produces<PropostaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapGetPropostas(this RouteGroupBuilder builder)
        {
            builder.MapGet("{id:guid}/proposals", async (
                [FromRoute] Guid id,
                [FromServices] IRepository<Solicitacao> repository) =>
            {
                var propostas = await repository.GetFirstAsync(p => p.Id == id, p => p.Id);
                if (propostas is null) return Results.NotFound($"Nenhuma proposta encontrada para a solicitação com ID {id}.");

                return Results.Ok(propostas.Propostas.Select(p => PropostaResponse.From(p)));
            })
            .RequireAuthorization(policy => policy.RequireRole("Cliente"))
            .WithSummary("Obter todas as propostas de uma solicitação")
            .Produces<IEnumerable<PropostaResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapPatchAcceptProposta(this RouteGroupBuilder builder)
        {
            builder.MapPatch("{id:guid}/proposals/{propostaId:guid}/accept", async (
                [FromRoute] Guid id,
                [FromRoute] Guid propostaId,
                [FromServices] IRepository<Proposta> repoProposta,
                [FromServices] IRepository<Locacao> repoLocacao) =>
            {
                var proposta = await repoProposta.GetFirstAsync(p => p.Id == propostaId && p.SolicitacaoId == id, p => p.Id);
                if (proposta is null) return Results.NotFound($"Proposta com ID {propostaId} não encontrada para a solicitação com ID {id}.");

                proposta.Status = StatusProposta.Aceita;

                var locacao = new Locacao
                {
                    Id = Guid.NewGuid(),
                    PropostaId = proposta.Id,
                    DataInicio = DateTime.UtcNow,
                    DataPrevistaEntrega = proposta.Solicitacao.DataInicioOperacao.AddDays(-proposta.Solicitacao.DisponibilidadePrevia), // Exemplo de data prevista de entrega
                    DataTermino = proposta.Solicitacao.DataInicioOperacao.AddDays(proposta.Solicitacao.DuracaoPrevistaLocacao),
                };

                using var score = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await repoProposta.UpdateAsync(proposta);
                await repoLocacao.AddAsync(locacao);

                score.Complete();

                return Results.Ok(PropostaResponse.From(proposta));
            })
            .RequireAuthorization(policy => policy.RequireRole("Cliente"))
            .WithSummary("Aceitar proposta")
            .Produces<PropostaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapPatchRejectProposta(this RouteGroupBuilder builder)
        {
            builder.MapPatch("{id:guid}/proposals/{propostaId:guid}/reject", async (
                [FromRoute] Guid id,
                [FromRoute] Guid propostaId,
                [FromServices] IRepository<Proposta> repoProposta) =>
            {
                var proposta = await repoProposta.GetFirstAsync(p => p.Id == propostaId && p.SolicitacaoId == id, p => p.Id);
                if (proposta is null) return Results.NotFound($"Proposta com ID {propostaId} não encontrada para a solicitação com ID {id}.");

                proposta.Status = StatusProposta.Recusada;
                await repoProposta.UpdateAsync(proposta);

                return Results.Ok(PropostaResponse.From(proposta));
            })
            .RequireAuthorization(policy => policy.RequireRole("Cliente"))
            .WithSummary("Recusar proposta")
            .Produces<PropostaResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapPostComentarioProposta(this RouteGroupBuilder builder)
        {
            builder.MapPost("{id:guid}/proposals/{propostaId:guid}/comment", async (
                [FromRoute] Guid id,
                [FromRoute] Guid propostaId,
                [FromBody] ComentarioRequest request,
                HttpContext context,
                [FromServices] IRepository<Proposta> repoProposta) =>
            {
                var proposta = await repoProposta.GetFirstAsync(p => p.Id == propostaId && p.SolicitacaoId == id, p => p.Id);
                if (proposta is null) return Results.NotFound($"Proposta com ID {propostaId} não encontrada para a solicitação com ID {id}.");
                
                string? quem = context.User.Identity?.Name;
                if (quem is null) return Results.Unauthorized();

                var comentario = proposta.AddComentario(new Comentario
                {
                    Id = Guid.NewGuid(),
                    Texto = request.Comentario,
                    Data = DateTime.UtcNow,
                    Usuario = quem
                });

                await repoProposta.UpdateAsync(proposta);

                return Results.Ok(PropostaResponse.From(proposta));
            })
            .RequireAuthorization(policy => policy.RequireRole("Cliente", "Suporte"))
            .WithSummary("Adicionar comentário à proposta")
            .Produces<Comentario>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return builder;
        }
    }
}
