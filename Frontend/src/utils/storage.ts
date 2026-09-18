const STORAGE_KEYS = {
  conversationId: 'buddybee_conversation_id',
  userId: 'buddybee_user_id',
} as const;

export function getOrCreateId(key: keyof typeof STORAGE_KEYS, prefix: string): string {
  if (typeof window === 'undefined') return `${prefix}_${Date.now()}`;
  try {
    const existing = localStorage.getItem(STORAGE_KEYS[key]);
    if (existing) return existing;
    const newId = `${prefix}_${crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2, 11)}`;
    localStorage.setItem(STORAGE_KEYS[key], newId);
    return newId;
  } catch {
    return `${prefix}_${Date.now()}`;
  }
}

export function resetStoredConversationId(): string {
  if (typeof window === 'undefined') return `conv_${Date.now()}`;
  const newId = `conv_${crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2, 11)}`;
  try {
    localStorage.setItem(STORAGE_KEYS.conversationId, newId);
  } catch {
    // Ignore storage errors
  }
  return newId;
}
