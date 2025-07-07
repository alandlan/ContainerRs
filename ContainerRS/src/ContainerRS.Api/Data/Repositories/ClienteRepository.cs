using ContainerRS.Api.Contracts;
using ContainerRS.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ContainerRS.Api.Data.Repositories
{
    public class ClienteRepository(AppDbContext dbContext) : IRepository<Cliente>
    {
        public async Task<Cliente> AddAsync(Cliente obj, CancellationToken cancellationToken = default)
        {
            await dbContext.Clientes.AddAsync(obj, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return obj;
        }

        public async Task<Cliente?> GetFirstAsync<TProperty>(Expression<Func<Cliente, bool>> predicate, Expression<Func<Cliente, TProperty>> orderBy, CancellationToken cancellationToken = default)
        {
            return await dbContext.Clientes
                .Include(c => c.Enderecos)
                .AsNoTracking()
                .OrderBy(orderBy)
                .FirstOrDefaultAsync(predicate,cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> GetWhereAsync(Expression<Func<Cliente, bool>>? filtro = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Cliente> queryClientes = dbContext.Clientes;
            if (filtro != null)
            {
                queryClientes = queryClientes.Where(filtro);
            }

            return await queryClientes
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task RemoveAsync(Cliente obj, CancellationToken cancellationToken = default)
        {
            dbContext.Clientes.Remove(obj);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Cliente> UpdateAsync(Cliente obj, CancellationToken cancellationToken = default)
        {
            dbContext.Set<Endereco>()
                .Where(e => e.ClienteId == obj.Id && !obj.Enderecos.Contains(e))
                .ToList()
                .ForEach(e => dbContext.Set<Endereco>().Remove(e));

            dbContext.Clientes.Update(obj);
            await dbContext.SaveChangesAsync(cancellationToken);
            return obj;
        }
    }
}
