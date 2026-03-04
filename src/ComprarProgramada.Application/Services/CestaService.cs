using ComprarProgramada.Application.DTOs.Cesta;
using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;

namespace ComprarProgramada.Application.Services;

public sealed class CestaService : ICestaService
{
    private readonly ICestaTopFiveRepository _cestas;
    private readonly IUnitOfWork _uow;

    public CestaService(ICestaTopFiveRepository cestas, IUnitOfWork uow)
    {
        _cestas = cestas;
        _uow = uow;
    }

    public async Task<CestaResponse> CriarCestaAsync(CriarCestaRequest request, CancellationToken ct = default)
    {
        // Desativa cesta anterior, se existir
        var cestaAtual = await _cestas.ObterAtivaAsync(ct);
        cestaAtual?.Desativar();

        // Cria nova cesta
        var composicao = request.Itens.Select(i => (i.Ticker, i.Percentual));
        var novaCesta = CestaTopFive.Criar(request.Nome, composicao);
        await _cestas.AdicionarAsync(novaCesta, ct);

        await _uow.CommitAsync(ct);

        return MapearParaResponse(novaCesta);
    }

    public async Task<CestaResponse> ObterCestaAtivaAsync(CancellationToken ct = default)
    {
        var cesta = await _cestas.ObterAtivaAsync(ct)
            ?? throw new InvalidOperationException("Nenhuma cesta Top Five ativa encontrada.");

        return MapearParaResponse(cesta);
    }

    private static CestaResponse MapearParaResponse(CestaTopFive cesta) =>
        new(
            cesta.Id,
            cesta.Nome,
            cesta.Ativa,
            cesta.DataCriacao,
            cesta.DataDesativacao,
            cesta.Itens
                .Select(i => new CestaItemResponse(i.Ticker.Valor, i.Percentual))
                .ToList());
}
