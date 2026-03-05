using ComprarProgramada.Domain.Entities.Base;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Conta gráfica individual criada para cada cliente no momento da adesão.
/// </summary>
public class ContaFilhote : Entity
{
    public string NumeroConta { get; private set; } = string.Empty;
    public int ClienteId { get; private set; }
    public DateTime DataCriacao { get; private set; }

    // Navigation
    public Cliente? Cliente { get; private set; }
    public CustodiaFilhote? Custodia { get; private set; }

    protected ContaFilhote() { }

    public static ContaFilhote Criar(Cliente cliente, int sequencial)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        return new ContaFilhote
        {
            Cliente = cliente,
            NumeroConta = $"FLH-{sequencial:D6}",
            DataCriacao = DateTime.UtcNow
        };
    }

    public static ContaFilhote Criar(int clienteId, int sequencial)
    {
        return new ContaFilhote
        {
            ClienteId = clienteId,
            NumeroConta = $"FLH-{sequencial:D6}",
            DataCriacao = DateTime.UtcNow
        };
    }
}
