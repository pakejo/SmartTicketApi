using System.Linq.Expressions;

namespace SmartTicketApi.Data.Repository
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get all entities of type T
        /// </summary>
        /// <returns>Entity of type T</returns>
        Task<List<T>> GetAll();

        /// <summary>
        /// Get an entity information
        /// </summary>
        /// <param name="id">Enity primary key</param>
        /// <returns>Entity of type T</returns>
        Task<T?> GetById(string id);

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="entity">New entity</param>
        /// <returns>The new created entity</returns>
        void Create(T entity);

        /// <summary>
        /// Updates a given entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Update entity</returns>
        void Update(T entity);

        /// <summary>
        /// Deletes a given entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns></returns>
        void Delete(T entity);

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        /// <returns></returns>
        public Task Save();

        /// <summary>
        /// Checks if it exists a entity with the given condition
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<bool> Exists(Expression<Func<T, bool>> predicate);
    }
}
