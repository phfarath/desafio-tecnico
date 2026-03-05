import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { executarCompra } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";
import { formatCurrency } from "../../shared/lib/format/formatters";

export function AdminMotorPage() {
  const [data, setData] = useState("");

  const mutation = useMutation({ mutationFn: executarCompra });

  return (
    <section className="page">
      <div className="card">
        <h2>Motor de compra</h2>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            mutation.mutate(data || undefined);
          }}
        >
          <label>
            Data de execucao (opcional)
            <input type="date" value={data} onChange={(e) => setData(e.target.value)} />
          </label>
          <button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Executando..." : "Executar compra"}
          </button>
        </form>

        {mutation.isError && <p className="error">{errorMessage(mutation.error)}</p>}
      </div>

      {mutation.data && (
        <div className="card">
          <h3>Resultado</h3>
          <p>Ordem: {mutation.data.ordemCompraId}</p>
          <p>Total consolidado: {formatCurrency(mutation.data.valorTotalConsolidado)}</p>
          <p>Total clientes: {mutation.data.totalClientes}</p>

          <table>
            <thead>
              <tr>
                <th>Ticker</th>
                <th>Qtd total</th>
                <th>Lote</th>
                <th>Fracionario</th>
                <th>Preco</th>
                <th>Valor</th>
              </tr>
            </thead>
            <tbody>
              {mutation.data.itensComprados.map((item) => (
                <tr key={item.ticker}>
                  <td>{item.ticker}</td>
                  <td>{item.quantidadeTotal}</td>
                  <td>{item.quantidadeLotePadrao}</td>
                  <td>{item.quantidadeFracionario}</td>
                  <td>{formatCurrency(item.precoUnitario)}</td>
                  <td>{formatCurrency(item.valorTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
