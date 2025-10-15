using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fundo.Applications.Domain.Entities;

[BsonIgnoreExtraElements]
public class Payments
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; } = null!;

    [BsonElement("amount")]
    public decimal Amount { get; private set; }

    [BsonElement("applicantId")]
    public int ApplicantId { get; private set; }

    [BsonElement("paymentDate")]
    public DateTime PaymentDate { get; private set; }

    [BsonElement("loanId")]
    public int LoanId { get; private set; }

    [BsonElement("isLoanPaymentCompleted")]
    public bool IsLoanPaymentCompleted { get; private set; } = false;
    
  
    protected Payments() { }
    

    public Payments(decimal amount, int applicantId, int loanId, DateTime? paymentDate = null, bool isLoanPaymentCompleted = false)
    {
        Amount = amount;
        ApplicantId = applicantId;
        LoanId = loanId;
        PaymentDate = paymentDate ?? DateTime.UtcNow;
        IsLoanPaymentCompleted = isLoanPaymentCompleted;
    }
    
   
    public void MarkAsCompleted()
    {
        IsLoanPaymentCompleted = true;
    }
}