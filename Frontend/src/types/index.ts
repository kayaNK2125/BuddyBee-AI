export type ChrysalisState =
  | 'idle'
  | 'interacting'
  | 'typing'
  | 'sending'
  | 'thinking'
  | 'responding'
  | 'error';

export type ApiHealth = 'checking' | 'healthy' | 'degraded' | 'offline';

export interface Message {
  id: string;
  conversationId: string;
  text: string;
  sender: 'User' | 'BuddyBee';
  timestamp: string;
  provider?: string;
  isProgressive?: boolean;
}

export interface AppError {
  message: string;
  details?: string;
  status?: number;
  retryable?: boolean;
}
