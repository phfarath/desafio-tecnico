# Compra Programada de Ações — Itaú Corretora

Sistema de compra programada de ações desenvolvido como desafio técnico.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker + Docker Compose](https://docs.docker.com/compose/)

## Como Rodar

### 1. Subir infraestrutura (MySQL + Kafka)

```bash
docker-compose up -d
```

### 2. Aplicar migrations e seed inicial

```bash
dotnet ef database update \
  --project src/ComprarProgramada.Infrastructure \
  --startup-project src/ComprarProgramada.API
```

O seed inicial cria automaticamente:
- Conta master (número `0001-MASTER`)
- Custódia master vazia

### 3. Arquivo de cotações COTAHIST (B3)

Baixe o arquivo diário do site da B3:
`https://www.b3.com.br/pt_br/market-data-e-indices/servicos-de-dados/market-data/historico/mercado-a-vista/series-historicas/`

Salve na pasta `cotacoes/` com o nome original (ex: `COTAHIST_D20260305.TXT`).

### 4. Rodar a API

```bash
dotnet run --project src/ComprarProgramada.API
```

Acesse o Swagger em: `http://localhost:5079/swagger`

### 5. Rodar o Worker (jobs Quartz)

```bash
dotnet run --project src/ComprarProgramada.Worker
```

O Worker executa o motor de compra automaticamente nos dias 5, 15 e 25 de cada mês.

---

## Arquitetura

O projeto segue **Clean Architecture + DDD**, organizado em 5 camadas:

```
Domain          → Entidades, Value Objects, Interfaces (zero dependências externas)
Application     → Casos de uso, Services, DTOs
Infrastructure  → EF Core, MySQL, COTAHIST parser, Kafka producer
API             → Controllers REST + Swagger
Worker          → Quartz scheduled jobs
```

### Diagrama de fluxo principal

```
Cliente adere → Cesta Top Five criada → Job dispara (dia 5/15/25)
   → MotorCompra: lê cotações COTAHIST
   → Compra lote padrão + fracionário na master
   → Distribui proporcionalmente para filhotes
   → Publica IR dedo-duro no Kafka
   → Resíduos ficam na custódia master para próximo ciclo
```

---

## Decisões Técnicas

### Quartz.NET para agendamento
Jobs agendados nos dias 5, 15 e 25 de cada mês via cron expression.
O Worker é um projeto separado do API para isolamento de responsabilidades.

### Apache Kafka para eventos de IR
Dois tópicos:
- `ir-dedo-duro-compra` — IR de 0,005% sobre cada compra
- `ir-venda-rebalanceamento` — IR de 20% sobre lucro em vendas > R$20.000/mês

### COTAHIST (B3) como fonte de cotações
Arquivo texto de campos fixos (245 chars/linha, ISO-8859-1).
O parser filtra `CODBDI=02` (lote padrão) e `TPMERC=010` (mercado a vista).
Preços vêm com 2 casas decimais implícitas (ex: `0000000003850` = R$ 38,50).

### Truncamento de quantidades
Quantidades de ações são sempre truncadas (`(int)(valor / preco)`), nunca arredondadas.
Resíduos financeiros ficam na custódia master e são reutilizados na próxima compra.

### Separação lote padrão / fracionário
`OrdemCompraItem.Criar()` calcula automaticamente:
- Lote padrão = `qtd / 100` lotes
- Fracionário = `qtd % 100` ações

### Rebalanceamento ao trocar a cesta
Ao criar uma nova cesta Top Five:
1. Vende ativos que saíram da cesta
2. Rebalanceia tickers que permaneceram mas mudaram de percentual
3. Compra novos tickers com o produto das vendas
4. Publica IR sobre vendas quando total mensal > R$20.000

---

## Endpoints Disponíveis

Documentação completa via Swagger (`/swagger`).

### Clientes
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/clientes/adesao` | Adesão de novo cliente |
| PUT | `/api/clientes/{id}/valor-mensal` | Alterar valor mensal de aporte |
| DELETE | `/api/clientes/{id}` | Desativar cliente |
| GET | `/api/clientes/{id}/carteira` | Carteira atual com posições e P&L |
| GET | `/api/clientes/{id}/rentabilidade` | Histórico de aportes e evolução da carteira |

### Administração
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/admin/cesta` | Criar nova cesta Top Five |
| GET | `/api/admin/cesta/ativa` | Retornar cesta ativa |
| GET | `/api/admin/cesta/historico` | Histórico de todas as cestas |
| POST | `/api/admin/rebalanceamento/executar` | Disparar rebalanceamento manualmente |
| POST | `/api/admin/rebalanceamento/executar-desvio` | Rebalancear por desvio de proporção |
| GET | `/api/admin/conta-master/custodia` | Visualizar custódia de resíduos da conta master |

### Motor
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/motor/executar` | Executar motor de compra manualmente |

---

## Como Executar os Testes

```bash
dotnet test
```

Os testes estão organizados em dois projetos:

- `ComprarProgramada.UnitTests` — testes de Domain e Application Services (sem I/O)
- `ComprarProgramada.IntegrationTests` — testes de API com WebApplicationFactory

```bash
# Somente testes unitários
dotnet test tests/ComprarProgramada.UnitTests

# Somente testes de integração
dotnet test tests/ComprarProgramada.IntegrationTests

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```
