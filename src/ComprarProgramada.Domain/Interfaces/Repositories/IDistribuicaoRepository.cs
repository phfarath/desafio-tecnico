using ComprarProgramada.Domain.Entities;

namespace ComprarProgramada.Domain.Interfaces.Repositories;

public interface IDistribuicaoRepository
{
    Task<IReadOnlyList<Distribuicao>> ObterPorOrdemAsync(int ordemCompraId, CancellationToken ct = default);
    Task<IReadOnlyList<Distribuicao>> ObterPorClienteAsync(int clienteId, CancellationToken ct = default);
    Task AdicionarAsync(Distribuicao distribuicao, CancellationToken ct = default);
    Task AdicionarVariasAsync(IEnumerable<Distribuicao> distribuicoes, CancellationToken ct = default);
}
