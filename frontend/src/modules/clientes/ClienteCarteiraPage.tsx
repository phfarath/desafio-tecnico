import { useQuery } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";
import { obterCarteira } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency, formatPercent } from "../../shared/lib/format/formatters";

export function ClienteCarteiraPage() {
  const { id } = useParams();
  const clienteId = Number(id);

  const query = useQuery({
    queryKey: ["carteira", clienteId],
    queryFn: () => obterCarteira(clienteId),
    enabled: Number.isFinite(clienteId),
  });

  return (
    <section className="page">
      <div className="card">
        <h2>Carteira do cliente</h2>
        <Link to="/clientes">
          <button type="button" className="secondary">Voltar</button>
        </Link>
      </div>

      <div className="card">
        {query.isLoading && <p>Carregando carteira...</p>}
        {query.isError && <p className="error">{errorMessage(query.error)}</p>}
        {query.data && (
          <>
            <p><strong>Cliente:</strong> {query.data.nomeCliente}</p>
            <p><strong>Conta:</strong> {query.data.numeroConta}</p>
            <p><strong>Investido:</strong> {formatCurrency(query.data.valorTotalInvestido)}</p>
            <p><strong>Atual:</strong> {formatCurrency(query.data.valorAtualCarteira)}</p>
            <p><strong>Rentabilidade:</strong> {formatPercent(query.data.rentabilidadePercent)}</p>

            <table>
              <thead>
                <tr>
                  <th>Ticker</th>
                  <th>Qtd</th>
                  <th>PM</th>
                  <th>Preco atual</th>
                  <th>Investido</th>
                  <th>Atual</th>
                  <th>Lucro/Prejuizo</th>
                  <th>Rentab</th>
                </tr>
              </thead>
              <tbody>
                {query.data.posicoes.map((item) => (
                  <tr key={item.ticker}>
                    <td>{item.ticker}</td>
                    <td>{item.quantidade}</td>
                    <td>{formatCurrency(item.precoMedio)}</td>
                    <td>{formatCurrency(item.precoAtual)}</td>
                    <td>{formatCurrency(item.valorInvestido)}</td>
                    <td>{formatCurrency(item.valorAtual)}</td>
                    <td>{formatCurrency(item.lucroPrejuizo)}</td>
                    <td>{formatPercent(item.rentabilidadePercent)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}
      </div>
    </section>
  );
}
