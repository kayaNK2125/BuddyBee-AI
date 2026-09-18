import * as THREE from 'three';

export class ChrysalisLighting {
  public group: THREE.Group;
  public ambientLight: THREE.AmbientLight;
  public hiveGlow: THREE.PointLight;

  constructor() {
    this.group = new THREE.Group();

    // 1. Dark obsidian ambient base
    this.ambientLight = new THREE.AmbientLight(0x0A0C10, 0.6);
    this.group.add(this.ambientLight);

    // 2. Warm solar amber atmospheric emitter in the right quadrant
    this.hiveGlow = new THREE.PointLight(0xF59E0B, 1.2, 18, 1.4);
    this.hiveGlow.position.set(3.5, 0.5, 2.0);
    this.group.add(this.hiveGlow);
  }

  public setIntensity(intensity: number): void {
    this.hiveGlow.intensity = 1.0 + (intensity * 0.3);
  }

  public setColor(colorHex: number): void {
    this.hiveGlow.color.setHex(colorHex);
  }

  public dispose(): void {
    this.hiveGlow.dispose();
    this.ambientLight.dispose();
  }
}
