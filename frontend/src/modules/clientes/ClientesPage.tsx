import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { alterarValorMensal, listarClientes, sairDoPrograma } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency, formatDate } from "../../shared/lib/format/formatters";

export function ClientesPage() {
  const queryClient = useQueryClient();
  const [filtroNome, setFiltroNome] = useState("");
  const [filtroAtivo, setFiltroAtivo] = useState<string>("todos");
  const [novoAporte, setNovoAporte] = useState<Record<number, string>>({});

  const ativoFiltro = useMemo(() => {
    if (filtroAtivo === "ativos") return true;
    if (filtroAtivo === "inativos") return false;
    return undefined;
  }, [filtroAtivo]);

  const clientesQuery = useQuery({
    queryKey: ["clientes", filtroNome, ativoFiltro],
    queryFn: () => listarClientes({ nome: filtroNome || undefined, ativo: ativoFiltro }),
  });

  const alterarMutation = useMutation({
    mutationFn: ({ clienteId, valor }: { clienteId: number; valor: number }) =>
      alterarValorMensal(clienteId, { novoValorMensal: valor }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["clientes"] }),
  });

  const sairMutation = useMutation({
    mutationFn: (clienteId: number) => sairDoPrograma(clienteId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["clientes"] }),
  });

  return (
    <section className="page">
      <div className="card">
        <h2>Clientes</h2>
        <div className="actions">
          <input
            placeholder="Filtrar por nome"
            value={filtroNome}
            onChange={(e) => setFiltroNome(e.target.value)}
          />
          <select value={filtroAtivo} onChange={(e) => setFiltroAtivo(e.target.value)}>
            <option value="todos">Todos</option>
            <option value="ativos">Ativos</option>
            <option value="inativos">Inativos</option>
          </select>
          <Link to="/clientes/novo">
            <button type="button">Nova adesao</button>
          </Link>
        </div>
        {(alterarMutation.isError || sairMutation.isError) && (
          <p className="error">{errorMessage(alterarMutation.error ?? sairMutation.error)}</p>
        )}
      </div>

      <div className="card">
        {clientesQuery.isLoading && <p>Carregando clientes...</p>}
        {clientesQuery.isError && <p className="error">{errorMessage(clientesQuery.error)}</p>}
        {clientesQuery.data && (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Nome</th>
                <th>CPF</th>
                <th>Status</th>
                <th>Conta</th>
                <th>Aporte</th>
                <th>Adesao</th>
                <th>Acoes</th>
              </tr>
            </thead>
            <tbody>
              {clientesQuery.data.map((cliente) => (
                <tr key={cliente.clienteId}>
                  <td>{cliente.clienteId}</td>
                  <td>{cliente.nome}</td>
                  <td>{cliente.cpfMascarado}</td>
                  <td>{cliente.ativo ? "Ativo" : "Inativo"}</td>
                  <td>{cliente.numeroConta || "-"}</td>
                  <td>{formatCurrency(cliente.valorMensal)}</td>
                  <td>{formatDate(cliente.dataAdesao)}</td>
                  <td>
                    <div className="actions">
                      <Link to={`/clientes/${cliente.clienteId}/carteira`}>
                        <button type="button" className="secondary">Carteira</button>
                      </Link>
                      <Link to={`/clientes/${cliente.clienteId}/rentabilidade`}>
                        <button type="button" className="secondary">Rentabilidade</button>
                      </Link>
                      <input
                        type="number"
                        min="100"
                        placeholder="Novo aporte"
                        value={novoAporte[cliente.clienteId] ?? ""}
                        onChange={(e) =>
                          setNovoAporte((prev) => ({ ...prev, [cliente.clienteId]: e.target.value }))
                        }
                      />
                      <button
                        type="button"
                        onClick={() => {
                          const valor = Number(novoAporte[cliente.clienteId]);
                          if (!Number.isFinite(valor) || valor < 100) return;
                          alterarMutation.mutate({ clienteId: cliente.clienteId, valor });
                        }}
                      >
                        Alterar aporte
                      </button>
                      <button
                        type="button"
                        className="danger"
                        disabled={!cliente.ativo}
                        onClick={() => sairMutation.mutate(cliente.clienteId)}
                      >
                        Saida
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}
