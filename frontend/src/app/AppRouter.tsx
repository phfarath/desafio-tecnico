import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "../shared/components/Layout";
import { DashboardPage } from "../modules/dashboard/DashboardPage";
import { ClientesPage } from "../modules/clientes/ClientesPage";
import { ClienteNovoPage } from "../modules/clientes/ClienteNovoPage";
import { ClienteCarteiraPage } from "../modules/clientes/ClienteCarteiraPage";
import { ClienteRentabilidadePage } from "../modules/clientes/ClienteRentabilidadePage";
import { AdminCestaPage } from "../modules/admin/AdminCestaPage";
import { AdminHistoricoPage } from "../modules/admin/AdminHistoricoPage";
import { AdminMotorPage } from "../modules/admin/AdminMotorPage";
import { AdminRebalanceamentoPage } from "../modules/admin/AdminRebalanceamentoPage";
import { AdminMasterPage } from "../modules/admin/AdminMasterPage";

export function AppRouter() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/clientes" element={<ClientesPage />} />
        <Route path="/clientes/novo" element={<ClienteNovoPage />} />
        <Route path="/clientes/:id/carteira" element={<ClienteCarteiraPage />} />
        <Route path="/clientes/:id/rentabilidade" element={<ClienteRentabilidadePage />} />
        <Route path="/admin/cesta" element={<AdminCestaPage />} />
        <Route path="/admin/cestas/historico" element={<AdminHistoricoPage />} />
        <Route path="/admin/motor" element={<AdminMotorPage />} />
        <Route path="/admin/rebalanceamento" element={<AdminRebalanceamentoPage />} />
        <Route path="/admin/master" element={<AdminMasterPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}
