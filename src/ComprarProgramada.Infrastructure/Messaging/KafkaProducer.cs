using System.Text.Json;
using Confluent.Kafka;
using ComprarProgramada.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprarProgramada.Infrastructure.Messaging;

public sealed class KafkaProducer : IEventPublisher, IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public KafkaProducer(IOptions<KafkaSettings> settings, ILogger<KafkaProducer> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 500
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublicarAsync<T>(
        string topico,
        string chave,
        T mensagem,
        CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(mensagem, JsonOptions);

        var kafkaMessage = new Message<string, string>
        {
            Key = chave,
            Value = payload
        };

        try
        {
            var result = await _producer.ProduceAsync(topico, kafkaMessage, ct);

            _logger.LogInformation(
                "Mensagem publicada no tópico {Topico} [partição {Particao}, offset {Offset}] chave={Chave}",
                result.Topic, result.Partition.Value, result.Offset.Value, chave);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Falha ao publicar mensagem no tópico {Topico} chave={Chave}: {Erro}",
                topico, chave, ex.Error.Reason);
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}
