using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using ContainerRS.Api.Identity;
using ContainerRS.Api.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContainerRS.Api.Endpoints
{
    public static class AprovacaoClientesEndpoints
    {
        public static IEndpointRouteBuilder MapAprovacaoClienteEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup(EndpointConstants.ROUTE_CLIENTES)
                .RequireAuthorization(policy => policy.RequireRole("Suporte"))
                .WithTags(EndpointConstants.TAG_CLIENTES)
                .WithOpenApi();

            group
                .MapApproveRegistroCliente()
                .MapRejectRegistroCliente();

            return builder;
        }

        public static RouteGroupBuilder MapApproveRegistroCliente(this RouteGroupBuilder builder)
        {
            builder.MapPatch("registration/{id:guid}/approve", async (
                [FromRoute] Guid id
                , [FromServices] IRepository<Cliente> repository
                , [FromServices] UserManager<AppUser> userManager) =>
            {
                var cliente = await repository.GetFirstAsync(c => c.Id == id, c => c.Id);
                if (cliente is null) return Results.NotFound();

                var user = await userManager.FindByEmailAsync(cliente.Email.Endereco);
                if (user is null)
                {
                    user = new AppUser
                    {
                        UserName = cliente.Email.Endereco,
                        Email = cliente.Email.Endereco,
                    };
                    await userManager.CreateAsync(user, "Alura@123");
                    await userManager.AddToRoleAsync(user, "Cliente");
                }

                user.EmailConfirmed = true;
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);

                return Results.Ok(new RegistrationStatusResponse(cliente.Id.ToString(), cliente.Email.Endereco, "Aprovado"));
            })
                .Produces<RegistrationStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
            return builder;
        }
    
        public static RouteGroupBuilder MapRejectRegistroCliente(this RouteGroupBuilder builder)
        {
            builder.MapPatch("registration/{id:guid}/reject", async (
                [FromRoute] Guid id
                , [FromServices] IRepository<Cliente> repository
                , [FromServices] UserManager<AppUser> userManager) =>
            {
                var cliente = await repository.GetFirstAsync(c => c.Id == id, c => c.Id);
                if (cliente is null) return Results.NotFound();

                var user = await userManager.FindByEmailAsync(cliente.Email.Endereco);
                if (user is null) return Results.NotFound();

                user.EmailConfirmed = false;
                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);

                return Results.Ok(new RegistrationStatusResponse(cliente.Id.ToString(), cliente.Email.Endereco, "Registro não aprovado"));
            })
                .Produces<RegistrationStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
            return builder;
        }
    }
}
