using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure.ApplicationDbContext;

using System.Collections;

namespace Project.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HayyContext _context;
        private Hashtable _repositories; // ده مخزن عشان دالة GetRepository

        // الخصائص الصريحة (Explicit Properties)
        public IPaymentRepository Payments { get; private set; }
        public IBusinessSubscriptionRepository BusinessSubscriptions { get; private set; }
        public IGenericRepository<SubscriptionPlan> SubscriptionPlans { get; private set; }

        public UnitOfWork(HayyContext context)
        {
            _context = context;

            // تهيئة الـ Repositories المخصوصة
            Payments = new PaymentRepository(_context);
            BusinessSubscriptions = new BusinessSubscriptionRepository(_context);

            // تهيئة الـ Generic Repository للباقات
            SubscriptionPlans = new GenericRepository<SubscriptionPlan>(_context);
        }

        // دالة الحفظ اللي أنت اخترتها
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // تنفيذ دالة GetRepository الذكية
        // وظيفتها: لو الـ Repo موجود هاته، ولو مش موجود أنشئه وخزنه للمرة الجاية
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);

                // بنعمل CreateInstance للدتايب ده runtime
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(TEntity)),
                    _context
                );

                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}