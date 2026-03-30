using Confluent.Kafka;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Messaging;

public class KafkaMessenger : IKafkaMessenger, IDisposable
{
    private readonly IProducer<string, string> _messenger;

    public KafkaMessenger(string bbcUrl)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bbcUrl
        };
        
        _messenger = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string requestType, string id, string status)
    {
        await _messenger.ProduceAsync(requestType, new Message<string, string>
        {
            Key = id,
            Value = status
        });
    }

    public void Dispose()
    {
        _messenger.Flush(TimeSpan.FromSeconds(5));
        _messenger.Dispose();
    }
}
