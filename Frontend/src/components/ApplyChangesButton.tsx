import React from "react";

export default function ApplyChangesButton() {
  const onClick = () => {
    alert("Fase 1 aplicada localmente. Reinicie o dev server se necessário.");
  };
  return <button onClick={onClick} style={{ padding: "8px 12px", background: "var(--accent)", color: "#04211a", border: "none", borderRadius: 8 }}>Aceitar Fase 1</button>;
}