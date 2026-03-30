using System.Net.Http.Json;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Services;

public class AccountServiceHttpClient : IAccountServiceClient
{
    private readonly HttpClient _httpClient;

    public AccountServiceHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CompteDto?> GetCompteAsync(Guid compteId)
    {
        var response = await _httpClient.GetAsync($"/internal/comptes/{compteId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompteDto>();
    }

    public async Task<CompteDto?> GetCompteByKeyAsync(string accountKey)
    {
        var response = await _httpClient.GetAsync($"/internal/comptes/key/{accountKey}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompteDto>();
    }


    public async Task<CompteDto> DebitAsync(Guid compteId, decimal montant)
    {
        var response = await _httpClient.PostAsJsonAsync($"/internal/comptes/{compteId}/debit", montant);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Debit failed: {error}");
        }
        return (await response.Content.ReadFromJsonAsync<CompteDto>())!;
    }

    public async Task<CompteDto> CreditAsync(Guid compteId, decimal montant)
    {
        var response = await _httpClient.PostAsJsonAsync($"/internal/comptes/{compteId}/credit", montant);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Credit failed: {error}");
        }
        return (await response.Content.ReadFromJsonAsync<CompteDto>())!;
    }
}
