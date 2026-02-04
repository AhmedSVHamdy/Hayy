using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure.Repositories
{
    public class UserInterestRepository : IUserInterestRepository
    {
        private readonly HayyContext _context;

        public UserInterestRepository(HayyContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<UserInterestProfile> interests)
        {
            await _context.UserInterestProfiles.AddRangeAsync(interests);
            
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
