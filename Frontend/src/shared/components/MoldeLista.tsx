import React from "react";

export default function MoldeLista({ title, children }: { title?: string; children: React.ReactNode }) {
  return (
    <div className="molde-list card">
      {title && <div className="molde-header"><h3>{title}</h3></div>}
      <div>{children}</div>
    </div>
  );
}