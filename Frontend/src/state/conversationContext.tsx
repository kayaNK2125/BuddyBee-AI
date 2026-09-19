import React, { createContext, useCallback, useEffect, useRef, useState } from 'react';
import { api } from '../api/client';
import type { Message, ChrysalisState, ApiHealth, AppError, SessionSummary } from '../types';
import { getProviderMode, setProviderMode, type ProviderMode } from '../utils/keyStorage';
import {
  getOrCreateId,
  resetStoredConversationId,
  setStoredConversationId,
  loadStoredSessions,
  saveStoredSessions,
  loadSessionMessages,
  saveSessionMessages,
  deleteSessionMessages,
} from '../utils/storage';

export interface ConversationContextValue {
  messages: Message[];
  status: ChrysalisState;
  apiHealth: ApiHealth;
  activeProvider: string | null;
  error: AppError | null;
  conversationId: string;
  userId: string;

  // Session state & actions
  sessions: SessionSummary[];
  isLoadingSessions: boolean;
  createSession: () => void;
  switchSession: (sessionId: string) => void;
  deleteSession: (sessionId: string) => void;

  // Sidebar state & actions
  isSidebarOpen: boolean;
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;

  // Settings modal state & actions
  isSettingsOpen: boolean;
  openSettings: () => void;
  closeSettings: () => void;
  setSettingsOpen: (open: boolean) => void;

  providerMode: ProviderMode;
  sendMessage: (text: string, isRetry?: boolean) => Promise<void>;
  retryLastMessage: () => Promise<void>;
  switchToManagedAndRetry: () => Promise<void>;
  resetConversation: () => void;
  dismissError: () => void;
  setStatus: (status: ChrysalisState) => void;
  checkHealth: () => Promise<void>;
}

export const ConversationContext = createContext<ConversationContextValue | null>(null);

export const ConversationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [conversationId, setConversationId] = useState<string>(() => getOrCreateId('conversationId', 'conv'));
  const [userId] = useState<string>(() => getOrCreateId('userId', 'user'));

  // Synchronous ref to active conversationId so async operations never read stale closures
  const activeConversationIdRef = useRef<string>(conversationId);
  useEffect(() => {
    activeConversationIdRef.current = conversationId;
  }, [conversationId]);

  // Set of conversation IDs currently with in-flight requests
  const pendingRequestsRef = useRef<Set<string>>(new Set());

  // Set of conversation IDs that have been explicitly deleted while requests may be in flight
  const deletedConversationsRef = useRef<Set<string>>(new Set());

  // Per-conversation retry text and error state (prevents cross-session retry contamination)
  const retryTextByConversationRef = useRef<Map<string, string>>(new Map());
  const errorsByConversationRef = useRef<Map<string, AppError>>(new Map());

  // Initialize messages directly from persistent session storage for the active conversation
  const [messages, setMessages] = useState<Message[]>(() => {
    const initialId = getOrCreateId('conversationId', 'conv');
    return loadSessionMessages(initialId);
  });

  const [status, setStatus] = useState<ChrysalisState>('idle');
  const [apiHealth, setApiHealth] = useState<ApiHealth>('checking');
  const [activeProvider, setActiveProvider] = useState<string | null>(null);
  const [error, setError] = useState<AppError | null>(null);

  // Session summaries list
  const [sessions, setSessions] = useState<SessionSummary[]>(() => loadStoredSessions());
  const [isLoadingSessions] = useState(false);

  // Responsive sidebar state: open by default on wide screens (>= 1024px)
  const [isSidebarOpen, setSidebarOpen] = useState<boolean>(() => {
    if (typeof window === 'undefined') return true;
    return window.innerWidth >= 1024;
  });

  // Provider mode state synced with sessionStorage
  const [providerMode, setProviderModeState] = useState<ProviderMode>(() => getProviderMode());

  // Track previous provider mode so we can detect Custom -> Managed transitions
  const prevProviderModeRef = useRef<ProviderMode>(getProviderMode());

  // Guard against concurrent auto-retries triggered by mode change events
  const modeChangeRetryInFlightRef = useRef<boolean>(false);

  // Listen for provider mode changes across components and storage
  useEffect(() => {
    const handleModeChange = (_e: Event) => {
      const newMode = getProviderMode();
      const prevMode = prevProviderModeRef.current;
      prevProviderModeRef.current = newMode;
      setProviderModeState(newMode);

      // Fix 3: When the user manually switches Custom -> Managed via Settings
      // and there is a pending failed Custom-mode request for the active conversation,
      // automatically retry that failed request using Managed mode.
      //
      // Guard conditions:
      //   1. Transition is specifically Custom -> Managed (not any other change)
      //   2. There is retry text recorded for the active conversation (i.e. a request failed)
      //   3. There is no request already in-flight for that conversation
      //   4. No concurrent mode-change retry is already running
      //   5. The mode-change event carries detail='managed' (i.e. came from setProviderMode)
      //      OR the previous mode was 'custom' (catches storage events too)
      if (
        prevMode === 'custom' &&
        newMode === 'managed' &&
        !modeChangeRetryInFlightRef.current
      ) {
        const targetConvId = activeConversationIdRef.current;
        const hasPendingFailedRequest =
          retryTextByConversationRef.current.has(targetConvId) &&
          !pendingRequestsRef.current.has(targetConvId);

        if (hasPendingFailedRequest) {
          modeChangeRetryInFlightRef.current = true;
          // Use a microtask delay so mode state settles before retry dispatches
          setTimeout(() => {
            const textToRetry = retryTextByConversationRef.current.get(activeConversationIdRef.current);
            if (
              textToRetry &&
              !pendingRequestsRef.current.has(activeConversationIdRef.current)
            ) {
              // sendMessage is stable (deps: [userId]) and uses activeConversationIdRef
              // so calling it directly here is safe without introducing a circular dep
              sendMessageRef.current(textToRetry, true).finally(() => {
                modeChangeRetryInFlightRef.current = false;
              });
            } else {
              modeChangeRetryInFlightRef.current = false;
            }
          }, 50);
        }
      }
    };
    window.addEventListener('buddybee_provider_mode_changed', handleModeChange);
    window.addEventListener('storage', handleModeChange);
    return () => {
      window.removeEventListener('buddybee_provider_mode_changed', handleModeChange);
      window.removeEventListener('storage', handleModeChange);
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Settings modal open/close state
  const [isSettingsOpen, setSettingsOpen] = useState<boolean>(false);
  const openSettings = useCallback(() => {
    setProviderModeState(getProviderMode());
    setSettingsOpen(true);
  }, []);
  const closeSettings = useCallback(() => {
    setProviderModeState(getProviderMode());
    setSettingsOpen(false);
  }, []);

  const lastSentTextRef = useRef<string | null>(null);

  // Ping backend to check health on mount
  const checkHealth = useCallback(async () => {
    try {
      setApiHealth('checking');
      await api.ping();
      setApiHealth('healthy');
    } catch {
      setApiHealth('offline');
    }
  }, []);

  useEffect(() => {
    checkHealth();
  }, [checkHealth]);

  const dismissError = useCallback(() => {
    const currentId = activeConversationIdRef.current;
    errorsByConversationRef.current.delete(currentId);
    setError(null);
    setStatus('idle');
  }, []);

  const toggleSidebar = useCallback(() => {
    setSidebarOpen((prev) => !prev);
  }, []);

  // Prominent New Chat / Create Session action
  const createSession = useCallback(() => {
    const newConvId = resetStoredConversationId();
    activeConversationIdRef.current = newConvId;
    setConversationId(newConvId);
    setMessages([]);
    setError(null);
    setStatus('idle');
    setActiveProvider(null);

    // Auto-close drawer on small screens
    if (typeof window !== 'undefined' && window.innerWidth < 768) {
      setSidebarOpen(false);
    }
  }, []);

  // Switch between conversation sessions
  const switchSession = useCallback((sessionId: string) => {
    if (!sessionId || sessionId === activeConversationIdRef.current) return;

    // 1. Immediately update active ref so in-flight async operations know active conversation changed
    activeConversationIdRef.current = sessionId;

    // 2. Load target conversation messages directly from storage
    const targetMessages = loadSessionMessages(sessionId);

    // 3. Atomically update active conversation ID and messages
    setStoredConversationId(sessionId);
    setConversationId(sessionId);
    setMessages(targetMessages);

    // 4. Restore conversation-specific error state
    const convError = errorsByConversationRef.current.get(sessionId) || null;
    setError(convError);

    // If the target conversation has an in-flight request, show thinking; otherwise error or idle
    if (pendingRequestsRef.current.has(sessionId)) {
      setStatus('thinking');
    } else if (convError) {
      setStatus('error');
    } else {
      setStatus('idle');
    }

    // Auto-close drawer on small screens
    if (typeof window !== 'undefined' && window.innerWidth < 768) {
      setSidebarOpen(false);
    }
  }, []);

  // Delete a conversation session
  const deleteSession = useCallback((sessionId: string) => {
    deletedConversationsRef.current.add(sessionId);

    setSessions((prev) => {
      const updated = prev.filter((s) => s.id !== sessionId);
      return saveStoredSessions(updated, activeConversationIdRef.current);
    });

    deleteSessionMessages(sessionId);
    pendingRequestsRef.current.delete(sessionId);
    errorsByConversationRef.current.delete(sessionId);
    retryTextByConversationRef.current.delete(sessionId);

    // If deleting the active session, start a fresh session
    if (sessionId === activeConversationIdRef.current) {
      createSession();
    }
  }, [createSession]);

  // Stable ref always pointing to latest sendMessage (used by mode-change auto-retry)
  const sendMessageRef = useRef<(text: string, isRetry?: boolean) => Promise<void>>(async () => {});

  const sendMessage = useCallback(
    async (text: string, isRetry = false) => {
      const trimmed = text.trim();
      const targetConvId = activeConversationIdRef.current;
      if (!trimmed) return;

      // Do not allow double submission for the same conversation while in-flight
      if (pendingRequestsRef.current.has(targetConvId)) return;

      pendingRequestsRef.current.add(targetConvId);
      lastSentTextRef.current = trimmed;
      retryTextByConversationRef.current.set(targetConvId, trimmed);
      errorsByConversationRef.current.delete(targetConvId);
      if (activeConversationIdRef.current === targetConvId) {
        setError(null);
      }

      const currentTargetMsgs = loadSessionMessages(targetConvId);
      const alreadyHasUserMessage = currentTargetMsgs.some(
        (m) => m.sender === 'User' && m.text === trimmed
      );

      if (!isRetry || !alreadyHasUserMessage) {
        // 1. Optimistic User Message strictly tagged with originating conversation ID
        const userMessage: Message = {
          id: 'msg_' + Date.now() + '_u',
          conversationId: targetConvId,
          text: trimmed,
          sender: 'User',
          timestamp: new Date().toISOString(),
        };

        const updatedWithUser = [...currentTargetMsgs, userMessage];
        saveSessionMessages(targetConvId, updatedWithUser);

        // Only update active UI if the user is still looking at targetConvId
        if (activeConversationIdRef.current === targetConvId) {
          setMessages(updatedWithUser);
          setStatus('sending');
        }

        // 2. Register or update session summary for targetConvId
        setSessions((prev) => {
          const existingIdx = prev.findIndex((s) => s.id === targetConvId);
          const chars = Array.from(trimmed);
          const titleSnippet = chars.length > 42 ? chars.slice(0, 42).join('').trim() + '…' : trimmed;

          let updated: SessionSummary[];
          if (existingIdx >= 0) {
            const existing = prev[existingIdx];
            const updatedItem: SessionSummary = {
              ...existing,
              updatedAt: new Date().toISOString(),
              messageCount: existing.messageCount + 1,
              snippet: titleSnippet,
            };
            updated = [updatedItem, ...prev.filter((_, idx) => idx !== existingIdx)];
          } else {
            const newSession: SessionSummary = {
              id: targetConvId,
              title: titleSnippet,
              createdAt: new Date().toISOString(),
              updatedAt: new Date().toISOString(),
              messageCount: 1,
              snippet: titleSnippet,
            };
            updated = [newSession, ...prev];
          }

          return saveStoredSessions(updated, targetConvId);
        });
      } else {
        // On retry of an existing message, do not duplicate the user entry
        if (activeConversationIdRef.current === targetConvId) {
          setStatus('sending');
        }
      }

      try {
        await new Promise((r) => setTimeout(r, 120));
        if (deletedConversationsRef.current.has(targetConvId)) {
          return;
        }
        if (activeConversationIdRef.current === targetConvId) {
          setStatus('thinking');
        }

        const response = await api.postChat({
          conversationId: targetConvId,
          userId,
          message: trimmed,
        });

        // INVARIANT (T2.6 / T7.2): Once a conversation is deleted, any later response
        // belonging to that conversation must be discarded and must NOT commit to storage.
        if (deletedConversationsRef.current.has(targetConvId)) {
          return;
        }

        const botMessage: Message = {
          id: 'msg_' + Date.now() + '_b',
          conversationId: targetConvId,
          text: response.reply,
          sender: 'BuddyBee',
          timestamp: new Date().toISOString(),
          provider: response.provider,
          isProgressive: true,
        };

        // INVARIANT: Response MUST always be committed to targetConvId's storage
        const latestTargetMsgs = loadSessionMessages(targetConvId);
        const finalTargetMessages = latestTargetMsgs.some((m) => m.id === botMessage.id)
          ? latestTargetMsgs
          : [...latestTargetMsgs, botMessage];
        saveSessionMessages(targetConvId, finalTargetMessages);

        // Update session message count in session list
        setSessions((prev) => {
          const existingIdx = prev.findIndex((s) => s.id === targetConvId);
          if (existingIdx >= 0) {
            const updated = [...prev];
            updated[existingIdx] = {
              ...updated[existingIdx],
              messageCount: finalTargetMessages.length,
              updatedAt: new Date().toISOString(),
            };
            return saveStoredSessions(updated, targetConvId);
          }
          return prev;
        });

        retryTextByConversationRef.current.delete(targetConvId);
        errorsByConversationRef.current.delete(targetConvId);

        // INVARIANT: Only update active view if the user is STILL viewing targetConvId
        if (activeConversationIdRef.current === targetConvId) {
          setError(null);
          setMessages(finalTargetMessages);
          setStatus('responding');
          setActiveProvider(response.provider);
          setApiHealth('healthy');

          setTimeout(() => {
            if (activeConversationIdRef.current === targetConvId) {
              setStatus('idle');
            }
          }, 1800);
        } else {
          // Response arrived while user is in another conversation:
          // The response is safely stored in targetConvId's history!
          // Active view is completely untouched.
          setApiHealth('healthy');
        }
      } catch (err: unknown) {
        if (deletedConversationsRef.current.has(targetConvId)) {
          return;
        }
        const appErr = err as AppError;
        // Do not mark backend offline for credential/configuration errors (400, 401) or rate limits (429)
        if (appErr.status && appErr.status < 500) {
          setApiHealth('healthy');
        } else {
          setApiHealth('degraded');
        }
        errorsByConversationRef.current.set(targetConvId, appErr);
        if (activeConversationIdRef.current === targetConvId) {
          setStatus('error');
          setError(appErr);
        }
      } finally {
        pendingRequestsRef.current.delete(targetConvId);
        deletedConversationsRef.current.delete(targetConvId);
      }
    },
    [userId]
  );

  // Keep the stable ref updated after every render so the mode-change listener
  // always calls the latest closure without recreating event listeners.
  useEffect(() => {
    sendMessageRef.current = sendMessage;
  });

  const retryLastMessage = useCallback(async () => {
    const targetConvId = activeConversationIdRef.current;
    let textToRetry = retryTextByConversationRef.current.get(targetConvId);
    if (!textToRetry) {
      // Fallback: Check if the last message in the target conversation is a User message
      const targetMsgs = loadSessionMessages(targetConvId);
      if (targetMsgs.length > 0 && targetMsgs[targetMsgs.length - 1].sender === 'User') {
        textToRetry = targetMsgs[targetMsgs.length - 1].text;
      }
    }
    if (textToRetry) {
      await sendMessage(textToRetry, true);
    }
  }, [sendMessage]);

  const switchToManagedAndRetry = useCallback(async () => {
    // 1. Prevent the mode-change event listener from also triggering an auto-retry,
    //    since we are explicitly retrying here already.
    modeChangeRetryInFlightRef.current = true;
    prevProviderModeRef.current = 'managed'; // pre-update so listener sees no transition

    // 2. Automatically switch provider mode to BuddyBee Managed in storage & state
    // (Existing session-stored API keys in sessionStorage are never deleted or cleared)
    setProviderMode('managed');
    setProviderModeState('managed');

    // 3. Automatically retry original failed user message with anti-duplication
    try {
      await retryLastMessage();
    } finally {
      modeChangeRetryInFlightRef.current = false;
    }
  }, [retryLastMessage]);

  return (
    <ConversationContext.Provider
      value={{
        messages,
        status,
        apiHealth,
        activeProvider,
        error,
        conversationId,
        userId,
        sessions,
        isLoadingSessions,
        createSession,
        switchSession,
        deleteSession,
        isSidebarOpen,
        toggleSidebar,
        setSidebarOpen,
        isSettingsOpen,
        openSettings,
        closeSettings,
        setSettingsOpen,
        providerMode,
        sendMessage,
        retryLastMessage,
        switchToManagedAndRetry,
        resetConversation: createSession,
        dismissError,
        setStatus,
        checkHealth,
      }}
    >
      {children}
    </ConversationContext.Provider>
  );
};
