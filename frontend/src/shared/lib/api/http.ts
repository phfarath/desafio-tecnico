import axios from "axios";
import type { ProblemDetails } from "./types";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7085",
  headers: {
    "Content-Type": "application/json",
  },
});

export function errorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ProblemDetails | undefined;
    return data?.detail ?? data?.title ?? error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return "Erro inesperado ao processar a requisicao.";
}
