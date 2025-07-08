using ContainerRS.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ContainerRS.Api.Data.Repositories
{
    public class SolicitacaoRepository(AppDbContext dbContext)
        : BaseRepository<Solicitacao>(dbContext)
    {
        public override Task<Solicitacao?> GetFirstAsync<TProperty>(Expression<Func<Solicitacao,bool>> filtro, Expression<Func<Solicitacao,TProperty>> orderBy, CancellationToken cancellationToken = default)
        {
            return dbContext.Solicitacoes
                .Include(s => s.Propostas)
                .AsNoTracking()
                .OrderBy(orderBy)
                .FirstOrDefaultAsync(filtro, cancellationToken);
        }
    }
}
