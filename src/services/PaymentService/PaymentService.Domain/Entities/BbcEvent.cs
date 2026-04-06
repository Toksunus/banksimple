using System.Text.Json.Serialization;

namespace PaymentService.Domain.Entities;

public class BbcEvent
{
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("is_origin")]
    public bool IsOrigin { get; set; }
    [JsonPropertyName("transaction_id")]
    public int TransactionId { get; set; }
}
