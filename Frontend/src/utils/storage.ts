import type { Message } from '../types';
import type { SessionSummary } from '../types/session';

const STORAGE_KEYS = {
  conversationId: 'buddybee_conversation_id',
  userId: 'buddybee_user_id',
  sessions: 'buddybee_sessions',
} as const;

// In-memory cache fallback for environments without localStorage or fast access
const inMemoryMessages = new Map<string, Message[]>();

// Track conversations whose last localStorage write failed, keeping inMemoryMessages authoritative
const failedWriteConversations = new Set<string>();

export function conversationExists(id: string): boolean {
  if (!id) return false;
  if (inMemoryMessages.has(id)) return true;
  if (typeof window !== 'undefined') {
    try {
      if (localStorage.getItem('buddybee_messages_' + id) !== null) return true;
      const rawSessions = localStorage.getItem(STORAGE_KEYS.sessions);
      if (rawSessions) {
        const parsed = JSON.parse(rawSessions);
        if (Array.isArray(parsed) && parsed.some((s: SessionSummary) => s && s.id === id)) {
          return true;
        }
      }
    } catch {
      // Ignore storage errors
    }
  }
  return false;
}

export function generateUniqueConversationId(): string {
  const maxAttempts = 50;
  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    let suffix: string;
    if (typeof crypto !== 'undefined' && crypto.randomUUID) {
      suffix = crypto.randomUUID();
    } else {
      suffix = Math.random().toString(36).substring(2, 11);
    }
    if (attempt > 0) {
      suffix = suffix + '_' + Date.now() + '_' + attempt;
    }
    const candidateId = 'conv_' + suffix;
    if (!conversationExists(candidateId)) {
      return candidateId;
    }
  }
  return 'conv_' + Date.now() + '_' + Math.random().toString(36).substring(2, 9);
}

export function getOrCreateId(key: 'conversationId' | 'userId', prefix: string): string {
  if (typeof window === 'undefined') return prefix + '_' + Date.now();
  try {
    const existing = localStorage.getItem(STORAGE_KEYS[key]);
    if (existing) return existing;
    let newId: string;
    if (key === 'conversationId') {
      newId = generateUniqueConversationId();
    } else {
      const randomSuffix = typeof crypto !== 'undefined' && crypto.randomUUID
        ? crypto.randomUUID()
        : Math.random().toString(36).substring(2, 11);
      newId = prefix + '_' + randomSuffix;
    }
    localStorage.setItem(STORAGE_KEYS[key], newId);
    return newId;
  } catch {
    return prefix + '_' + Date.now();
  }
}

export function resetStoredConversationId(): string {
  if (typeof window === 'undefined') return 'conv_' + Date.now();
  const newId = generateUniqueConversationId();
  try {
    localStorage.setItem(STORAGE_KEYS.conversationId, newId);
  } catch {
    // Ignore storage errors
  }
  return newId;
}

export function setStoredConversationId(id: string): void {
  if (typeof window === 'undefined') return;
  try {
    localStorage.setItem(STORAGE_KEYS.conversationId, id);
  } catch {
    // Ignore storage errors
  }
}

export const MAX_SESSIONS = 30;

export function loadStoredSessions(): SessionSummary[] {
  if (typeof window === 'undefined') return [];
  try {
    const raw = localStorage.getItem(STORAGE_KEYS.sessions);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed)) return parsed.slice(0, MAX_SESSIONS);
  } catch {
    // Ignore parse errors
  }
  return [];
}

export function saveStoredSessions(sessions: SessionSummary[], activeId?: string): SessionSummary[] {
  if (typeof window === 'undefined') return sessions.slice(0, MAX_SESSIONS);

  let kept = sessions;
  if (sessions.length > MAX_SESSIONS) {
    const toEvictCount = sessions.length - MAX_SESSIONS;
    const evictedIds = new Set<string>();

    // Evict oldest sessions from the end, protecting activeId if provided
    for (let i = sessions.length - 1; i >= 0 && evictedIds.size < toEvictCount; i--) {
      const s = sessions[i];
      if (!activeId || s.id !== activeId) {
        evictedIds.add(s.id);
      }
    }

    // Fallback if needed to satisfy toEvictCount
    if (evictedIds.size < toEvictCount) {
      for (let i = sessions.length - 1; i >= 0 && evictedIds.size < toEvictCount; i--) {
        evictedIds.add(sessions[i].id);
      }
    }

    // Evicted sessions must have their message storage cleaned up as part of the same operation
    evictedIds.forEach((id) => {
      deleteSessionMessages(id);
    });

    kept = sessions.filter((s) => !evictedIds.has(s.id));
  }

  try {
    localStorage.setItem(STORAGE_KEYS.sessions, JSON.stringify(kept));
  } catch {
    // Ignore storage errors
  }

  return kept;
}

export function loadSessionMessages(conversationId: string): Message[] {
  if (!conversationId) return [];

  // If localStorage write previously failed for this conversation,
  // in-memory state is authoritative and must not be overwritten by stale localStorage data
  if (failedWriteConversations.has(conversationId)) {
    const mem = inMemoryMessages.get(conversationId);
    if (mem) return mem.slice();
  }

  if (typeof window !== 'undefined') {
    try {
      const raw = localStorage.getItem('buddybee_messages_' + conversationId);
      if (raw) {
        const parsed = JSON.parse(raw);
        if (Array.isArray(parsed)) {
          inMemoryMessages.set(conversationId, parsed.slice());
          return parsed.slice();
        }
      }
    } catch {
      // Ignore parse/storage errors
    }
  }
  const mem = inMemoryMessages.get(conversationId);
  return mem ? mem.slice() : [];
}

export function saveSessionMessages(conversationId: string, messages: Message[]): void {
  if (!conversationId) return;
  const clone = messages.slice();
  inMemoryMessages.set(conversationId, clone);
  if (typeof window !== 'undefined') {
    try {
      localStorage.setItem('buddybee_messages_' + conversationId, JSON.stringify(clone));
      failedWriteConversations.delete(conversationId);
    } catch {
      // Write failed (e.g. QuotaExceededError). Mark conversation so in-memory remains authoritative.
      failedWriteConversations.add(conversationId);
    }
  }
}

export function deleteSessionMessages(conversationId: string): void {
  if (!conversationId) return;
  inMemoryMessages.delete(conversationId);
  failedWriteConversations.delete(conversationId);
  if (typeof window !== 'undefined') {
    try {
      localStorage.removeItem('buddybee_messages_' + conversationId);
    } catch {
      // Ignore storage errors
    }
  }
}
