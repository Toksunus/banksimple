namespace PaymentService.Domain.Interfaces;

public interface IKafkaMessenger
{
    Task PublishAsync(string requestType, string id, string status);
}
