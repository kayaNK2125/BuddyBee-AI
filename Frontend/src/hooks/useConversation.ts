import { useContext } from 'react';
import { ConversationContext, type ConversationContextValue } from '../state/conversationContext';

export function useConversation(): ConversationContextValue {
  const context = useContext(ConversationContext);
  if (!context) {
    throw new Error('useConversation must be used within a ConversationProvider');
  }
  return context;
}
