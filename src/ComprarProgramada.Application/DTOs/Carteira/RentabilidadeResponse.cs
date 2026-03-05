namespace ComprarProgramada.Application.DTOs.Carteira;

public sealed record RentabilidadeResponse(
    int ClienteId,
    string NomeCliente,
    decimal ValorTotalInvestido,
    decimal ValorAtualCarteira,
    decimal RentabilidadePercent,
    IReadOnlyList<HistoricoAporteItem> HistoricoAportes,
    IReadOnlyList<EvolucaoCarteiraItem> EvolucaoCarteira);

public sealed record HistoricoAporteItem(
    DateOnly Data,
    decimal ValorAporte,
    string Parcela);

public sealed record EvolucaoCarteiraItem(
    DateOnly Data,
    decimal ValorInvestidoAcumulado,
    decimal ValorCarteira);
