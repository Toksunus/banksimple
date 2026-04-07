using System.Net.Http.Json;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services;

public class BbcServiceHttpClient : IBbcServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly int _bankId;

    public BbcServiceHttpClient(HttpClient httpClient, int bankId)
    {
        _httpClient = httpClient;
        _bankId = bankId;
    }

    public async Task TransactionsAsync(int bbcCompteId, string toKey, decimal amount)
    {
        var payload = new
        {
            from_account_id = bbcCompteId,
            from_bank_id = _bankId,
            to_key = toKey,
            amount
        };
        var response = await _httpClient.PostAsJsonAsync("/transactions", payload);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Transaction failed: {error}");
        }
    }

    public async Task RegisterKeyAsync(int bbcCompteId, string clientKey)
    {
        var payload = new
        {
            client_key = clientKey,
            account_id = bbcCompteId,
            bank_id = _bankId
        };
        var response = await _httpClient.PostAsJsonAsync("/create-key", payload);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"BBC key registration failed: {error}");
        }
    }

    public async Task ConfirmTransactionAsync(int bbcAccountId, decimal amount, int transactionId, bool isOrigin, string status)
    {
        var payload = new
        {
            from_account_id = bbcAccountId,
            from_bank_id = _bankId,
            amount,
            is_origin = isOrigin,
            transaction_id = transactionId,
            status
        };
        var response = await _httpClient.PostAsJsonAsync("/confirm-transaction", payload);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Confirm transaction failed: {error}");
        }
    }
}
