import * as THREE from 'three';

interface FireflyData {
  baseX: number;
  baseY: number;
  baseZ: number;
  freqX: number;
  freqY: number;
  freqZ: number;
  ampX: number;
  ampY: number;
  ampZ: number;
  phase: number;
  blinkFreq: number;
  baseSize: number;
}

export class ParticleField {
  public points: THREE.Points;
  private geometry: THREE.BufferGeometry;
  private material: THREE.PointsMaterial;
  private texture: THREE.CanvasTexture | null = null;
  private fireflies: FireflyData[] = [];
  private positions: Float32Array;
  private colors: Float32Array;

  constructor(count = 85) {
    this.geometry = new THREE.BufferGeometry();
    this.positions = new Float32Array(count * 3);
    this.colors = new Float32Array(count * 3);

    for (let i = 0; i < count; i++) {
      // Wide distribution across the viewport volume
      // Slightly denser in the right quadrant (negative space)
      const isRightBiased = Math.random() > 0.35;
      const x = isRightBiased 
        ? (Math.random() * 6.5 - 0.5) 
        : (Math.random() * 8.0 - 7.0);
      const y = (Math.random() * 8.5 - 4.25);
      const z = (Math.random() * 5.0 - 2.5);

      this.fireflies.push({
        baseX: x,
        baseY: y,
        baseZ: z,
        freqX: 0.30 + Math.random() * 0.40,
        freqY: 0.35 + Math.random() * 0.45,
        freqZ: 0.25 + Math.random() * 0.35,
        ampX: 0.40 + Math.random() * 0.50,
        ampY: 0.35 + Math.random() * 0.45,
        ampZ: 0.30 + Math.random() * 0.40,
        phase: Math.random() * Math.PI * 2,
        blinkFreq: 0.7 + Math.random() * 1.5,
        baseSize: 0.06 + Math.random() * 0.04,
      });

      this.positions[i * 3] = x;
      this.positions[i * 3 + 1] = y;
      this.positions[i * 3 + 2] = z;

      this.colors[i * 3] = 0.96;
      this.colors[i * 3 + 1] = 0.62;
      this.colors[i * 3 + 2] = 0.04;
    }

    this.geometry.setAttribute('position', new THREE.BufferAttribute(this.positions, 3));
    this.geometry.setAttribute('color', new THREE.BufferAttribute(this.colors, 3));

    // Create a soft glowing circular firefly sprite texture
    this.texture = this.createFireflyTexture();

    this.material = new THREE.PointsMaterial({
      size: 0.16,
      map: this.texture,
      vertexColors: true,
      transparent: true,
      opacity: 0.85,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
    });

    this.points = new THREE.Points(this.geometry, this.material);
  }

  private createFireflyTexture(): THREE.CanvasTexture {
    const canvas = document.createElement('canvas');
    canvas.width = 64;
    canvas.height = 64;
    const ctx = canvas.getContext('2d')!;

    const grad = ctx.createRadialGradient(32, 32, 0, 32, 32, 32);
    grad.addColorStop(0, 'rgba(255, 255, 255, 1.0)');
    grad.addColorStop(0.2, 'rgba(253, 230, 138, 0.9)');
    grad.addColorStop(0.5, 'rgba(245, 158, 11, 0.4)');
    grad.addColorStop(0.8, 'rgba(217, 119, 6, 0.15)');
    grad.addColorStop(1.0, 'rgba(217, 119, 6, 0)');

    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, 64, 64);

    const tex = new THREE.CanvasTexture(canvas);
    tex.needsUpdate = true;
    return tex;
  }

  public update(time: number, speedMultiplier = 1.0, colorHex = 0xF59E0B): void {
    const posAttr = this.geometry.attributes.position as THREE.BufferAttribute;
    const colAttr = this.geometry.attributes.color as THREE.BufferAttribute;
    const pos = posAttr.array as Float32Array;
    const col = colAttr.array as Float32Array;
    const count = this.fireflies.length;

    const rBase = ((colorHex >> 16) & 255) / 255;
    const gBase = ((colorHex >> 8) & 255) / 255;
    const bBase = (colorHex & 255) / 255;

    for (let i = 0; i < count; i++) {
      const f = this.fireflies[i];
      const idx = i * 3;

      const t = time * speedMultiplier;
      pos[idx] = f.baseX + Math.sin(t * f.freqX + f.phase) * f.ampX;
      pos[idx + 1] = f.baseY + Math.cos(t * f.freqY + f.phase * 1.3) * f.ampY;
      pos[idx + 2] = f.baseZ + Math.sin(t * f.freqZ + f.phase * 0.7) * f.ampZ;

      // Soft natural blinking
      const pulse = Math.pow(Math.sin(time * f.blinkFreq + f.phase), 4);
      const intensity = 0.20 + 0.80 * pulse;

      col[idx] = THREE.MathUtils.lerp(rBase * 0.65, 0.99, pulse * 0.65) * intensity;
      col[idx + 1] = THREE.MathUtils.lerp(gBase * 0.65, 0.92, pulse * 0.65) * intensity;
      col[idx + 2] = THREE.MathUtils.lerp(bBase * 0.65, 0.60, pulse * 0.65) * intensity;
    }

    posAttr.needsUpdate = true;
    colAttr.needsUpdate = true;
  }

  public dispose(): void {
    this.geometry.dispose();
    this.material.dispose();
    if (this.texture) {
      this.texture.dispose();
    }
  }
}
