export interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  instance?: string;
}

export interface ClienteResumoResponse {
  clienteId: number;
  nome: string;
  cpfMascarado: string;
  ativo: boolean;
  valorMensal: number;
  numeroConta: string;
  dataAdesao: string;
}

export interface AdesaoRequest {
  nome: string;
  cpf: string;
  email: string;
  valorMensal: number;
}

export interface AdesaoResponse {
  clienteId: number;
  nome: string;
  cpf: string;
  email: string;
  valorMensal: number;
  valorParcela: number;
  numeroConta: string;
  dataAdesao: string;
}

export interface AlterarValorMensalRequest {
  novoValorMensal: number;
}

export interface CarteiraResponse {
  clienteId: number;
  nomeCliente: string;
  numeroConta: string;
  valorTotalInvestido: number;
  valorAtualCarteira: number;
  rentabilidadePercent: number;
  posicoes: PosicaoItem[];
}

export interface PosicaoItem {
  ticker: string;
  quantidade: number;
  precoMedio: number;
  precoAtual: number;
  valorInvestido: number;
  valorAtual: number;
  lucroPrejuizo: number;
  rentabilidadePercent: number;
}

export interface RentabilidadeResponse {
  clienteId: number;
  nomeCliente: string;
  valorTotalInvestido: number;
  valorAtualCarteira: number;
  rentabilidadePercent: number;
  historicoAportes: HistoricoAporteItem[];
  evolucaoCarteira: EvolucaoCarteiraItem[];
}

export interface HistoricoAporteItem {
  data: string;
  valorAporte: number;
  parcela: string;
}

export interface EvolucaoCarteiraItem {
  data: string;
  valorInvestidoAcumulado: number;
  valorCarteira: number;
}

export interface CestaItemRequest {
  ticker: string;
  percentual: number;
}

export interface CriarCestaRequest {
  nome: string;
  itens: CestaItemRequest[];
}

export interface CestaItemResponse {
  ticker: string;
  percentual: number;
}

export interface CestaResponse {
  id: number;
  nome: string;
  ativa: boolean;
  dataCriacao: string;
  dataDesativacao?: string;
  itens: CestaItemResponse[];
}

export interface ExecutarCompraResponse {
  ordemCompraId: number;
  dataCompra: string;
  valorTotalConsolidado: number;
  totalClientes: number;
  itensComprados: ItemCompradoResponse[];
  residuosMaster: ResidualMasterResponse[];
}

export interface ItemCompradoResponse {
  ticker: string;
  quantidadeLotePadrao: number;
  quantidadeFracionario: number;
  quantidadeTotal: number;
  precoUnitario: number;
  valorTotal: number;
}

export interface ResidualMasterResponse {
  ticker: string;
  quantidade: number;
}

export interface RebalanceamentoResponse {
  totalClientes: number;
  totalClientesComVendas: number;
  totalVendasGeral: number;
  totalIrPublicado: number;
  detalhes: RebalanceamentoClienteResponse[];
}

export interface RebalanceamentoClienteResponse {
  clienteId: number;
  nomeCliente: string;
  totalVendas: number;
  lucroLiquido: number;
  irPublicado: boolean;
  valorIr: number;
}

export interface CustodiaMasterResponse {
  itens: CustodiaMasterItemResponse[];
  valorTotalResiduo: number;
}

export interface CustodiaMasterItemResponse {
  ticker: string;
  quantidade: number;
  precoMedio: number;
  valorTotal: number;
}
