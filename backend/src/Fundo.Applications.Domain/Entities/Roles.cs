using System.ComponentModel.DataAnnotations.Schema;
using Fundo.Applications.Domain.Common;

namespace Fundo.Applications.Domain.Entities;

[Table("roles")]
public class Roles : Entity<int>
{
    [Column("name")]
    public string Name { get; private set; } = null!;
    
    [Column("description")]
    public string? Description { get; private set; }

    protected Roles() { }

    public Roles(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }
}