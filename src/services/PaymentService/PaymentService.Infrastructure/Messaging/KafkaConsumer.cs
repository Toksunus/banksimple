using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Messaging;

public class KafkaConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private const string EventTypeOrigin = "CREATE_TRANSACTION_ORIGIN";
    private const string EventTypeDestination = "CREATE_TRANSACTION_DESTINATION";
    private const string EventTypeCompensate = "COMPENSATE_ORIGIN";

    public KafkaConsumer(
        string brokerUrl,
        string groupId,
        string bankId,
        ILogger<KafkaConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = brokerUrl,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe($"bbc.events.{bankId}");
    }

    protected override async Task ExecuteAsync(CancellationToken stopRequestToken)
    {
        await Task.Run(async () =>
        {
            while (!stopRequestToken.IsCancellationRequested)
            {
                try
                {
                    var consumerResult = _consumer.Consume(stopRequestToken);
                    var bbcEvent = JsonSerializer.Deserialize<BbcEvent>(consumerResult.Message.Value);
                    if (bbcEvent == null) continue;

                    switch (bbcEvent.EventType)
                    {
                        case EventTypeOrigin:
                            await HandleOrigin(bbcEvent);
                            break;
                        case EventTypeDestination:
                            await HandleDestination(bbcEvent);
                            break;
                        case EventTypeCompensate:
                            await HandleCompensate(bbcEvent);
                            break;
                    }
                    _consumer.Commit(consumerResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while consuming message");
                }
            }
        }, stopRequestToken);
    }

    private async Task HandleOrigin(BbcEvent bbcEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var accountClient = scope.ServiceProvider.GetRequiredService<IAccountServiceClient>();
        var bbcClient = scope.ServiceProvider.GetRequiredService<IBbcServiceClient>();

        try
        {
            await accountClient.DebitAsync(bbcEvent.AccountId, bbcEvent.Amount);
            _logger.LogInformation("Compte debité pour transactionId: {TransactionId}, accountId: {AccountId}", bbcEvent.TransactionId, bbcEvent.AccountId);
            await bbcClient.ConfirmTransactionAsync(bbcEvent.AccountId, bbcEvent.Amount, bbcEvent.TransactionId, true, "CONFIRMED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debit failed for transactionId: {TransactionId}", bbcEvent.TransactionId);
            await bbcClient.ConfirmTransactionAsync(bbcEvent.AccountId, bbcEvent.Amount, bbcEvent.TransactionId, true, "FAILED");
        }
    }

    private async Task HandleDestination(BbcEvent bbcEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var accountClient = scope.ServiceProvider.GetRequiredService<IAccountServiceClient>();
        var bbcClient = scope.ServiceProvider.GetRequiredService<IBbcServiceClient>();

        try
        {
            await accountClient.CreditAsync(bbcEvent.AccountId, bbcEvent.Amount);
            _logger.LogInformation("Compte crédité pour transactionId: {TransactionId}, accountId: {AccountId}", bbcEvent.TransactionId, bbcEvent.AccountId);
            await bbcClient.ConfirmTransactionAsync(bbcEvent.AccountId, bbcEvent.Amount, bbcEvent.TransactionId, false, "CONFIRMED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crédit échoué pour transactionId: {TransactionId}", bbcEvent.TransactionId);
            await bbcClient.ConfirmTransactionAsync(bbcEvent.AccountId, bbcEvent.Amount, bbcEvent.TransactionId, false, "FAILED");
        }
    }

    private async Task HandleCompensate(BbcEvent bbcEvent)
    {
        using var scope = _scopeFactory.CreateScope();
        var accountClient = scope.ServiceProvider.GetRequiredService<IAccountServiceClient>();

        try
        {
            await accountClient.CreditAsync(bbcEvent.AccountId, bbcEvent.Amount);
            _logger.LogInformation("Compte compensé pour transactionId: {TransactionId}, accountId: {AccountId}", bbcEvent.TransactionId, bbcEvent.AccountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compensation échouée pour transactionId: {TransactionId}", bbcEvent.TransactionId);
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}
