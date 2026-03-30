using System.Text.Json.Serialization;

namespace PaymentService.Domain.Entities;

public class PaymentDetails
{
    [JsonPropertyName("from_key")]
    public string FromKey { get; set; }
    [JsonPropertyName("to_key")]
    public string ToKey { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}