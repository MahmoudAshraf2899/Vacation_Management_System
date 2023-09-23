using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IServices_Repository_Layer
{
    public interface IGenericRepository<T> where T : class
    {
        void Add(T entity);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        void Update(T entity);
        Task DeleteAsync(T entity);
        void Delete(T entity);
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        Task SaveAsync();
        void Save();
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task AddRangeAsync(IEnumerable<T> entities);
        void AddRange(IEnumerable<T> entities);

    }
}
