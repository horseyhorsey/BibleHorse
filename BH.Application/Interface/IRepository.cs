using Ardalis.Specification;
using BH.Domain.Model;

namespace BH.Application.Interface
{
    public interface IRepository
    {
        Task<T> AddAsync<T>(T entity) where T : BHEntity, IAggregateRoot;
        Task<T> AddNoSaveAsync<T>(T entity) where T : BHEntity, IAggregateRoot;
        IQueryable<T> ApplySpecification<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot;
        Task<int> CountAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot;
        Task DeleteAsync<T>(T entity) where T : BHEntity, IAggregateRoot;
        Task<bool> EnsureCreatedAsync();
        Task<T> FirstAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot;
        Task<T> FirstOrDefaultAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot;
        Task<T> GetByIdAsync<T>(long id) where T : BHEntity, IAggregateRoot;
        Task<List<T>> ListAsync<T>() where T : BHEntity, IAggregateRoot;
        Task<List<T>> ListAsync<T>(ISpecification<T> spec) where T : BHEntity, IAggregateRoot;
        Task<int> SaveChangesAsync();
        Task UpdateAsync<T>(T entity) where T : BHEntity, IAggregateRoot;
    }
}
