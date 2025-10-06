import React from "react";

type QueryProviderProps = {
  children: React.ReactNode;
};

export const QueryProvider: React.FC<QueryProviderProps> = ({ children }) => {
  // You can add your query client/provider logic here
  return <>{children}</>;
};