import { useQuery } from "@tanstack/react-query";
import { obterCustodiaMaster } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency } from "../../shared/lib/format/formatters";

export function AdminMasterPage() {
  const query = useQuery({ queryKey: ["custodiaMaster"], queryFn: obterCustodiaMaster });

  return (
    <section className="page">
      <div className="card">
        <h2>Custodia Master</h2>
      </div>

      <div className="card">
        {query.isLoading && <p>Carregando custodia...</p>}
        {query.isError && <p className="error">{errorMessage(query.error)}</p>}
        {query.data && (
          <>
            <p><strong>Valor total residual:</strong> {formatCurrency(query.data.valorTotalResiduo)}</p>
            <table>
              <thead>
                <tr>
                  <th>Ticker</th>
                  <th>Quantidade</th>
                  <th>Preco medio</th>
                  <th>Valor total</th>
                </tr>
              </thead>
              <tbody>
                {query.data.itens.map((item) => (
                  <tr key={item.ticker}>
                    <td>{item.ticker}</td>
                    <td>{item.quantidade}</td>
                    <td>{formatCurrency(item.precoMedio)}</td>
                    <td>{formatCurrency(item.valorTotal)}</td>
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
