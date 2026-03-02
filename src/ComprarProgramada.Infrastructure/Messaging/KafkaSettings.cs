namespace ComprarProgramada.Infrastructure.Messaging;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = "localhost:9092";
    public string TopicoIrDedoDuro { get; init; } = "ir-compra-programada";
    public string TopicoIrVenda { get; init; } = "ir-venda-rebalanceamento";
}
