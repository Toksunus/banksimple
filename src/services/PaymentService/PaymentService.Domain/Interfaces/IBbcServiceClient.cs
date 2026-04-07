namespace PaymentService.Domain.Interfaces;

public interface IBbcServiceClient
{
    Task TransactionsAsync(int bbcCompteId, string toKey, decimal amount);
    Task ConfirmTransactionAsync(int bbcAccountId, decimal amount, int transactionId, bool isOrigin, string status);
    Task RegisterKeyAsync(int bbcCompteId, string clientKey);
}
