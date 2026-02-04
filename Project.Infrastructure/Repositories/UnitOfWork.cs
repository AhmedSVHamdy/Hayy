using Project.Core.Domain.RopositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Dictionary<Type, object> _repositories = [];
        private readonly HayyContext _dbcontext;

        public UnitOfWork(HayyContext dbcontext)
        {
           _dbcontext = dbcontext;
        }
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var EntityType = typeof(TEntity);

            if (_repositories.TryGetValue(EntityType, out object? repository))
               return (IGenericRepository<TEntity>)repository!;

            var newRepository = new GenericRepository<TEntity>(_dbcontext);

            _repositories[EntityType] = newRepository;
            return newRepository;



        }

        public async Task<int> SaveChangesAsync()=>
                    await  _dbcontext.SaveChangesAsync();

    }
}
