import type { ChrysalisState } from '../types';

export interface ChrysalisTransformTarget {
  waveSpeed: number;           // Velocity of cellular breathing & honey energy waves
  glowIntensity: number;       // Base line luminescence multiplier
  particleSpeed: number;       // Firefly drift velocity multiplier
  parallaxSensitivity: number; // Camera tilt & parallax response to mouse
  accentColor: number;         // Primary amber luminescence (e.g. 0xF59E0B)
  secondaryColor: number;      // Warm spark highlight (e.g. 0xFDE68A)
}

export interface ChrysalisStateConfig {
  state: ChrysalisState;
  target: ChrysalisTransformTarget;
  transitionSpeed: number;
}
