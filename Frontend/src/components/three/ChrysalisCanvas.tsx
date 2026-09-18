import React, { useEffect, useRef, useState } from 'react';
import { ChrysalisScene } from '../../three/ChrysalisScene';
import type { ChrysalisState } from '../../types';
import './ChrysalisCanvas.css';

interface ChrysalisCanvasProps {
  state: ChrysalisState;
  className?: string;
}

export const ChrysalisCanvas: React.FC<ChrysalisCanvasProps> = ({ state, className = '' }) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const sceneRef = useRef<ChrysalisScene | null>(null);
  const [hasWebGL, setHasWebGL] = useState(true);

  useEffect(() => {
    if (!canvasRef.current) return;

    const scene = new ChrysalisScene(canvasRef.current);
    sceneRef.current = scene;

    if (!scene.isAvailable()) {
      setHasWebGL(false);
    }

    return () => {
      scene.dispose();
      sceneRef.current = null;
    };
  }, []);

  useEffect(() => {
    if (sceneRef.current) {
      sceneRef.current.setState(state);
    }
  }, [state]);

  return (
    <div className={`chrysalis-wrapper ${className}`} aria-hidden="true">
      {hasWebGL ? (
        <canvas ref={canvasRef} className="chrysalis-canvas" />
      ) : (
        /* Graceful WebGL Fallback: Ambient CSS Solar Beacon */
        <div className={`chrysalis-fallback state-${state}`} />
      )}
    </div>
  );
};
