using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RepositoryContracts
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(Guid id);
        Task AddAsync(TEntity entity);
        void Remove(TEntity entity);
        void Update (TEntity entity);




    }
}
