using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ContainerRS.Api.Identity
{
    public class AppUserClaimsPrincipalFactory(
        UserManager<AppUser> userManager
        , IOptions<IdentityOptions> optionsAccessor
        , IRepository<Cliente> repository)
        : UserClaimsPrincipalFactory<AppUser>(userManager, optionsAccessor)
    {
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            var cliente = await repository
                .GetFirstAsync(
                    c => c.Email.Endereco.Equals(user.Email),
                    c => c.Id);

            if (cliente is not null)
            {
                identity.AddClaim(new Claim("ClienteId", cliente.Id.ToString()));
            }

            (await UserManager.GetRolesAsync(user))
                .ToList()
                .ForEach(role => identity.AddClaim(new Claim(ClaimTypes.Role, role)));

            return identity;
        }
    }
}
