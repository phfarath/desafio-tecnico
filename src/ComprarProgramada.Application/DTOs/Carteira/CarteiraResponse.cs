namespace ComprarProgramada.Application.DTOs.Carteira;

public sealed record CarteiraResponse(
    int ClienteId,
    string NomeCliente,
    string NumeroConta,
    decimal ValorTotalInvestido,
    decimal ValorAtualCarteira,
    decimal RentabilidadePercent,
    IReadOnlyList<PosicaoItem> Posicoes);

public sealed record PosicaoItem(
    string Ticker,
    int Quantidade,
    decimal PrecoMedio,
    decimal PrecoAtual,
    decimal ValorInvestido,
    decimal ValorAtual,
    decimal LucroPrejuizo,
    decimal RentabilidadePercent);
