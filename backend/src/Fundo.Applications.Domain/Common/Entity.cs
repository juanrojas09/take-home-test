using System.ComponentModel.DataAnnotations.Schema;
using Fundo.Applications.Domain.Events;

namespace Fundo.Applications.Domain.Common;

public class Entity<T>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public T Id { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Column("deleted_at")]
    public bool DeletedAt { get; set; } = false;
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        if(!_domainEvents.Any()) return;
        _domainEvents.Clear();
    }
}