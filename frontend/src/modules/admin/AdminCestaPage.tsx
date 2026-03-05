import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { criarCesta, obterCestaAtual } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";

const emptyItens = [
  { ticker: "", percentual: 0 },
  { ticker: "", percentual: 0 },
  { ticker: "", percentual: 0 },
  { ticker: "", percentual: 0 },
  { ticker: "", percentual: 0 },
];

export function AdminCestaPage() {
  const queryClient = useQueryClient();
  const [nome, setNome] = useState("Top Five");
  const [itens, setItens] = useState(emptyItens);

  const soma = useMemo(() => itens.reduce((acc, item) => acc + Number(item.percentual || 0), 0), [itens]);

  const cestaQuery = useQuery({ queryKey: ["cestaAtual"], queryFn: obterCestaAtual });

  const mutation = useMutation({
    mutationFn: criarCesta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cestaAtual"] });
      queryClient.invalidateQueries({ queryKey: ["historicoCestas"] });
    },
  });

  return (
    <section className="page">
      <div className="card">
        <h2>Cesta Top Five</h2>
        {cestaQuery.isLoading && <p>Carregando cesta ativa...</p>}
        {cestaQuery.isError && <p className="error">{errorMessage(cestaQuery.error)}</p>}
        {cestaQuery.data && (
          <p>
            <strong>Ativa:</strong> {cestaQuery.data.nome} - {cestaQuery.data.itens.map((i) => `${i.ticker} ${i.percentual}%`).join(" | ")}
          </p>
        )}
      </div>

      <div className="card">
        <h3>Criar nova cesta</h3>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            mutation.mutate({
              nome,
              itens: itens.map((i) => ({ ticker: i.ticker.trim().toUpperCase(), percentual: Number(i.percentual) })),
            });
          }}
        >
          <label>
            Nome da cesta
            <input value={nome} onChange={(e) => setNome(e.target.value)} />
          </label>

          {itens.map((item, index) => (
            <div className="grid" key={index}>
              <label>
                Ticker {index + 1}
                <input
                  value={item.ticker}
                  onChange={(e) =>
                    setItens((prev) => prev.map((it, i) => (i === index ? { ...it, ticker: e.target.value } : it)))
                  }
                />
              </label>
              <label>
                Percentual
                <input
                  type="number"
                  step="0.01"
                  value={item.percentual}
                  onChange={(e) =>
                    setItens((prev) => prev.map((it, i) => (i === index ? { ...it, percentual: Number(e.target.value) } : it)))
                  }
                />
              </label>
            </div>
          ))}

          <p>Soma atual: {soma.toFixed(2)}%</p>

          <button type="submit" disabled={mutation.isPending || soma !== 100}>
            {mutation.isPending ? "Salvando..." : "Salvar cesta"}
          </button>
        </form>

        {mutation.isError && <p className="error">{errorMessage(mutation.error)}</p>}
        {mutation.isSuccess && <p className="success">Cesta criada com sucesso.</p>}
      </div>
    </section>
  );
}
