import React from "react";
import { createBrowserRouter } from "react-router-dom";
import Login from "../features/auth/pages/Login";
import Register from "../features/auth/pages/Register";
import Menu from "../features/menu/Menu";
import HubRecepcionista from "../features/menu/HubRecepcionista";
import AgendaDiaria from "../features/scheduling/AgendaDiaria";
import NovoAgendamento from "../features/scheduling/NovoAgendamento";

const router = createBrowserRouter([
  { path: "/", element: <Login /> },
  { path: "/login", element: <Login /> },
  { path: "/register", element: <Register /> },
  { path: "/menu", element: <Menu /> },
  { path: "/hub", element: <HubRecepcionista /> },
  { path: "/agendamento", element: <AgendaDiaria date={new Date()} /> },
  { path: "/novo-agendamento", element: <NovoAgendamento /> },
]);

export default router;