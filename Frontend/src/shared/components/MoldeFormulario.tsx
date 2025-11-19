import React from "react";

export default function MoldeFormulario({ title, children, onSubmit }: { title?: string; children: React.ReactNode; onSubmit?: (e: React.FormEvent) => void }) {
  return (
    <form onSubmit={onSubmit} className="molde-form">
      {title && <div className="molde-header"><h3>{title}</h3></div>}
      <div style={{ display: "grid", gap: 8 }}>{children}</div>
    </form>
  );
}