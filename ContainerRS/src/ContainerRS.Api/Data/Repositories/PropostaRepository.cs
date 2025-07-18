﻿using ContainerRS.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ContainerRS.Api.Data.Repositories
{
    public class PropostaRepository(AppDbContext dbContext) : BaseRepository<Proposta>(dbContext)
    {
        public override Task<Proposta?> GetFirstAsync<TProperty>(Expression<Func<Proposta, bool>> include, Expression<Func<Proposta,TProperty>> orderBy, CancellationToken cancellationToken = default)
        {
            return dbContext.Propostas
                .Include(p => p.Comentarios)
                .Include(p => p.Solicitacao)
                .AsNoTracking()
                .OrderBy(orderBy)
                .FirstOrDefaultAsync(include, cancellationToken);
        }
    }
}
