

using System.Linq.Expressions;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Interfaces;
using Fundo.Applications.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

public class QuerySqlDb<T>:IQuerySqlDb<T> where T:class
{
    protected readonly ApplicationDbContext _context;
    protected readonly IQueryable<T> _entity;

    public QuerySqlDb(ApplicationDbContext context)
    {
        _context = context;
        _entity = _context!.Set<T>();
    }

    public async Task<List<T>> GetAsync()
    {
        try
        {
            return await _entity.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filterExpression, bool tracking = true, params Expression<Func<T, object>>[] include)
    {
        try
        {
            IQueryable<T> query = _entity;

            if (include.Any())
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            if (tracking)
            {
                return await query.FirstOrDefaultAsync(filterExpression);
            }
            else
            {
                return await query.AsNoTracking().FirstOrDefaultAsync(filterExpression);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al ejecutar FirstOrDefaultAsync: {ex.Message}", ex);
        }
    }

    public virtual async Task<List<T>> GetPaginatedListAsyncWithStringParams(
        Expression<Func<T, bool>>? filterExpression,
        int page,
        int pageSize,
        bool tracking = true,
        params string[] includeStrings)
    {
        try
        {
            IQueryable<T> query = _entity.AsQueryable();

            if (includeStrings.Any())
            {
                foreach (var includeStr in includeStrings)
                {
                    query = query.Include(includeStr);
                }
            }

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            if (filterExpression == null)
            {
                return await query
                    .OrderByDescending(x=> EF.Property<object>(x, "CreatedAt"))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            return await query
                .OrderBy(x=> EF.Property<object>(x, "Id")) 
                .Where(filterExpression)
                .Skip(Pagination<T>.CalculateOffset(page,pageSize))
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
    public async Task<List<T>> WhereAsync(
        Expression<Func<T, bool>> filterExpression,
        bool tracking = true,
        params Expression<Func<T, object>>[] include)
    {
        try
        {
               
            IQueryable<T> query = _entity;

                
            if (include.Any())
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

     
            query = tracking ? query : query.AsNoTracking();

                
            return await query.Where(filterExpression).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }


    public virtual async Task<List<T>> GetPaginatedListAsync(
        Expression<Func<T, bool>>? filterExpression,
        int page,
        int pageSize,
        bool tracking = true,
        params Expression<Func<T, object>>[] include)
    {
        try
        {
            IQueryable<T> query = _entity.AsQueryable();

            if (include.Any())
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            if (filterExpression == null)
            {
                return await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            return await query
                .Where(filterExpression)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }




    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
    {
        return await _entity.AnyAsync(filter);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? filterExpression, CancellationToken cancellationToken = default)
    {
        if (filterExpression == null)
        {
            return await _entity.CountAsync(cancellationToken);
        }
        return await _entity.CountAsync(filterExpression, cancellationToken);
    }

    public async Task<List<T>> BatchWhereAsync(Expression<Func<T, bool>> filterExpression,string orderBy, bool tracking = true, params Expression<Func<T, object>>[] include)
    {
        try
        {
               
            IQueryable<T> query = _entity;

                
            if (include.Any())
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

     
            query = tracking ? query : query.AsNoTracking();

           
                
            return await query.Where(filterExpression).Take(20).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public async Task<T?> FirstOrDefaultAsNoTrackingAsync(Expression<Func<T, bool>> predicate, bool tracking = false, params Expression<Func<T, object>>[] includes)
    {
        try
        {
            IQueryable<T> query = _entity;

            if (includes.Any())
            {
                foreach (var includeExpression in includes)
                {
                    query = query.Include(includeExpression);
                }
            }
            
            return await query.AsNoTracking().FirstOrDefaultAsync(predicate);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
}