import { useQuery } from "@tanstack/react-query";
import { listarClientes, obterCestaAtual, obterCustodiaMaster } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency } from "../../shared/lib/format/formatters";

export function DashboardPage() {
  const cestaQuery = useQuery({ queryKey: ["cestaAtual"], queryFn: obterCestaAtual });
  const clientesQuery = useQuery({ queryKey: ["clientesDashboard"], queryFn: () => listarClientes({ ativo: true }) });
  const masterQuery = useQuery({ queryKey: ["custodiaMaster"], queryFn: obterCustodiaMaster });

  return (
    <section className="page">
      <div className="card">
        <h2>Painel Operacional</h2>
        <p>Resumo rapido dos indicadores principais para operacao do programa.</p>
      </div>

      <div className="grid">
        <article className="card">
          <h3>Cesta ativa</h3>
          {cestaQuery.isLoading && <p>Carregando...</p>}
          {cestaQuery.isError && <p className="error">{errorMessage(cestaQuery.error)}</p>}
          {cestaQuery.data && (
            <>
              <strong>{cestaQuery.data.nome}</strong>
              <p>{cestaQuery.data.itens.map((i) => `${i.ticker} ${i.percentual}%`).join(" | ")}</p>
            </>
          )}
        </article>

        <article className="card">
          <h3>Clientes ativos</h3>
          {clientesQuery.isLoading && <p>Carregando...</p>}
          {clientesQuery.isError && <p className="error">{errorMessage(clientesQuery.error)}</p>}
          {clientesQuery.data && <p>{clientesQuery.data.length}</p>}
        </article>

        <article className="card">
          <h3>Valor residual master</h3>
          {masterQuery.isLoading && <p>Carregando...</p>}
          {masterQuery.isError && <p className="error">{errorMessage(masterQuery.error)}</p>}
          {masterQuery.data && <p>{formatCurrency(masterQuery.data.valorTotalResiduo)}</p>}
        </article>
      </div>
    </section>
  );
}
