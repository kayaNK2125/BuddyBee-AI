import type { ChrysalisState } from '../types';
import type { ChrysalisTransformTarget } from './types';

const STATE_CONFIGS: Record<ChrysalisState, ChrysalisTransformTarget> = {
  idle: {
    waveSpeed: 0.75,           // Tranquil, meditative 8s breathing wave
    glowIntensity: 1.0,        // Soft, ambient baseline luminescence
    particleSpeed: 0.75,       // Slow drifting firefly embers
    parallaxSensitivity: 1.0,  // Smooth depth parallax
    accentColor: 0xF59E0B,     // Classic Solar Amber
    secondaryColor: 0xFDE68A,
  },
  interacting: {
    waveSpeed: 1.15,
    glowIntensity: 1.15,
    particleSpeed: 1.0,
    parallaxSensitivity: 1.2,
    accentColor: 0xF59E0B,
    secondaryColor: 0xFDE68A,
  },
  typing: {
    waveSpeed: 1.50,           // Kinetic ripple across honeycomb cells
    glowIntensity: 1.25,       // Cells awaken with energetic pulse
    particleSpeed: 1.25,
    parallaxSensitivity: 1.1,
    accentColor: 0xFBBF24,
    secondaryColor: 0xFDE68A,
  },
  sending: {
    waveSpeed: 2.20,           // Harmonic energy sweep through the lattice
    glowIntensity: 1.50,       // Radiant pulse across the network
    particleSpeed: 1.60,
    parallaxSensitivity: 0.8,
    accentColor: 0xFDE68A,
    secondaryColor: 0xF59E0B,
  },
  thinking: {
    waveSpeed: 1.80,           // Coordinated rhythmic pulse through the hive mind
    glowIntensity: 1.40,       // Warm golden illumination
    particleSpeed: 1.35,
    parallaxSensitivity: 1.0,
    accentColor: 0xFBBF24,
    secondaryColor: 0xFDE68A,
  },
  responding: {
    waveSpeed: 1.10,           // Steady golden honey flow
    glowIntensity: 1.20,       // Gentle continuous luminescence
    particleSpeed: 1.0,
    parallaxSensitivity: 1.0,
    accentColor: 0xF59E0B,
    secondaryColor: 0xFDE68A,
  },
  error: {
    waveSpeed: 0.45,           // Subdued, dim smoldering pulse
    glowIntensity: 0.75,       // Lower visibility
    particleSpeed: 0.40,
    parallaxSensitivity: 0.5,
    accentColor: 0xDC2626,     // Alert amber-crimson
    secondaryColor: 0x991B1B,
  },
};

export class ChrysalisStateMachine {
  public current: ChrysalisTransformTarget;
  public target: ChrysalisTransformTarget;
  public currentState: ChrysalisState = 'idle';

  constructor() {
    this.current = { ...STATE_CONFIGS.idle };
    this.target = { ...STATE_CONFIGS.idle };
  }

  public transitionTo(state: ChrysalisState): void {
    if (this.currentState === state) return;
    this.currentState = state;
    this.target = { ...STATE_CONFIGS[state] };
  }

  public update(delta: number): void {
    const lerpRate = 3.2 * delta;

    this.current.waveSpeed += (this.target.waveSpeed - this.current.waveSpeed) * Math.min(lerpRate, 1);
    this.current.glowIntensity += (this.target.glowIntensity - this.current.glowIntensity) * Math.min(lerpRate, 1);
    this.current.particleSpeed += (this.target.particleSpeed - this.current.particleSpeed) * Math.min(lerpRate, 1);
    this.current.parallaxSensitivity += (this.target.parallaxSensitivity - this.current.parallaxSensitivity) * Math.min(lerpRate, 1);
  }
}
