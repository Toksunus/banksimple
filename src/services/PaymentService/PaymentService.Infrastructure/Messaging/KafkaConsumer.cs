using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Entities;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Messaging;

public class KafkaConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IKafkaMessenger _messenger;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private const string RequestTypePaymentInitiated = "payment.initiated";
    private const string RequestTypeCreditNotify = "payment.credit.notify";
    private const string RequestTypeParticipantResponse = "participant.response";

    public KafkaConsumer(
        string bbcUrl,
        string instanceId,
        IKafkaMessenger messenger,
        ILogger<KafkaConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _messenger = messenger;
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = bbcUrl,
            GroupId = instanceId
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stopRequestToken)
    {
        _consumer.Subscribe(new[] { RequestTypeCreditNotify, RequestTypePaymentInitiated });

        await Task.Run(async () =>
        {
            while (!stopRequestToken.IsCancellationRequested)
            {
                try
                {
                    var consumerResult = _consumer.Consume(stopRequestToken);

                    // result of the requestType sent to the system
                    switch (consumerResult.Topic)
                    {
                        // id and value of the message sent to the system
                        case RequestTypePaymentInitiated:
                            await HandlePaymentInitiated(consumerResult.Message.Key, consumerResult.Message.Value);
                            break;
                        // id and value of the message sent to the system
                        case RequestTypeCreditNotify:
                            await HandleCreditNotify(consumerResult.Message.Key, consumerResult.Message.Value);
                            break;
                        // case RequestTypeParticipantResponse:
                        //     // Handle participant responses if needed
                        //     break;
                    }
                    //makes sure that kafka knows that the message has been processed and can commit the offset
                    _consumer.Commit(consumerResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while consuming message");
                }
            }
        }
        , stopRequestToken);
    }

    private async Task HandlePaymentInitiated(string paymentId, string payload)
    {
        var paymentDetails = JsonSerializer.Deserialize<PaymentDetails>(payload);
        using var scope = _scopeFactory.CreateScope();
        var clientAccount = scope.ServiceProvider.GetRequiredService<IAccountServiceClient>();
        var compte = await clientAccount.GetCompteByKeyAsync(paymentDetails.FromKey);

        if(compte == null)
        {
            _logger.LogWarning("Account not found for paymentId: {PaymentId} with account key: {AccountKey}", paymentId, paymentDetails.FromKey);
            return;
        }
        
        await clientAccount.DebitAsync(compte.CompteId, paymentDetails.Amount);
        _logger.LogInformation("Account debited for paymentId: {PaymentId} with account key: {AccountKey}", paymentId, paymentDetails.FromKey);
    }

    private async Task HandleCreditNotify(string paymentId, string payload)
    {
        var paymentDetails = JsonSerializer.Deserialize<PaymentDetails>(payload);
        using var scope = _scopeFactory.CreateScope();
        var clientAccount = scope.ServiceProvider.GetRequiredService<IAccountServiceClient>();
        var compte = await clientAccount.GetCompteByKeyAsync(paymentDetails.ToKey);

        if(compte == null)
        {
            _logger.LogWarning("Account not found for paymentId: {PaymentId} with account key: {AccountKey}", paymentId, paymentDetails.ToKey);
            return;
        }

        await clientAccount.CreditAsync(compte.CompteId, paymentDetails.Amount);
        _logger.LogInformation("Account credited for paymentId: {PaymentId} with account key: {AccountKey}", paymentId, paymentDetails.ToKey);
        
    }

    // Handle responses from participants is managed by the centralized bank

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
