import React, { createContext, useCallback, useEffect, useRef, useState } from 'react';
import { api } from '../api/client';
import type { Message, ChrysalisState, ApiHealth, AppError } from '../types';
import { getOrCreateId, resetStoredConversationId } from '../utils/storage';

export interface ConversationContextValue {
  messages: Message[];
  status: ChrysalisState;
  apiHealth: ApiHealth;
  activeProvider: string | null;
  error: AppError | null;
  conversationId: string;
  userId: string;
  sendMessage: (text: string) => Promise<void>;
  retryLastMessage: () => Promise<void>;
  resetConversation: () => void;
  dismissError: () => void;
  setStatus: (status: ChrysalisState) => void;
  checkHealth: () => Promise<void>;
}

export const ConversationContext = createContext<ConversationContextValue | null>(null);

export const ConversationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [status, setStatus] = useState<ChrysalisState>('idle');
  const [apiHealth, setApiHealth] = useState<ApiHealth>('checking');
  const [activeProvider, setActiveProvider] = useState<string | null>(null);
  const [error, setError] = useState<AppError | null>(null);

  const [conversationId, setConversationId] = useState<string>(() => getOrCreateId('conversationId', 'conv'));
  const [userId] = useState<string>(() => getOrCreateId('userId', 'user'));

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
    setError(null);
    setStatus('idle');
  }, []);

  const resetConversation = useCallback(() => {
    const newConvId = resetStoredConversationId();
    setConversationId(newConvId);
    setMessages([]);
    setError(null);
    setStatus('idle');
    setActiveProvider(null);
  }, []);

  const sendMessage = useCallback(
    async (text: string) => {
      const trimmed = text.trim();
      if (!trimmed || status === 'sending' || status === 'thinking') return;

      lastSentTextRef.current = trimmed;
      setError(null);

      // 1. Optimistic User Message
      const userMessage: Message = {
        id: `msg_${Date.now()}_u`,
        conversationId,
        text: trimmed,
        sender: 'User',
        timestamp: new Date().toISOString(),
      };

      setMessages((prev) => [...prev, userMessage]);

      // 2. 3D Transition -> sending
      setStatus('sending');

      try {
        // Small delay so the 150ms contraction is visible before thinking orbit
        await new Promise((r) => setTimeout(r, 120));
        setStatus('thinking');

        const response = await api.postChat({
          conversationId,
          userId,
          message: trimmed,
        });

        // 3. 3D Transition -> responding
        setStatus('responding');
        setActiveProvider(response.provider);
        setApiHealth('healthy');

        const botMessage: Message = {
          id: `msg_${Date.now()}_b`,
          conversationId,
          text: response.reply,
          sender: 'BuddyBee',
          timestamp: new Date().toISOString(),
          provider: response.provider,
          isProgressive: true,
        };

        setMessages((prev) => [...prev, botMessage]);

        // After response delivery, transition back to idle
        setTimeout(() => {
          setStatus('idle');
        }, 1800);
      } catch (err: unknown) {
        setStatus('error');
        setApiHealth('offline');
        const appErr = err as AppError;
        setError(appErr);
      }
    },
    [conversationId, userId, status]
  );

  const retryLastMessage = useCallback(async () => {
    if (lastSentTextRef.current) {
      // Remove failed bot attempt if any and resend
      await sendMessage(lastSentTextRef.current);
    }
  }, [sendMessage]);

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
        sendMessage,
        retryLastMessage,
        resetConversation,
        dismissError,
        setStatus,
        checkHealth,
      }}
    >
      {children}
    </ConversationContext.Provider>
  );
};
