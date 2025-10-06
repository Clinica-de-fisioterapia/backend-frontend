import React from "react";
import { createBrowserRouter } from "react-router-dom";
import LoginPage from "../../features/auth/pages/LoginPage";
import RegisterUnitPage from "../../features/auth/pages/RegisterUnitPage";
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
  // ... outras rotas existentes
]);
