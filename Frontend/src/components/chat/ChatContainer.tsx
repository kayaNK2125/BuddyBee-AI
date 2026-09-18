import React from 'react';
import { useConversation } from '../../hooks/useConversation';
import { useAutoScroll } from '../../hooks/useAutoScroll';
import { EmptyState } from './EmptyState';
import { MessageItem } from './MessageItem';
import { CommandCapsule } from './CommandCapsule';
import { AlertCircle, RotateCcw, X } from 'lucide-react';
import './ChatContainer.css';

export const ChatContainer: React.FC = () => {
  const {
    messages,
    error,
    sendMessage,
    retryLastMessage,
    dismissError,
  } = useConversation();

  const { scrollRef } = useAutoScroll([messages.length, error]);

  const isEmpty = messages.length === 0;

  return (
    <div className="chat-container">
      {/* Scrollable Conversation Stream */}
      <div className="chat-timeline" ref={scrollRef}>
        <div className="timeline-inner">
          {/* Error Banner */}
          {error && (
            <div className="error-banner" role="alert">
              <div className="error-content">
                <AlertCircle size={18} className="error-icon" />
                <div className="error-text">
                  <span className="error-message">{error.message}</span>
                  {error.details && <span className="error-details">{error.details}</span>}
                </div>
              </div>
              <div className="error-actions">
                {error.retryable && (
                  <button
                    className="error-retry-btn"
                    onClick={retryLastMessage}
                    aria-label="Retry failed message"
                  >
                    <RotateCcw size={13} />
                    <span>Retry</span>
                  </button>
                )}
                <button
                  className="error-dismiss-btn"
                  onClick={dismissError}
                  aria-label="Dismiss error"
                >
                  <X size={15} />
                </button>
              </div>
            </div>
          )}

          {isEmpty ? (
            <EmptyState onSelectPrompt={sendMessage} />
          ) : (
            <div className="message-list" role="log" aria-live="polite">
              {messages.map((msg, idx) => (
                <MessageItem
                  key={msg.id}
                  message={msg}
                  isLatest={idx === messages.length - 1}
                />
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Fixed-Position Floating Command Capsule */}
      <CommandCapsule />
    </div>
  );
};
