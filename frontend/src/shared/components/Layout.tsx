import { NavLink } from "react-router-dom";
import type { PropsWithChildren } from "react";

const links = [
  { to: "/", label: "Dashboard" },
  { to: "/clientes", label: "Clientes" },
  { to: "/clientes/novo", label: "Nova adesao" },
  { to: "/admin/cesta", label: "Cesta" },
  { to: "/admin/cestas/historico", label: "Historico" },
  { to: "/admin/motor", label: "Motor" },
  { to: "/admin/rebalanceamento", label: "Rebalanceamento" },
  { to: "/admin/master", label: "Custodia master" },
];

export function Layout({ children }: PropsWithChildren) {
  return (
    <div className="shell">
      <header className="topbar">
        <h1>Compra Programada</h1>
        <nav>
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
      </header>
      <main className="content">{children}</main>
    </div>
  );
}
