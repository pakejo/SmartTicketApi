using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SmartTicketApi.Data.Repository
{
    public abstract class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly SmartTicketApiContext context;
        private readonly DbSet<T> table;

        protected GenericRepository(SmartTicketApiContext context)
        {
            this.context = context;
            table = context.Set<T>();
        }

        public void Create(T entity) => table.Add(entity);

        public Task<List<T>> GetAll() => table.ToListAsync();

        public Task<T?> GetById(string id) => table.FindAsync(id).AsTask();

        public void Update(T entity)
        {
            table.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity) => table.Remove(entity);

        public async Task Save() => await context.SaveChangesAsync();

        public Task<bool> Exists(Expression<Func<T, bool>> predicate) => table.AnyAsync(predicate);
    }
}
