using ComprarProgramada.Domain.Entities.Base;

namespace ComprarProgramada.Domain.Entities;

/// <summary>
/// Conta da corretora onde as compras são consolidadas antes da distribuição.
/// Existe uma única instância no sistema.
/// </summary>
public class ContaMaster : Entity
{
    public string NumeroConta { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }

    // Navigation
    public CustodiaMaster? Custodia { get; private set; }

    protected ContaMaster() { }

    public static ContaMaster Criar()
    {
        return new ContaMaster
        {
            NumeroConta = "MST-000001",
            DataCriacao = DateTime.UtcNow
        };
    }
}
