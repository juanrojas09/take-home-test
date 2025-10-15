using System.ComponentModel.DataAnnotations.Schema;
using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Enums;

namespace Fundo.Applications.Domain.Entities;

public class Loans : Entity<int>
{
    [Column("amount")]
    public decimal Amount { get; private set; }
    
    [Column("current_balance")]
    public decimal CurrentBalance { get; private set; }

    [ForeignKey("ApplicantId")]
    [Column("applicant_id")]
    public int ApplicantId { get; private set; }
    
    [ForeignKey("StatusId")]
    [Column("status_id")]
    public int StatusId { get; private set; }
    
    #region navigation properties
    public Users Applicant { get; private set; } = null!;
    public LoanStates Status { get; private set; } = null!;
    #endregion
    
    // Constructor protegido para EF Core
    protected Loans() { }
    
    // Constructor privado para factory method
    private Loans(decimal amount, int applicantId, int statusId)
    {
        Amount = amount;
        CurrentBalance = amount;
        ApplicantId = applicantId;
        StatusId = statusId;
    }
    
    // Factory method para crear un nuevo préstamo
    public static Loans CreateNew(decimal amount, int applicantId, int? statusId = null)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Loan amount must be greater than zero.");
        }

        return new Loans(
            amount,
            applicantId,
            statusId ?? (int)LoanStatusesEnum.ACTIVE
        );
    }
    
    // Método para deducir del saldo actual
    public static void DeductCurrentBalance(Loans loan, decimal paymentAmount)
    {
        if (paymentAmount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }

        if (paymentAmount > loan.CurrentBalance)
        {
            throw new InvalidOperationException("Payment amount exceeds current balance.");
        }

        loan.CurrentBalance -= paymentAmount;

        if (loan.CurrentBalance == 0)
        {
            loan.StatusId = (int)LoanStatusesEnum.PAID; 
        }
    }
}