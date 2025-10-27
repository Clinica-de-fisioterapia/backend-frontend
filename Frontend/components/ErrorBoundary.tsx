import React from 'react';
import { logError } from '../services/errorLogger';

interface ErrorBoundaryState {
  hasError: boolean;
}

interface ErrorBoundaryProps {
  children: React.ReactNode;
}

class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    // envia log para o serviço de logs e mantém o console
    logError(error, errorInfo).catch(() => {});
    console.error("Erro capturado:", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return <h1>Algo deu errado. Tente novamente mais tarde.</h1>;
    }

    return this.props.children; 
  }
}

export default ErrorBoundary;