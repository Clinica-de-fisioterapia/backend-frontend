import React, { createContext, useContext, ReactNode } from "react";

interface AuthContextType {
  // define your auth context shape here
  user: any;
  login: (userData: any) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  // implement your auth logic here
  const login = (userData: any) => {};
  const logout = () => {};

  const value = { user: null, login, logout };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};