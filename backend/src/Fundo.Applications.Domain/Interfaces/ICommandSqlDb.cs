
namespace Fundo.Applications.Domain.Interfaces;

public interface ICommandSqlDb<T>
    where T : class
{
    Task<T> AddAsync(T entity);
    void UpdateAsync(T entity, bool updateRelatedEntities = false);
    void RemoveAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entity);
    Task SaveAsyncChanges(CancellationToken token);
    void RemoveRangeAsync(List<T> entities);
    Task UpdateRangeAsync(List<T> entities);
    
    Task LogicDeleteAsync(T entity);
}