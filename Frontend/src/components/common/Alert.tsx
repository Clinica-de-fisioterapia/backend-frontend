import React from 'react';
import { AlertCircle, CheckCircle, XCircle, Info, X } from 'lucide-react';

type AlertType = 'success' | 'error' | 'warning' | 'info';

interface AlertProps {
  type: AlertType;
  message: string;
  onClose?: () => void;
  style?: React.CSSProperties;
}

const iconMap: Record<AlertType, React.ReactNode> = {
  success: <CheckCircle size={20} />,
  error: <XCircle size={20} />,
  warning: <AlertCircle size={20} />,
  info: <Info size={20} />,
};

const colorMap: Record<AlertType, { background: string; text: string; border: string }> = {
  success: {
    background: '#ecfdf5',
    text: '#047857',
    border: '#a7f3d0'
  },
  error: {
    background: '#fef2f2',
    text: '#b91c1c',
    border: '#fca5a5'
  },
  warning: {
    background: '#fffbeb',
    text: '#b45309',
    border: '#fde68a'
  },
  info: {
    background: '#eff6ff',
    text: '#1d4ed8',
    border: '#bfdbfe'
  },
};

export const Alert: React.FC<AlertProps> = ({ type, message, onClose, style }) => {
  const { background, text, border } = colorMap[type];

  return (
    <div 
      style={{
        background,
        color: text,
        border: `1px solid ${border}`,
        padding: '12px 16px',
        borderRadius: '8px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '12px',
        fontSize: '14px',
        fontWeight: '500',
        animation: 'fadeIn 0.3s ease-out',
        ...style
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
        {iconMap[type]}
        <span>{message}</span>
      </div>
      {onClose && (
        <button
          onClick={onClose}
          style={{
            color: text,
            opacity: 0.7,
            transition: 'opacity 0.2s',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.opacity = '1';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.opacity = '0.7';
          }}
        >
          <X size={18} />
        </button>
      )}
    </div>
  );
};