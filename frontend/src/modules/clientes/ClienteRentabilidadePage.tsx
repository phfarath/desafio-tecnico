import { useQuery } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";
import { Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { obterRentabilidade } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency, formatPercent } from "../../shared/lib/format/formatters";

export function ClienteRentabilidadePage() {
  const { id } = useParams();
  const clienteId = Number(id);

  const query = useQuery({
    queryKey: ["rentabilidade", clienteId],
    queryFn: () => obterRentabilidade(clienteId),
    enabled: Number.isFinite(clienteId),
  });

  return (
    <section className="page">
      <div className="card">
        <h2>Rentabilidade</h2>
        <Link to="/clientes">
          <button type="button" className="secondary">Voltar</button>
        </Link>
      </div>

      <div className="card">
        {query.isLoading && <p>Carregando rentabilidade...</p>}
        {query.isError && <p className="error">{errorMessage(query.error)}</p>}
        {query.data && (
          <>
            <p><strong>Cliente:</strong> {query.data.nomeCliente}</p>
            <p><strong>Investido total:</strong> {formatCurrency(query.data.valorTotalInvestido)}</p>
            <p><strong>Carteira atual:</strong> {formatCurrency(query.data.valorAtualCarteira)}</p>
            <p><strong>Rentabilidade:</strong> {formatPercent(query.data.rentabilidadePercent)}</p>

            <h3>Evolucao da carteira</h3>
            <div style={{ width: "100%", height: 280 }}>
              <ResponsiveContainer>
                <LineChart data={query.data.evolucaoCarteira}>
                  <XAxis dataKey="data" />
                  <YAxis />
                  <Tooltip />
                  <Line type="monotone" dataKey="valorInvestidoAcumulado" stroke="#0f6cbf" name="Investido" />
                  <Line type="monotone" dataKey="valorCarteira" stroke="#11a868" name="Carteira" />
                </LineChart>
              </ResponsiveContainer>
            </div>

            <h3>Historico de aportes</h3>
            <table>
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Parcela</th>
                  <th>Valor</th>
                </tr>
              </thead>
              <tbody>
                {query.data.historicoAportes.map((item, index) => (
                  <tr key={`${item.data}-${index}`}>
                    <td>{item.data}</td>
                    <td>{item.parcela}</td>
                    <td>{formatCurrency(item.valorAporte)}</td>
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
