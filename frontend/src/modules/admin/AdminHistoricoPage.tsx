import { useQuery } from "@tanstack/react-query";
import { obterHistoricoCestas } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatDate } from "../../shared/lib/format/formatters";

export function AdminHistoricoPage() {
  const query = useQuery({ queryKey: ["historicoCestas"], queryFn: obterHistoricoCestas });

  return (
    <section className="page">
      <div className="card">
        <h2>Historico de cestas</h2>
      </div>

      <div className="card">
        {query.isLoading && <p>Carregando historico...</p>}
        {query.isError && <p className="error">{errorMessage(query.error)}</p>}
        {query.data && (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Nome</th>
                <th>Status</th>
                <th>Criacao</th>
                <th>Desativacao</th>
                <th>Itens</th>
              </tr>
            </thead>
            <tbody>
              {query.data.map((cesta) => (
                <tr key={cesta.id}>
                  <td>{cesta.id}</td>
                  <td>{cesta.nome}</td>
                  <td>{cesta.ativa ? "Ativa" : "Inativa"}</td>
                  <td>{formatDate(cesta.dataCriacao)}</td>
                  <td>{cesta.dataDesativacao ? formatDate(cesta.dataDesativacao) : "-"}</td>
                  <td>{cesta.itens.map((i) => `${i.ticker} ${i.percentual}%`).join(" | ")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}
