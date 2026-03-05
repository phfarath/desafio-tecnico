import { api } from "./http";
import type {
  AdesaoRequest,
  AdesaoResponse,
  AlterarValorMensalRequest,
  CarteiraResponse,
  ClienteResumoResponse,
  CriarCestaRequest,
  CestaResponse,
  CustodiaMasterResponse,
  ExecutarCompraResponse,
  RebalanceamentoResponse,
  RentabilidadeResponse,
} from "./types";

export async function listarClientes(params?: { ativo?: boolean; nome?: string }) {
  const response = await api.get<ClienteResumoResponse[]>("/api/clientes", { params });
  return response.data;
}

export async function aderirCliente(payload: AdesaoRequest) {
  const response = await api.post<AdesaoResponse>("/api/clientes/adesao", payload);
  return response.data;
}

export async function alterarValorMensal(clienteId: number, payload: AlterarValorMensalRequest) {
  await api.put(`/api/clientes/${clienteId}/valor-mensal`, payload);
}

export async function sairDoPrograma(clienteId: number) {
  await api.post(`/api/clientes/${clienteId}/saida`);
}

export async function obterCarteira(clienteId: number) {
  const response = await api.get<CarteiraResponse>(`/api/clientes/${clienteId}/carteira`);
  return response.data;
}

export async function obterRentabilidade(clienteId: number) {
  const response = await api.get<RentabilidadeResponse>(`/api/clientes/${clienteId}/rentabilidade`);
  return response.data;
}

export async function obterCestaAtual() {
  const response = await api.get<CestaResponse>("/api/admin/cesta/atual");
  return response.data;
}

export async function obterHistoricoCestas() {
  const response = await api.get<CestaResponse[]>("/api/admin/cesta/historico");
  return response.data;
}

export async function criarCesta(payload: CriarCestaRequest) {
  const response = await api.post<CestaResponse>("/api/admin/cesta", payload);
  return response.data;
}

export async function executarCompra(data?: string) {
  const response = await api.post<ExecutarCompraResponse>("/api/motor/executar-compra", null, {
    params: data ? { data } : undefined,
  });
  return response.data;
}

export async function executarRebalanceamento(cestaAnteriorId: number, novaCestaId: number) {
  const response = await api.post<RebalanceamentoResponse>("/api/admin/rebalanceamento/executar", null, {
    params: { cestaAnteriorId, novaCestaId },
  });
  return response.data;
}

export async function executarRebalanceamentoPorDesvio(limiarDesvio: number) {
  const response = await api.post<RebalanceamentoResponse>("/api/admin/rebalanceamento/executar-desvio", null, {
    params: { limiarDesvio },
  });
  return response.data;
}

export async function obterCustodiaMaster() {
  const response = await api.get<CustodiaMasterResponse>("/api/admin/conta-master/custodia");
  return response.data;
}
