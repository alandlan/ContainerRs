using ContainerRS.Api.Domain;

namespace ContainerRS.Api.Data.Repositories
{
    public class LocacaoRepository(AppDbContext context) : BaseRepository<Locacao>(context)
    {
    }
}
