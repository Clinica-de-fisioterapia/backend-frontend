import React from "react";
import { createBrowserRouter } from "react-router-dom";
import RegisterUnitPage from "../../features/auth/pages/RegisterUnitPage";
import Login from "../../pages/Login/Login";
import Agendamento from "../../features/scheduling/Agendamento";
import Menu from "../../features/menu/Menu";
// ... outras imports

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Login />,
  },
  {
    path: "/menu",
    element: <Menu />,
  },
  {
    path: "/cadastro-unidade",
    element: <RegisterUnitPage />,
  },
  {
    path: "/agendamento",
    element: <Agendamento />,
  },
  // ... outras rotas existentes
]);

export default router;