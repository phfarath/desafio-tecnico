# Compra Programada de Acoes - Itau Corretora

Sistema de compra programada de acoes desenvolvido como desafio tecnico.

## Pre-requisitos

- .NET 10 SDK
- Node.js 20+
- npm 10+
- Docker + Docker Compose (para MySQL + Kafka)
- PowerShell 5+ (Windows nativo) ou Bash (Linux/macOS/WSL/Git Bash)

## Quick Start (Backend + Frontend)

1. Suba a infraestrutura:

```bash
docker-compose up -d
```

2. Aplique migrations:

```bash
dotnet ef database update \
  --project src/ComprarProgramada.Infrastructure \
  --startup-project src/ComprarProgramada.API
```

3. Rode API + frontend com um comando:

Windows (PowerShell):
```powershell
.\run-dev.ps1
```

Linux/macOS/WSL/Git Bash:
```bash
./run-dev.sh
```

4. Abra no navegador:

- Swagger API: `http://localhost:5079/swagger`
- Frontend: `http://localhost:5173`

## Scripts de execucao local

### Windows nativo (PowerShell)

O script [`run-dev.ps1`](./run-dev.ps1):

- inicia a API em `http://localhost:5079`
- inicia o frontend em `http://localhost:5173`
- injeta `VITE_API_BASE_URL=http://localhost:5079` no processo do frontend
- encerra os dois processos ao pressionar `Ctrl+C`

Se o PowerShell bloquear execucao de scripts:

```powershell
powershell -ExecutionPolicy Bypass -File .\run-dev.ps1
```

### Linux/macOS/WSL/Git Bash

O script [`run-dev.sh`](./run-dev.sh):

- inicia a API em `http://localhost:5079`
- inicia o frontend em `http://localhost:5173`
- injeta `VITE_API_BASE_URL=http://localhost:5079` no processo do frontend
- encerra os dois processos ao pressionar `Ctrl+C`

## Rodar manualmente (alternativa)

### API

```bash
dotnet run --project src/ComprarProgramada.API --urls "http://localhost:5079"
```

### Frontend

PowerShell:
```powershell
cd frontend
npm install
$env:VITE_API_BASE_URL="http://localhost:5079"
npm run dev -- --host localhost --port 5173
```

Bash:
```bash
cd frontend
npm install
VITE_API_BASE_URL=http://localhost:5079 npm run dev -- --host localhost --port 5173
```

## Endpoints disponiveis

### Clientes

- `GET /api/clientes` (CRIADA PARA FRONTEND)
- `POST /api/clientes/adesao`
- `PUT /api/clientes/{id}/valor-mensal`
- `POST /api/clientes/{id}/saida`
- `GET /api/clientes/{id}/carteira`
- `GET /api/clientes/{id}/rentabilidade`

### Administracao

- `POST /api/admin/cesta`
- `GET /api/admin/cesta/atual`
- `GET /api/admin/cesta/historico`
- `POST /api/admin/rebalanceamento/executar`
- `POST /api/admin/rebalanceamento/executar-desvio`
- `GET /api/admin/conta-master/custodia`

### Motor

- `POST /api/motor/executar-compra`

## Testes

```bash
dotnet test
```

Projetos de teste:

- `tests/ComprarProgramada.UnitTests`
- `tests/ComprarProgramada.IntegrationTests`
