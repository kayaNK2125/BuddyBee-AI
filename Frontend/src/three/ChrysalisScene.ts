import * as THREE from 'three';
import { ChrysalisMesh } from './ChrysalisMesh';
import { ChrysalisLighting } from './Lighting';
import { ParticleField } from './ParticleField';
import { ChrysalisStateMachine } from './StateMachine';
import type { ChrysalisState } from '../types';

export class ChrysalisScene {
  private canvas: HTMLCanvasElement;
  private renderer: THREE.WebGLRenderer | null = null;
  private scene: THREE.Scene;
  private camera: THREE.PerspectiveCamera;
  private clock: THREE.Clock;

  public mesh: ChrysalisMesh;
  public lighting: ChrysalisLighting;
  public particles: ParticleField;
  public stateMachine: ChrysalisStateMachine;

  private animationFrameId: number | null = null;
  private isVisible = true;
  private isReducedMotion = false;
  private lastInteractionTime = Date.now();
  private pointer = { x: 0, y: 0, targetX: 0, targetY: 0 };
  private boundResize: () => void;
  private boundVisibilityChange: () => void;
  private boundPointerMove: (e: MouseEvent) => void;

  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.clock = new THREE.Clock();
    this.scene = new THREE.Scene();

    if (typeof window !== 'undefined') {
      this.isReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    // Wide perspective camera for full-screen atmospheric honeycomb canvas
    const width = canvas.clientWidth || window.innerWidth || 1440;
    const height = canvas.clientHeight || window.innerHeight || 900;
    const aspect = width / (height || 1);
    this.camera = new THREE.PerspectiveCamera(45, aspect, 0.1, 50);
    this.camera.position.set(0, 0, 6.0);

    try {
      this.renderer = new THREE.WebGLRenderer({
        canvas,
        alpha: true,
        antialias: true,
        powerPreference: 'high-performance',
      });
      this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));
      this.renderer.setSize(width, height, false);
    } catch (e) {
      console.warn('WebGL initialization failed, falling back to non-3D mode.', e);
      this.renderer = null;
    }

    this.lighting = new ChrysalisLighting();
    this.scene.add(this.lighting.group);

    // Procedural Honeycomb Lattice
    this.mesh = new ChrysalisMesh();
    this.scene.add(this.mesh.group);

    // Firefly Swarm (95 bioluminescent embers)
    this.particles = new ParticleField(95);
    this.scene.add(this.particles.points);

    this.stateMachine = new ChrysalisStateMachine();

    this.boundResize = this.onResize.bind(this);
    this.boundVisibilityChange = this.onVisibilityChange.bind(this);
    this.boundPointerMove = this.onPointerMove.bind(this);

    window.addEventListener('resize', this.boundResize);
    document.addEventListener('visibilitychange', this.boundVisibilityChange);
    window.addEventListener('mousemove', this.boundPointerMove, { passive: true });

    this.startLoop();
  }

  public isAvailable(): boolean {
    return this.renderer !== null;
  }

  public setState(state: ChrysalisState): void {
    this.stateMachine.transitionTo(state);
    this.lastInteractionTime = Date.now();
  }

  private onPointerMove(e: MouseEvent): void {
    this.lastInteractionTime = Date.now();
    // Normalize -1 to +1
    this.pointer.targetX = (e.clientX / window.innerWidth) * 2 - 1;
    this.pointer.targetY = -(e.clientY / window.innerHeight) * 2 + 1;
  }

  private onVisibilityChange(): void {
    this.isVisible = !document.hidden;
    if (this.isVisible) {
      this.clock.start();
      this.lastInteractionTime = Date.now();
    }
  }

  public onResize(): void {
    if (!this.canvas || !this.renderer) return;
    const width = this.canvas.clientWidth || window.innerWidth;
    const height = this.canvas.clientHeight || window.innerHeight;

    if (width === 0 || height === 0) return;

    this.camera.aspect = width / height;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(width, height, false);
  }

  private startLoop(): void {
    const animate = () => {
      this.animationFrameId = requestAnimationFrame(animate);

      if (!this.isVisible || !this.renderer) return;

      const delta = Math.min(this.clock.getDelta(), 0.1);
      const elapsedTime = this.clock.getElapsedTime();

      // Smooth pointer tracking for multi-plane camera parallax
      this.pointer.x += (this.pointer.targetX - this.pointer.x) * (2.8 * delta);
      this.pointer.y += (this.pointer.targetY - this.pointer.y) * (2.8 * delta);

      this.stateMachine.update(delta);
      const current = this.stateMachine.current;

      // Idle throttling after 15s of inactivity
      const isIdle = Date.now() - this.lastInteractionTime > 15000;
      if (isIdle && Math.floor(elapsedTime * 60) % 3 !== 0) {
        return;
      }

      if (this.isReducedMotion) {
        this.mesh.updateLattice(delta, elapsedTime, 0.2, current.glowIntensity);
        this.particles.update(elapsedTime, 0.2, this.stateMachine.target.accentColor);
        this.renderer.render(this.scene, this.camera);
        return;
      }

      // Multi-plane parallax camera tilt
      const parallax = current.parallaxSensitivity;
      const targetCamX = this.pointer.x * 0.35 * parallax;
      const targetCamY = this.pointer.y * 0.25 * parallax;

      this.camera.position.x += (targetCamX - this.camera.position.x) * (2.5 * delta);
      this.camera.position.y += (targetCamY - this.camera.position.y) * (2.5 * delta);
      this.camera.lookAt(0, 0, 0);

      // Cellular breathing & wave propagation through the honeycomb lattice
      this.mesh.updateLattice(delta, elapsedTime, current.waveSpeed, current.glowIntensity);
      this.mesh.setAccentColor(this.stateMachine.target.accentColor);

      // Firefly ember swarm drift & pulsation
      this.particles.update(elapsedTime, current.particleSpeed, this.stateMachine.target.accentColor);

      // Atmospheric lighting updates
      this.lighting.setIntensity(current.glowIntensity);
      this.lighting.setColor(this.stateMachine.target.accentColor);

      this.renderer.render(this.scene, this.camera);
    };

    animate();
  }

  public dispose(): void {
    if (this.animationFrameId !== null) {
      cancelAnimationFrame(this.animationFrameId);
    }

    window.removeEventListener('resize', this.boundResize);
    document.removeEventListener('visibilitychange', this.boundVisibilityChange);
    window.removeEventListener('mousemove', this.boundPointerMove);

    this.mesh.dispose();
    this.lighting.dispose();
    this.particles.dispose();

    if (this.renderer) {
      this.renderer.dispose();
    }
  }
}
