import React, { createContext, useContext, useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

const queryClient = new QueryClient();

type QueryContextValue = {
  queryData: any | null;
  isLoading: boolean;
  error: any | null;
};

const QueryContext = createContext<QueryContextValue>({ queryData: null, isLoading: false, error: null });

export const QueryProvider: React.FC<{ children?: React.ReactNode }> = ({ children }) => {
  const [queryData] = useState<any | null>(null);
  const [isLoading] = useState<boolean>(false);
  const [error] = useState<any | null>(null);

  return (
    <QueryClientProvider client={queryClient}>
      <QueryContext.Provider value={{ queryData, isLoading, error }}>
        {children}
      </QueryContext.Provider>
    </QueryClientProvider>
  );
};

export const useQueryContext = () => useContext(QueryContext);

export default QueryProvider;