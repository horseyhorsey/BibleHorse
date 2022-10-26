using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using BH.Application.Interface;
using BH.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace BH.Data
{
    public class BHRepository : IRepository
    {
        private BhDataContext _dbContext;

        public BHRepository(BhDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> AddAsync<T>(T entity) where T : BHEntity, IAggregateRoot
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<T> AddNoSaveAsync<T>(T entity) where T : BHEntity, IAggregateRoot
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task<int> CountAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.CountAsync();
        }

        public async Task DeleteAsync<T>(T entity) where T : BHEntity, IAggregateRoot
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> EnsureCreatedAsync()
        {
            return await _dbContext.Database.EnsureCreatedAsync();
        }        

        public async Task<T> FirstAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.FirstAsync();
        }

        public async Task<T> FirstOrDefaultAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.FirstOrDefaultAsync();
        }

        public T GetById<T>(long id) where T : BHEntity, IAggregateRoot
        {
            return _dbContext.Set<T>().SingleOrDefault(e => e.Id == id);
        }

        public Task<T> GetByIdAsync<T>(long id) where T : BHEntity, IAggregateRoot
        {
            return _dbContext.Set<T>().SingleOrDefaultAsync(e => e.Id == id);
        }

        public Task<List<T>> ListAsync<T>() where T : BHEntity, IAggregateRoot
        {
            return _dbContext.Set<T>().ToListAsync();
        }

        public async Task<List<T>> ListAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.ToListAsync();
        }
        public async Task UpdateAsync<T>(T entity) where T : BHEntity, IAggregateRoot
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public IQueryable<T> ApplySpecification<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot
        {
            var evaluator = new SpecificationEvaluator();
            return evaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), spec);
        }

        public Task<int> SaveChangesAsync()
        {            
            return _dbContext.SaveChangesAsync();
        }
    }
}
