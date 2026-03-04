namespace ComprarProgramada.Application.Events;

public sealed record IrVendaMessage(
    string Tipo,
    int ClienteId,
    string Cpf,
    string MesReferencia,
    decimal TotalVendasMes,
    decimal LucroLiquido,
    decimal Aliquota,
    decimal ValorIr,
    IReadOnlyList<DetalheVendaItem> Detalhes,
    DateTime DataCalculo)
{
    public static IrVendaMessage Criar(
        int clienteId,
        string cpf,
        string mesReferencia,
        decimal totalVendasMes,
        decimal lucroLiquido,
        decimal valorIr,
        IReadOnlyList<DetalheVendaItem> detalhes) =>
        new(
            Tipo: "IR_VENDA",
            ClienteId: clienteId,
            Cpf: cpf,
            MesReferencia: mesReferencia,
            TotalVendasMes: totalVendasMes,
            LucroLiquido: lucroLiquido,
            Aliquota: 0.20m,
            ValorIr: valorIr,
            Detalhes: detalhes,
            DataCalculo: DateTime.UtcNow);
}

public sealed record DetalheVendaItem(
    string Ticker,
    int Quantidade,
    decimal PrecoVenda,
    decimal PrecoMedio,
    decimal Lucro);
