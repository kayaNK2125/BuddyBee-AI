import React, { useState } from 'react';
import { useConversation } from '../../hooks/useConversation';
import { useAutoScroll } from '../../hooks/useAutoScroll';
import { EmptyState } from './EmptyState';
import { MessageItem } from './MessageItem';
import { CommandCapsule } from './CommandCapsule';
import { AlertCircle, RotateCcw, X, Key, Sparkles, ChevronDown, ChevronUp } from 'lucide-react';
import { getProviderMode } from '../../utils/keyStorage';
import './ChatContainer.css';

/** Strip raw provider JSON / proto paths from details for primary user-facing display.
 *  Lines/segments containing proto type URLs, raw JSON, or "Details: {...}" are removed.
 *  The full original value is still kept in rawDetails for the collapsible tech section.
 */
function sanitizeDetails(details: string | undefined): string | undefined {
  if (!details) return undefined;
  const lines = details.split('\n');
  const cleaned = lines
    .map((l) => {
      // Strip inline "Details: {...}" suffix that Google APIs append
      const detailsIdx = l.indexOf('Details: {');
      if (detailsIdx !== -1) return l.slice(0, detailsIdx).trimEnd();
      const detailsIdx2 = l.indexOf('Details: [');
      if (detailsIdx2 !== -1) return l.slice(0, detailsIdx2).trimEnd();
      return l;
    })
    .filter((l) => {
      const t = l.trim();
      if (!t) return false;
      // Drop lines containing proto type URLs
      if (t.includes('type.googleapis.com')) return false;
      // Drop lines that start with raw JSON
      if (t.startsWith('{') || t.startsWith('[')) return false;
      // Drop lines that are entirely a JSON object (e.g. multi-line stringified)
      if (t.startsWith('"@type"')) return false;
      return true;
    });
  const result = cleaned.join('\n').trim();
  return result.length > 0 ? result : undefined;
}

const ErrorBanner: React.FC = () => {
  const { error, retryLastMessage, dismissError, openSettings, switchToManagedAndRetry, providerMode } = useConversation();
  const [showDetails, setShowDetails] = useState(false);

  if (!error) return null;

  const isCustomMode = providerMode === 'custom' || getProviderMode() === 'custom';
  const cleanDetails = sanitizeDetails(error.details);
  const rawDetails = error.details;
  const hasDetails = !!rawDetails;

  return (
    <div className="error-banner" role="alert">
      <div className="error-banner-main">
        <div className="error-content">
          <AlertCircle size={18} className="error-icon" />
          <div className="error-text">
            <span className="error-message">{error.message}</span>
            {cleanDetails && (
              <span className="error-details">{cleanDetails}</span>
            )}
          </div>
        </div>
        <div className="error-actions">
          {(isCustomMode || error.code === 'API_KEY_REQUIRED' || error.status === 401 || error.code === 'INVALID_API_KEY') && (
            <button
              className="error-settings-btn"
              onClick={openSettings}
              aria-label="Open Settings"
              title="Open Settings"
            >
              <Key size={13} />
              <span>Open Settings</span>
            </button>
          )}
          {error.retryable && (
            <button
              className="error-retry-btn"
              onClick={retryLastMessage}
              aria-label="Try Again"
              title="Try Again"
            >
              <RotateCcw size={13} />
              <span>Try Again</span>
            </button>
          )}
          {isCustomMode && (
            <button
              className="error-switch-managed-btn"
              onClick={switchToManagedAndRetry}
              aria-label="Switch to BuddyBee Managed & Retry"
              title="Switch to BuddyBee Managed & Retry"
            >
              <Sparkles size={13} />
              <span>Switch to BuddyBee Managed &amp; Retry</span>
            </button>
          )}
          {hasDetails && (
            <button
              className="error-details-toggle"
              onClick={() => setShowDetails((v) => !v)}
              aria-expanded={showDetails}
              aria-label={showDetails ? 'Hide technical details' : 'Show technical details'}
              title={showDetails ? 'Hide technical details' : 'Show technical details'}
            >
              {showDetails ? <ChevronUp size={12} /> : <ChevronDown size={12} />}
              <span>{showDetails ? 'Hide details' : 'Show details'}</span>
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
      {showDetails && hasDetails && (
        <div className="error-tech-details">
          <pre className="error-tech-pre">{rawDetails}</pre>
        </div>
      )}
    </div>
  );
};

export const ChatContainer: React.FC = () => {
  const { messages, error, sendMessage } = useConversation();

  const { scrollRef } = useAutoScroll([messages.length, error]);

  const isEmpty = messages.length === 0;

  return (
    <div className="chat-container">
      {/* Sticky Error Banner - sits at top of chat-container, outside scroll area */}
      <ErrorBanner />

      {/* Scrollable Conversation Stream */}
      <div className="chat-timeline" ref={scrollRef}>
        <div className="timeline-inner">
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
