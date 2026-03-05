import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useMutation } from "@tanstack/react-query";
import { aderirCliente } from "../../shared/lib/api/services";
import { errorMessage } from "../../shared/lib/api/http";

const schema = z.object({
  nome: z.string().min(3, "Nome obrigatorio"),
  cpf: z.string().min(11, "CPF invalido"),
  email: z.string().email("Email invalido"),
  valorMensal: z.coerce.number().min(100, "Valor minimo de R$ 100,00"),
});

type FormValues = z.infer<typeof schema>;

export function ClienteNovoPage() {
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      nome: "",
      cpf: "",
      email: "",
      valorMensal: 300,
    },
  });

  const mutation = useMutation({ mutationFn: aderirCliente });

  return (
    <section className="page">
      <div className="card">
        <h2>Nova adesao</h2>
        <form onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
          <label>
            Nome
            <input {...form.register("nome")} />
            {form.formState.errors.nome && <span className="error">{form.formState.errors.nome.message}</span>}
          </label>

          <label>
            CPF
            <input {...form.register("cpf")} placeholder="000.000.000-00" />
            {form.formState.errors.cpf && <span className="error">{form.formState.errors.cpf.message}</span>}
          </label>

          <label>
            Email
            <input type="email" {...form.register("email")} />
            {form.formState.errors.email && <span className="error">{form.formState.errors.email.message}</span>}
          </label>

          <label>
            Valor mensal
            <input type="number" min={100} step="0.01" {...form.register("valorMensal")} />
            {form.formState.errors.valorMensal && (
              <span className="error">{form.formState.errors.valorMensal.message}</span>
            )}
          </label>

          <button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? "Enviando..." : "Cadastrar cliente"}
          </button>
        </form>

        {mutation.isError && <p className="error">{errorMessage(mutation.error)}</p>}
        {mutation.isSuccess && (
          <p className="success">
            Cliente criado: ID {mutation.data.clienteId} - conta {mutation.data.numeroConta}
          </p>
        )}
      </div>
    </section>
  );
}
