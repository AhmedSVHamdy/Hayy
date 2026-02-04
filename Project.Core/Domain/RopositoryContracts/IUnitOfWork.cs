using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();

        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
