using Microsoft.EntityFrameworkCore;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly HayyContext _dbcontext;

        public GenericRepository(HayyContext dbcontext )
        {
           _dbcontext = dbcontext;
        }
        public async Task AddAsync(TEntity entity)
        {
          await _dbcontext.Set<TEntity>().AddAsync( entity );
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()=> 
                       await _dbcontext.Set<TEntity>().ToListAsync();
        

        public async Task<TEntity?> GetByIdAsync(Guid id) =>
                       await _dbcontext.Set<TEntity>().FindAsync(id).AsTask();


        public void Remove(TEntity entity)=>
                    _dbcontext.Set<TEntity>().Remove(entity);


        public void Update(TEntity entity)=> 
                    _dbcontext.Set<TEntity>().Update(entity);

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbcontext.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }

    }
}
