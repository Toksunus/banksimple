namespace PaymentService.Domain.Interfaces;

public interface IBbcServiceClient
{
    Task TransactionsAsync(int bbcCompteId, string toKey, decimal amount);
    Task ConfirmTransactionAsync(Guid accountId, decimal amount, int transactionId, bool isOrigin, string status);
}
