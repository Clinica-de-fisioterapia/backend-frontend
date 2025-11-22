import React from 'react';

export const Logo: React.FC<{ size?: 'small' | 'medium' | 'large' }> = ({ 
  size = 'medium' 
}) => {
  const sizeMap = {
    small: { container: 32, fontSize: 16, gap: 8 },
    medium: { container: 40, fontSize: 18, gap: 12 },
    large: { container: 50, fontSize: 24, gap: 16 }
  };
  const { container, fontSize, gap } = sizeMap[size];

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      gap: `${gap}px`,
      fontSize: `${fontSize}px`,
      fontWeight: 'bold',
      color: '#2563eb'
    }}>
      <div style={{
        width: `${container}px`,
        height: `${container}px`,
        background: 'linear-gradient(135deg, #2563eb, #1e40af)',
    
        borderRadius: '50%',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        color: 'white',
        fontSize: `${fontSize * 0.8}px`,
        fontWeight: 'bold'
      }}>
        ⏱️
      </div>
      <span>ChronoSys</span>
    </div>
  );
};