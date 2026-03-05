import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { executarRebalanceamento, executarRebalanceamentoPorDesvio } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency } from "../../shared/lib/format/formatters";

export function AdminRebalanceamentoPage() {
  const [cestaAnteriorId, setCestaAnteriorId] = useState("");
  const [novaCestaId, setNovaCestaId] = useState("");
  const [limiar, setLimiar] = useState("5");

  const porTrocaMutation = useMutation({
    mutationFn: () => executarRebalanceamento(Number(cestaAnteriorId), Number(novaCestaId)),
  });

  const porDesvioMutation = useMutation({
    mutationFn: () => executarRebalanceamentoPorDesvio(Number(limiar)),
  });

  const resultado = porTrocaMutation.data ?? porDesvioMutation.data;

  return (
    <section className="page">
      <div className="card">
        <h2>Rebalanceamento</h2>

        <h3>Executar por troca de cesta</h3>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            porTrocaMutation.mutate();
          }}
        >
          <label>
            Cesta anterior ID
            <input type="number" value={cestaAnteriorId} onChange={(e) => setCestaAnteriorId(e.target.value)} />
          </label>
          <label>
            Nova cesta ID
            <input type="number" value={novaCestaId} onChange={(e) => setNovaCestaId(e.target.value)} />
          </label>
          <button type="submit" disabled={porTrocaMutation.isPending}>Executar por troca</button>
        </form>

        <h3>Executar por desvio</h3>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            porDesvioMutation.mutate();
          }}
        >
          <label>
            Limiar de desvio (%)
            <input type="number" step="0.1" value={limiar} onChange={(e) => setLimiar(e.target.value)} />
          </label>
          <button type="submit" disabled={porDesvioMutation.isPending}>Executar por desvio</button>
        </form>

        {(porTrocaMutation.isError || porDesvioMutation.isError) && (
          <p className="error">{errorMessage(porTrocaMutation.error ?? porDesvioMutation.error)}</p>
        )}
      </div>

      {resultado && (
        <div className="card">
          <h3>Resumo</h3>
          <p>Total clientes: {resultado.totalClientes}</p>
          <p>Clientes com vendas: {resultado.totalClientesComVendas}</p>
          <p>Total vendas: {formatCurrency(resultado.totalVendasGeral)}</p>
          <p>Total IR publicado: {formatCurrency(resultado.totalIrPublicado)}</p>

          <table>
            <thead>
              <tr>
                <th>Cliente</th>
                <th>Vendas</th>
                <th>Lucro liquido</th>
                <th>IR publicado</th>
                <th>Valor IR</th>
              </tr>
            </thead>
            <tbody>
              {resultado.detalhes.map((d) => (
                <tr key={d.clienteId}>
                  <td>{d.nomeCliente}</td>
                  <td>{formatCurrency(d.totalVendas)}</td>
                  <td>{formatCurrency(d.lucroLiquido)}</td>
                  <td>{d.irPublicado ? "Sim" : "Nao"}</td>
                  <td>{formatCurrency(d.valorIr)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
