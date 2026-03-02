namespace ComprarProgramada.Domain.Interfaces.Services;

/// <summary>
/// Publica eventos de domínio em tópicos Kafka (IR dedo-duro, IR sobre vendas etc.).
/// </summary>
public interface IEventPublisher
{
    Task PublicarAsync<T>(
        string topico,
        string chave,
        T mensagem,
        CancellationToken ct = default);
}
