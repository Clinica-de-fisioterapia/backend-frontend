export async function logError(error: Error, info?: React.ErrorInfo) {
  const payload = {
    message: error?.message || String(error),
    stack: (error as any)?.stack || null,
    componentStack: info?.componentStack || null,
    time: new Date().toISOString(),
  };

  // Log local (sempre)
  console.error("ErrorBoundary log:", payload);

  // Tenta enviar para backend (rota exemplo /api/logs). Ignora falhas.
  try {
    await fetch("/api/logs", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
  } catch (e) {
    console.error("Falha ao enviar log para /api/logs:", e);
  }
}