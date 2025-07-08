using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using ContainerRS.Api.Identity;
using ContainerRS.Api.Requests;
using ContainerRS.Api.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace ContainerRS.Api.Endpoints
{
    public static class ClientesEndpoints
    {
        public const string ENDPOINT_NAME_GET_CLIENTE = "GetCliente";
        public static RouteGroupBuilder MapGetClienteById(this RouteGroupBuilder builder)
        {
            builder.MapGet("{id}", async (
                [FromRoute] Guid id
                , [FromServices] IRepository<Cliente> repository) =>
            {
                var cliente = await repository.GetFirstAsync(c => c.Id == id, c => c.Id);
                if (cliente is null) return Results.NotFound();

                return Results.Ok(cliente);
            })
                .WithName(ENDPOINT_NAME_GET_CLIENTE)
                .Produces<IEnumerable<ClienteResponse>>(StatusCodes.Status200OK);

            return builder;
        }

        public static RouteGroupBuilder MapPostClientes(this RouteGroupBuilder builder)
        {
            builder.MapPost("registration", async (
                [FromBody] RegistroRequest request
                , [FromServices] IRepository<Cliente> repository) =>
            {
                var clienteExistente = await repository.GetFirstAsync(c => c.Email.Equals(request.Email),c => c.Id);

                if (clienteExistente is not null)
                {
                    return Results.Conflict("Cliente já cadastrado com este e-mail.");
                }

                var cliente = new Cliente(request.Nome, new Email(request.Email), request.CPF)
                {
                    Celular = request.Celular
                };
                if(request.Endereco is not null)
                {
                    cliente.AddEndereco(request.Endereco.ToModel());
                }
                await repository.AddAsync(cliente);

                return Results.CreatedAtRoute(
                    ClientesEndpoints.ENDPOINT_NAME_GET_CLIENTE,
                    new { id = cliente.Id },
                    ClienteResponse.From(cliente));
            })
                .WithName("CreateCliente")
                .Produces<ClienteResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);
            return builder;
        }

        public static RouteGroupBuilder MapPutCliente(this RouteGroupBuilder builder)
        {
            builder.MapPut("{id}", async (
                [FromRoute] Guid id,
                [FromBody] RegistroRequest request,
                [FromServices] IRepository<Cliente> repository,
                CancellationToken cancellationToken) =>
            {
                var clienteExistente = await repository.GetFirstAsync(c => c.Id == id, c => c.Id, cancellationToken);
                if (clienteExistente is null)
                {
                    return Results.NotFound("Cliente não encontrado.");
                }

                clienteExistente.Celular = request.Celular;

                await repository.UpdateAsync(clienteExistente, cancellationToken);

                return Results.Ok(new { Message = "Cliente atualizado com sucesso." });
            })
                .Produces<ClienteResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        public static RouteGroupBuilder MapDeleteCliente(this RouteGroupBuilder builder)
        {
            builder.MapDelete("{id}", async (
                [FromRoute] Guid id,
                [FromServices] IRepository<Cliente> repository,
                [FromServices] UserManager<AppUser> userManager,
                CancellationToken cancellationToken) =>
            {
                var clienteExistente = await repository.GetFirstAsync(c => c.Id == id, c => c.Id, cancellationToken);
                if (clienteExistente is null)
                {
                    return Results.NotFound("Cliente não encontrado.");
                }

                var user = await userManager.FindByEmailAsync(clienteExistente.Email.Endereco.ToString()!);

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if(user is not null) await userManager.DeleteAsync(user);
                await repository.RemoveAsync(clienteExistente, cancellationToken);

                scope.Complete();
                return Results.NoContent();
            })
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return builder;
        }
}
