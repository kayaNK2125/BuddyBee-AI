import type { ChrysalisState } from '../types';
import type { ChrysalisTransformTarget } from './types';

const STATE_CONFIGS: Record<ChrysalisState, ChrysalisTransformTarget> = {
  idle: {
    waveSpeed: 0.75,           // Tranquil, meditative 8s breathing wave
    glowIntensity: 1.0,        // Soft ambient baseline luminescence
    particleSpeed: 0.70,       // Calm, ambient drifting
    parallaxSensitivity: 1.0,  // Smooth depth parallax
    accentColor: 0xF59E0B,     // Classic Solar Amber
    secondaryColor: 0xFDE68A,
  },
  interacting: {
    waveSpeed: 0.95,
    glowIntensity: 1.10,
    particleSpeed: 0.75,       // Calm transition
    parallaxSensitivity: 1.1,
    accentColor: 0xF59E0B,
    secondaryColor: 0xFDE68A,
  },
  typing: {
    waveSpeed: 1.15,           // Gentle kinetic ripple
    glowIntensity: 1.18,       // Subtle awakening
    particleSpeed: 0.78,       // Steady, calm drift (no speed surge)
    parallaxSensitivity: 1.05,
    accentColor: 0xFBBF24,
    secondaryColor: 0xFDE68A,
  },
  sending: {
    waveSpeed: 1.40,           // Measured inward pulse
    glowIntensity: 1.30,
    particleSpeed: 0.85,       // Slight focus without sudden rush
    parallaxSensitivity: 0.85,
    accentColor: 0xFDE68A,
    secondaryColor: 0xF59E0B,
  },
  thinking: {
    waveSpeed: 1.30,           // Calm rhythmic pulse
    glowIntensity: 1.25,
    particleSpeed: 0.80,       // Calm drift during synthesis
    parallaxSensitivity: 0.95,
    accentColor: 0xFBBF24,
    secondaryColor: 0xFDE68A,
  },
  responding: {
    waveSpeed: 0.95,           // Steady golden honey flow
    glowIntensity: 1.15,
    particleSpeed: 0.75,       // Return smoothly to baseline
    parallaxSensitivity: 1.0,
    accentColor: 0xF59E0B,
    secondaryColor: 0xFDE68A,
  },
  error: {
    waveSpeed: 0.45,           // Subdued smoldering pulse
    glowIntensity: 0.75,
    particleSpeed: 0.40,       // Slow subdued embers
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
    const lerpRate = 3.0 * delta;

    this.current.waveSpeed += (this.target.waveSpeed - this.current.waveSpeed) * Math.min(lerpRate, 1);
    this.current.glowIntensity += (this.target.glowIntensity - this.current.glowIntensity) * Math.min(lerpRate, 1);
    this.current.particleSpeed += (this.target.particleSpeed - this.current.particleSpeed) * Math.min(lerpRate, 1);
    this.current.parallaxSensitivity += (this.target.parallaxSensitivity - this.current.parallaxSensitivity) * Math.min(lerpRate, 1);
  }
}
