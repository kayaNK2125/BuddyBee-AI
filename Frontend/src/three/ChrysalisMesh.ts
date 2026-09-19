import * as THREE from 'three';

interface HexCell {
  mesh: THREE.LineLoop;
  material: THREE.LineBasicMaterial;
  baseX: number;
  baseY: number;
  baseZ: number;
  baseOpacity: number;
  phase: number;
  layer: number;
}

export class ChrysalisMesh {
  public group: THREE.Group;
  private cells: HexCell[] = [];
  private unitGeometries: THREE.BufferGeometry[] = [];

  constructor() {
    this.group = new THREE.Group();
    this.buildHoneycombLattice();
  }

  private buildHoneycombLattice(): void {
    const createHexGeo = (radius: number): THREE.BufferGeometry => {
      const pts: THREE.Vector3[] = [];
      for (let i = 0; i < 6; i++) {
        const angle = (Math.PI / 6) + (i * Math.PI / 3);
        pts.push(new THREE.Vector3(radius * Math.cos(angle), radius * Math.sin(angle), 0));
      }
      const geo = new THREE.BufferGeometry().setFromPoints(pts);
      this.unitGeometries.push(geo);
      return geo;
    };

    // Layer 1: Distant Background Hive (Deep amber, smaller scale)
    // 15-20% fewer cells near edge; subtle depth
    this.generateLayer({
      radius: 0.42,
      depthZ: -2.2,
      xRange: [-6.5, 6.5],
      yRange: [-4.0, 4.0],
      baseOpacityMax: 0.11,
      baseOpacityMin: 0.015,
      layerIndex: 0,
      geo: createHexGeo(0.42),
      baseSkipProbability: 0.46,
    });

    // Layer 2: Midground Main Honeycomb (Atmospheric right side, faint bottom-left echo)
    this.generateLayer({
      radius: 0.65,
      depthZ: 0.0,
      xRange: [-6.0, 6.5],
      yRange: [-3.8, 3.8],
      baseOpacityMax: 0.27,
      baseOpacityMin: 0.015,
      layerIndex: 1,
      geo: createHexGeo(0.65),
      baseSkipProbability: 0.42,
    });

    // Layer 3: Foreground Accent Cells (Floating large hexagonal halos, sparse)
    this.generateLayer({
      radius: 0.90,
      depthZ: 1.6,
      xRange: [1.2, 5.8],
      yRange: [-2.8, 2.8],
      baseOpacityMax: 0.18,
      baseOpacityMin: 0.04,
      layerIndex: 2,
      geo: createHexGeo(0.90),
      baseSkipProbability: 0.68,
    });
  }

  private generateLayer(config: {
    radius: number;
    depthZ: number;
    xRange: [number, number];
    yRange: [number, number];
    baseOpacityMax: number;
    baseOpacityMin: number;
    layerIndex: number;
    geo: THREE.BufferGeometry;
    baseSkipProbability: number;
  }): void {
    const { radius, depthZ, xRange, yRange, baseOpacityMax, baseOpacityMin, layerIndex, geo, baseSkipProbability } = config;

    const dx = Math.sqrt(3) * radius;
    const dy = 1.5 * radius;

    const colStart = Math.floor(xRange[0] / dx);
    const colEnd = Math.ceil(xRange[1] / dx);
    const rowStart = Math.floor(yRange[0] / dy);
    const rowEnd = Math.ceil(yRange[1] / dy);

    for (let r = rowStart; r <= rowEnd; r++) {
      const y = r * dy;
      const xOffset = (Math.abs(r) % 2 === 1) ? dx * 0.5 : 0;

      for (let c = colStart; c <= colEnd; c++) {
        const x = c * dx + xOffset;

        if (x < xRange[0] || x > xRange[1] || y < yRange[0] || y > yRange[1]) continue;

        // Bottom-left subtle presence zone
        const isBottomLeft = (x >= -5.8 && x <= -1.8 && y >= -3.8 && y <= -0.8);

        // Right side: reduce density near the edge by 15-20% so it stays atmospheric rather than grid-like
        let skipChance = baseSkipProbability;
        if (x > 2.8) {
          skipChance += 0.18;
        }

        if (isBottomLeft) {
          // Slightly higher preservation for bottom-left echo cells
          skipChance = 0.40;
        }

        if (Math.random() < Math.min(skipChance, 0.88)) continue;

        let baseOpacity = 0;

        if (isBottomLeft) {
          // Whisper-thin lines in the bottom-left area (0.04 - 0.075)
          const blWeight = Math.max(0, 1 - Math.hypot((x + 3.8) / 2.5, (y + 2.3) / 1.8));
          baseOpacity = 0.038 + (blWeight * 0.035);
        } else {
          // Completely clear reading column (-1.8 <= x < 0.6)
          if (x < 0.6) continue;

          // Faster, softer fade-out gradient toward the right
          const normX = Math.min(Math.max((x - 0.6) / 5.2, 0), 1);
          const horizontalWeight = Math.pow(normX, 1.7);
          baseOpacity = THREE.MathUtils.lerp(baseOpacityMin, baseOpacityMax, horizontalWeight);
        }

        if (baseOpacity < 0.02) continue;

        const material = new THREE.LineBasicMaterial({
          color: 0xF59E0B,
          transparent: true,
          opacity: baseOpacity,
          blending: THREE.AdditiveBlending,
          depthWrite: false,
        });

        const lineLoop = new THREE.LineLoop(geo, material);
        lineLoop.position.set(x, y, depthZ);

        this.cells.push({
          mesh: lineLoop,
          material,
          baseX: x,
          baseY: y,
          baseZ: depthZ,
          baseOpacity,
          phase: Math.random() * Math.PI * 2,
          layer: layerIndex,
        });

        this.group.add(lineLoop);
      }
    }
  }

  public updateLattice(_delta: number, time: number, waveSpeed: number, glowIntensity: number): void {
    const count = this.cells.length;

    for (let i = 0; i < count; i++) {
      const cell = this.cells[i];

      const wavePhase = time * waveSpeed + (cell.baseX * 0.40) - (cell.baseY * 0.30) + cell.phase * 0.2;
      const wave = Math.sin(wavePhase);

      const scale = 1.0 + (wave * 0.016);
      cell.mesh.scale.set(scale, scale, 1);

      const waveMultiplier = 0.88 + (0.22 * wave);
      const targetOpacity = cell.baseOpacity * glowIntensity * waveMultiplier;
      cell.material.opacity = Math.max(0.01, Math.min(0.88, targetOpacity));
    }
  }

  public setAccentColor(colorHex: number): void {
    const count = this.cells.length;
    for (let i = 0; i < count; i++) {
      this.cells[i].material.color.setHex(colorHex);
    }
  }

  public dispose(): void {
    for (const cell of this.cells) {
      cell.material.dispose();
    }
    for (const geo of this.unitGeometries) {
      geo.dispose();
    }
    this.cells = [];
    this.unitGeometries = [];
  }
}
