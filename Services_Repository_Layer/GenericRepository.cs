using IServices_Repository_Layer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services_Repository_Layer
{
    public abstract class GenericRepository<C, T> : IGenericRepository<T> where T : class where C : Microsoft.EntityFrameworkCore.DbContext, new()
    {
        private C _entities = new C();
        public C Context
        {
            get { return _entities; }
            set { _entities = value; }

        }
        public async Task AddAsync(T entity)
        {
            await _entities.Set<T>().AddAsync(entity);
            await _entities.SaveChangesAsync();
        }
        public void Add(T entity)
        {
            _entities.Set<T>().Add(entity);
            _entities.SaveChanges();
        }
        public void AddRange(IEnumerable<T> entities)
        {
            _entities.Set<T>().AddRange(entities);
            _entities.SaveChanges();
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _entities.Set<T>().AddRangeAsync(entities);
            await _entities.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _entities.Set<T>().Remove(entity);
            await _entities.SaveChangesAsync();
        }
        public void Delete(T entity)
        {
            _entities.Set<T>().Remove(entity);
            _entities.SaveChanges();
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            var query = _entities.Set<T>().Where(predicate);
            return query;
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task SaveAsync()
        {
            await _entities.SaveChangesAsync();
        }
        public void Save()
        {
            _entities.SaveChanges();
        }

        public async Task UpdateAsync(T entity)
        {
            _entities.Set<T>().Update(entity);
            await _entities.SaveChangesAsync();
        }
        public void Update(T entity)
        {
            _entities.Set<T>().Update(entity);
            _entities.SaveChanges();
        }
    }
}
