
using Fundo.Applications.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.Infrastructure.Persistance.Repositories;

public class CommandSqlDb<T>:ICommandSqlDb<T> where T:class
{   
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _entity;
    public CommandSqlDb(ApplicationDbContext context)
    {
        _context = context;
        _entity = context.Set<T>();
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            await _entity.AddAsync(entity);
            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public async Task<List<T>> AddRangeAsync(List<T> entity)
    {
        try
        {
            await _entity.AddRangeAsync(entity);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }

        return entity;
    }

    public void RemoveAsync(T entity)
    {
        try
        {
             _context.Remove(entity);
           
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public void RemoveRangeAsync(List<T> entities)
    {
        try
        {
            _context.RemoveRange(entities);
     
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public virtual void UpdateAsync(T entity, bool updateRelatedEntities = false)
    {
        try
        {
            var entry = _context.Entry(entity);
            if (updateRelatedEntities)
            {
                foreach (var navigation in entry.Navigations)
                {
                    if (navigation.CurrentValue != null)
                    {
                        navigation.IsModified = true;
                    }
                }
            }
        
            entry.State = EntityState.Modified;
      
            
        
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public async Task SaveAsyncChanges(CancellationToken token)
    {
        await _context.SaveChangesAsync(token);
    }

    public virtual async Task UpdateRangeAsync(List<T> entities)
    {
        try
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }

    public Task LogicDeleteAsync(T entity)
    {
        try
        {
            var entry = _context.Entry(entity);
            var prop = entity.GetType().GetProperty("DeletedAt");
            if (prop != null && prop.PropertyType == typeof(DateTime?))
            {
                prop.SetValue(entity, DateTime.UtcNow);
            }
            else
            {
                throw new Exception("Entity does not have a DeletedAt property of type DateTime?");
            }
            entry.State = EntityState.Modified;
            return _context.SaveChangesAsync();
            
            
        }catch(Exception ex)
        {
            throw new Exception(ex.ToString());
        }
    }
}