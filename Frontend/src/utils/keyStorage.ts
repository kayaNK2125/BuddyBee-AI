// Session-scoped client storage for user API provider keys.
// Keys are held in sessionStorage only (cleared upon tab/browser close)
// to prevent sensitive personal credentials from persisting on shared devices.

export type ProviderMode = 'managed' | 'custom';
export type ProviderId = 'gemini' | 'openai';

const SESSION_KEYS = {
  mode: 'buddybee_provider_mode',
  gemini: 'buddybee_key_gemini',
  openai: 'buddybee_key_openai',
} as const;

export function getProviderMode(): ProviderMode {
  if (typeof window === 'undefined') return 'managed';
  try {
    const mode = sessionStorage.getItem(SESSION_KEYS.mode);
    return mode === 'custom' ? 'custom' : 'managed';
  } catch {
    return 'managed';
  }
}

export function setProviderMode(mode: ProviderMode): void {
  if (typeof window === 'undefined') return;
  try {
    sessionStorage.setItem(SESSION_KEYS.mode, mode);
    window.dispatchEvent(new CustomEvent('buddybee_provider_mode_changed', { detail: mode }));
  } catch {
    // Ignore storage errors
  }
}

export function getProviderKey(provider: ProviderId): string | null {
  if (typeof window === 'undefined') return null;
  try {
    const val = sessionStorage.getItem(SESSION_KEYS[provider]);
    return val && val.trim().length > 0 ? val.trim() : null;
  } catch {
    return null;
  }
}

export function saveProviderKey(provider: ProviderId, key: string): void {
  if (typeof window === 'undefined') return;
  try {
    const trimmed = key.trim();
    if (trimmed) {
      sessionStorage.setItem(SESSION_KEYS[provider], trimmed);
    } else {
      sessionStorage.removeItem(SESSION_KEYS[provider]);
    }
  } catch {
    // Ignore storage errors
  }
}

export function removeProviderKey(provider: ProviderId): void {
  if (typeof window === 'undefined') return;
  try {
    sessionStorage.removeItem(SESSION_KEYS[provider]);
  } catch {
    // Ignore storage errors
  }
}

export function hasProviderKey(provider: ProviderId): boolean {
  return getProviderKey(provider) !== null;
}

export function getMaskedKeyHint(provider: ProviderId): string | null {
  const key = getProviderKey(provider);
  if (!key) return null;
  const bullet = '\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022';
  if (key.length <= 8) return bullet;
  const suffix = key.slice(-4);
  return `${bullet}${suffix}`;
}

export function getActiveUserKeys(): { gemini?: string; openai?: string } {
  const mode = getProviderMode();
  if (mode !== 'custom') return {};

  const keys: { gemini?: string; openai?: string } = {};
  const gemini = getProviderKey('gemini');
  if (gemini) keys.gemini = gemini;

  const openai = getProviderKey('openai');
  if (openai) keys.openai = openai;

  return keys;
}
