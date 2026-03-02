namespace ComprarProgramada.Infrastructure.Messaging.Messages;

public sealed record IrDedoDuroMessage(
    string Tipo,
    int ClienteId,
    string Cpf,
    string Ticker,
    string TipoOperacao,
    int Quantidade,
    decimal PrecoUnitario,
    decimal ValorOperacao,
    decimal Aliquota,
    decimal ValorIr,
    DateTime DataOperacao
)
{
    public static IrDedoDuroMessage Criar(
        int clienteId,
        string cpf,
        string ticker,
        int quantidade,
        decimal precoUnitario,
        decimal valorOperacao,
        decimal valorIr) =>
        new(
            Tipo: "IR_DEDO_DURO",
            ClienteId: clienteId,
            Cpf: cpf,
            Ticker: ticker,
            TipoOperacao: "COMPRA",
            Quantidade: quantidade,
            PrecoUnitario: precoUnitario,
            ValorOperacao: valorOperacao,
            Aliquota: 0.00005m,
            ValorIr: valorIr,
            DataOperacao: DateTime.UtcNow
        );
}
