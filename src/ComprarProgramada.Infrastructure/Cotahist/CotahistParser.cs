using System.Text;

namespace ComprarProgramada.Infrastructure.Cotahist;

/// <summary>
/// Faz parse do arquivo COTAHIST da B3 (layout posicional, 245 colunas, ISO-8859-1).
/// Processa apenas registros TIPREG=01, CODBDI=02 ou 96, TPMERC=010 ou 020.
/// </summary>
public static class CotahistParser
{
    private static readonly Encoding Iso88591;

    static CotahistParser()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Iso88591 = Encoding.GetEncoding("ISO-8859-1");
    }

    /// <summary>
    /// Lê o arquivo de forma lazy e retorna apenas cotações de lote padrão e fracionário
    /// dos mercados à vista (010) e fracionário (020).
    /// </summary>
    public static IEnumerable<CotacaoB3> Parse(string caminhoArquivo)
    {
        foreach (var linha in File.ReadLines(caminhoArquivo, Iso88591))
        {
            if (linha.Length < 245)
                continue;

            if (linha[..2] != "01")
                continue;

            var codbdi = linha.Substring(10, 2);
            if (codbdi != "02" && codbdi != "96")
                continue;

            if (!int.TryParse(linha.Substring(24, 3), out var tpmerc))
                continue;

            if (tpmerc != 10 && tpmerc != 20)
                continue;

            yield return new CotacaoB3
            {
                DataPregao  = DateOnly.ParseExact(linha.Substring(2, 8), "yyyyMMdd"),
                CodigoBdi   = codbdi,
                Ticker      = linha.Substring(12, 12).Trim(),
                TipoMercado = tpmerc,
                PrecoAbertura   = ParsePreco(linha.Substring(56, 13)),
                PrecoMaximo     = ParsePreco(linha.Substring(69, 13)),
                PrecoMinimo     = ParsePreco(linha.Substring(82, 13)),
                PrecoFechamento = ParsePreco(linha.Substring(108, 13)),
            };
        }
    }

    private static decimal ParsePreco(string campo) =>
        long.TryParse(campo.Trim(), out var v) ? v / 100m : 0m;
}
