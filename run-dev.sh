#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_URL="http://localhost:5079"
FRONTEND_URL="http://localhost:5173"

API_PID=""
FRONT_PID=""

resolve_dotnet() {
  if command -v dotnet >/dev/null 2>&1; then
    echo "dotnet"
    return 0
  fi

  if [[ -x "/c/Program Files/dotnet/dotnet.exe" ]]; then
    echo "/c/Program Files/dotnet/dotnet.exe"
    return 0
  fi

  if [[ -x "/mnt/c/Program Files/dotnet/dotnet.exe" ]]; then
    echo "/mnt/c/Program Files/dotnet/dotnet.exe"
    return 0
  fi

  return 1
}

cleanup() {
  if [[ -n "$API_PID" ]] && kill -0 "$API_PID" 2>/dev/null; then
    kill "$API_PID" 2>/dev/null || true
  fi

  if [[ -n "$FRONT_PID" ]] && kill -0 "$FRONT_PID" 2>/dev/null; then
    kill "$FRONT_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT INT TERM

if ! DOTNET_BIN="$(resolve_dotnet)"; then
  echo "Erro: nao foi possivel localizar o dotnet."
  echo "No Git Bash, instale o .NET SDK ou adicione C:/Program Files/dotnet ao PATH."
  exit 1
fi

if ! command -v npm >/dev/null 2>&1; then
  echo "Erro: npm nao encontrado no PATH."
  exit 1
fi

echo "[1/2] Iniciando API em $API_URL"
(
  cd "$ROOT_DIR"
  ASPNETCORE_ENVIRONMENT=Development "$DOTNET_BIN" run --project src/ComprarProgramada.API --urls "$API_URL"
) &
API_PID=$!

echo "[2/2] Iniciando frontend em $FRONTEND_URL"
(
  cd "$ROOT_DIR/frontend"

  if [[ ! -d node_modules ]]; then
    echo "Instalando dependencias do frontend..."
    npm install
  fi

  VITE_API_BASE_URL="$API_URL" npm run dev -- --host localhost --port 5173
) &
FRONT_PID=$!

echo ""
echo "Aplicacao em execucao:"
echo "- Swagger:  $API_URL/swagger"
echo "- Frontend: $FRONTEND_URL"
echo ""
echo "Pressione Ctrl+C para encerrar ambos os processos."

wait
