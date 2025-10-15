using System.ComponentModel.DataAnnotations.Schema;
using Fundo.Applications.Domain.Common;

namespace Fundo.Applications.Domain.Entities;

[Table("loan_states")]
public class LoanStates : Entity<int>
{
    [Column("name")]
    public string Name { get; private set; } = null!;
    
   
    protected LoanStates() { }
    
 
    public LoanStates(string name)
    {
        Name = name;
    }
}