using ContainerRS.Api.Domain;

namespace ContainerRS.Api.Data.Repositories
{
    public class ContainerRepository : BaseRepository<Conteiner>
    {
        public ContainerRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
