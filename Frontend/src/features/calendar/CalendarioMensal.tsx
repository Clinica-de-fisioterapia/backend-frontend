import React from "react";

interface Props {
  horizonDays?: number; // horizonte em dias (30/60/90)
  onDayClick?: (date: Date) => void;
  selectedDate?: Date;
}

export default function CalendarioMensal({ horizonDays = 30, onDayClick, selectedDate }: Props) {
  const monthsCount = Math.min(Math.ceil(horizonDays / 30), 12);
  const today = new Date();

  const months = Array.from({ length: monthsCount }).map((_, i) => new Date(today.getFullYear(), today.getMonth() + i, 1));

  return (
    <div style={{ display: "grid", gap: 12 }}>
      {months.map((m) => (
        <div key={m.toISOString()} className="calendar-month">
          <h4>{m.toLocaleString(undefined, { month: "long", year: "numeric" })}</h4>
          <div className="calendar-grid">
            {Array.from({ length: 42 }).map((_, idx) => {
              const firstOfMonth = new Date(m.getFullYear(), m.getMonth(), 1);
              const day = new Date(firstOfMonth);
              day.setDate(firstOfMonth.getDate() + (idx - firstOfMonth.getDay()));
              const delta = Math.floor((+day - +today) / (1000 * 60 * 60 * 24));
              const inHorizon = delta >= 0 && delta < horizonDays;
              const isSelected = selectedDate && day.toDateString() === selectedDate.toDateString();
              return (
                <button
                  key={idx}
                  disabled={!inHorizon}
                  onClick={() => inHorizon && onDayClick?.(day)}
                  className={`calendar-day ${!inHorizon ? "disabled" : ""} ${isSelected ? "selected" : ""}`}
                >
                  {day.getDate()}
                </button>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}