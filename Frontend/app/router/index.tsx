import React from "react";
import { createBrowserRouter } from "react-router-dom";
import RegisterUnitPage from "../../features/auth/pages/RegisterUnitPage";
import Login from "../../pages/Login/Login";
import Agendamento from "../../features/scheduling/Agendamento";
import Menu from "../../features/menu/Menu";
import Register from "../../pages/Register/Register";
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
  {
    path: "/register",
    element: <Register />,
  },
  // ... outras rotas existentes
]);

export default router;