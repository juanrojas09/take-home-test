using System.Linq.Expressions;

namespace Fundo.Applications.Domain.Interfaces;

public interface IQuerySqlDb<T> where T : class
{
    Task<List<T>> GetAsync();
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filterExpression, bool tracking = true, params Expression<Func<T, object>>[] include);
    Task<T?> FirstOrDefaultAsNoTrackingAsync(Expression<Func<T, bool>> predicate, bool tracking = false, params Expression<Func<T, object>>[] includes);
    Task<List<T>> WhereAsync(Expression<Func<T, bool>> filterExpression, bool tracking = true, params Expression<Func<T, object>>[] include);
    Task<List<T>> GetPaginatedListAsync(Expression<Func<T, bool>>? filterExpression, int page, int pageSize, bool tracking = true, params Expression<Func<T, object>>[] include);

    Task<List<T>> GetPaginatedListAsyncWithStringParams(Expression<Func<T, bool>>? filterExpression, int page, int pageSize,
        bool tracking = true, params string[]  includeStrings);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    Task<int> CountAsync(Expression<Func<T, bool>>? filterExpression, CancellationToken cancellationToken = default);
    Task<List<T>> BatchWhereAsync(Expression<Func<T, bool>> filterExpression,string orderBy, bool tracking = true, params Expression<Func<T, object>>[] include);
    
}