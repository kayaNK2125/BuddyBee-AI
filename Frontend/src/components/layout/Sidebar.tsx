import React, { useEffect } from 'react';
import { Plus, MessageSquare, Trash2, X, Sparkles } from 'lucide-react';
import { useConversation } from '../../hooks/useConversation';
import './Sidebar.css';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const {
    sessions,
    conversationId,
    createSession,
    switchSession,
    deleteSession,
    isLoadingSessions,
    apiHealth,
  } = useConversation();

  // Close on Escape key on mobile
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  return (
    <>
      {/* Mobile Backdrop */}
      <div
        className={`sidebar-backdrop ${isOpen ? 'visible' : ''}`}
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Main Sidebar / Drawer */}
      <aside
        className={`app-sidebar ${isOpen ? 'open' : 'closed'}`}
        aria-label="Conversation History"
      >
        {/* Sidebar Header with Close button (mobile) and New Chat Action */}
        <div className="sidebar-header">
          <div className="sidebar-title-row">
            <span className="sidebar-section-title">Conversations</span>
            <button
              className="sidebar-close-btn"
              onClick={onClose}
              title="Close sidebar"
              aria-label="Close sidebar"
            >
              <X size={16} />
            </button>
          </div>

          {/* Prominent Primary New Chat Button */}
          <button
            className="sidebar-new-chat-btn"
            onClick={createSession}
            aria-label="Start new conversation"
          >
            <Plus size={16} className="new-chat-icon" />
            <span>New Chat</span>
          </button>
        </div>

        {/* Scrollable Session List */}
        <div className="sidebar-session-list" role="list">
          {isLoadingSessions ? (
            <div className="sidebar-loading">
              <div className="sidebar-skeleton" />
              <div className="sidebar-skeleton short" />
              <div className="sidebar-skeleton" />
            </div>
          ) : sessions.length === 0 ? (
            <div className="sidebar-empty-state">
              <div className="empty-icon-box">
                <MessageSquare size={18} />
              </div>
              <span className="empty-title">No conversations yet</span>
              <p className="empty-desc">Your chat sessions will appear here as you explore ideas.</p>
            </div>
          ) : (
            sessions.map((session) => {
              const isActive = session.id === conversationId;
              const date = new Date(session.updatedAt);
              const timeFormatted = isNaN(date.getTime())
                ? ''
                : date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });

              return (
                <div
                  key={session.id}
                  className={`session-item ${isActive ? 'session-active' : ''}`}
                  onClick={() => switchSession(session.id)}
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      switchSession(session.id);
                    }
                  }}
                  aria-current={isActive ? 'true' : undefined}
                  aria-label={`Conversation: ${session.title}`}
                >
                  <MessageSquare size={14} className="session-icon" />

                  <div className="session-info">
                    <span className="session-title" title={session.title}>
                      {session.title}
                    </span>
                    {timeFormatted && <span className="session-meta">{timeFormatted}</span>}
                  </div>

                  <button
                    className="session-delete-btn"
                    onClick={(e) => {
                      e.stopPropagation();
                      deleteSession(session.id);
                    }}
                    title="Delete conversation"
                    aria-label={`Delete conversation: ${session.title}`}
                  >
                    <Trash2 size={13} />
                  </button>
                </div>
              );
            })
          )}
        </div>

        {/* Sidebar Footer with Live Connectivity & Info */}
        <div className="sidebar-footer">
          <div className="footer-system-info">
            <Sparkles size={13} className="footer-sparkle" />
            <span className="footer-label">BuddyBee AI Engine</span>
          </div>
          <span className="footer-status-tag">
            {apiHealth === 'healthy' && 'Online'}
            {apiHealth === 'checking' && 'Checking...'}
            {apiHealth === 'offline' && 'Offline'}
            {apiHealth === 'degraded' && 'Degraded'}
          </span>
        </div>
      </aside>
    </>
  );
};
