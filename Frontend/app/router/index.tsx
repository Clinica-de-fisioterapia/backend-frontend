import React from "react";
import { createBrowserRouter } from "react-router-dom";
import LoginPage from "../../features/auth/pages/LoginPage";
import RegisterUnitPage from "../../features/auth/pages/RegisterUnitPage";
import Login from "../../pages/Login/Login";
import Agendamento from "../../features/scheduling/Agendamento";
// ... outras imports

export const router = createBrowserRouter([
  {
    path: "/",
    element: <LoginPage />,
  },
  {
    path: "/cadastro-unidade",
    element: <RegisterUnitPage />,
  },
  {
    path: "/login",
    element: <Login />,
  },
  {
    path: "/agendamento",
    element: <Agendamento />,
  },
  // ... outras rotas existentes
]);

export default router;