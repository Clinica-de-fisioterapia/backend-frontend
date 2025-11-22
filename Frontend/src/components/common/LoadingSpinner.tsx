import React from 'react';

interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  fullScreen?: boolean;
}

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'medium',
  fullScreen = false
}) => {
  const sizeMap = {
    small: 24,
    medium: 40,
    large: 64
  };
  const containerStyle: React.CSSProperties = fullScreen
    ? {
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        background: 'rgba(255, 255, 255, 0.9)',
      
        zIndex: 9999
      }
    : {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '20px'
      };
  
  return (
    <div style={containerStyle}>
      <div 
        style={{
          width: `${sizeMap[size]}px`,
          height: `${sizeMap[size]}px`,
          border: '3px solid #e5e7eb',
          borderTop: '3px solid #2563eb',
          borderRadius: '50%',
          animation: 'spin 1s linear infinite'
        }}
      >
        {/* O keyframes 'spin' é definido em src/app.css, mas incluído aqui para robustez. */}
        <style>{`
          @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
          }
        `}</style>
      </div>
    </div>
  );
};