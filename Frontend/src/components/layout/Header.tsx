import React from 'react';
import { PanelLeft, PanelLeftClose, Plus, Cpu, Settings } from 'lucide-react';
import { useConversation } from '../../hooks/useConversation';
import './Header.css';

export const Header: React.FC = () => {
  const {
    apiHealth,
    activeProvider,
    createSession,
    checkHealth,
    isSidebarOpen,
    toggleSidebar,
    openSettings,
  } = useConversation();

  return (
    <header className="app-header">
      <div className="header-left">
        {/* Sidebar / Drawer Toggle */}
        <button
          className={`sidebar-toggle-btn ${isSidebarOpen ? 'active' : ''}`}
          onClick={toggleSidebar}
          title={isSidebarOpen ? 'Close sidebar' : 'Open sidebar'}
          aria-label={isSidebarOpen ? 'Close sidebar' : 'Open sidebar'}
          aria-expanded={isSidebarOpen}
        >
          {isSidebarOpen ? <PanelLeftClose size={18} /> : <PanelLeft size={18} />}
        </button>

        {/* Brand Mark */}
        <div className="brand-mark" aria-label="BuddyBee Logo">
          <svg className="brand-hexagon" viewBox="0 0 32 32" fill="none">
            <polygon points="16,2 29,9.5 29,24.5 16,32 3,24.5 3,9.5" fill="#141821" stroke="#F59E0B" strokeWidth="2"/>
            <circle cx="16" cy="16" r="3.5" fill="#FDE68A"/>
          </svg>
          <span className="brand-name">BuddyBee</span>
        </div>
        <span className="brand-badge">AI Presence</span>
      </div>

      <div className="header-right">
        {/* Backend Connectivity Status (Accurate live Online/Offline probe) */}
        <button
          className={`status-pill health-${apiHealth}`}
          onClick={checkHealth}
          title="Click to re-check API connection"
          aria-label={`API Status: ${apiHealth}`}
        >
          <span className="status-dot" />
          <span className="status-label">
            {apiHealth === 'healthy' && 'Online'}
            {apiHealth === 'checking' && 'Checking...'}
            {apiHealth === 'offline' && 'Offline'}
            {apiHealth === 'degraded' && 'Degraded'}
          </span>
        </button>

        {/* Active Engine Indicator */}
        {activeProvider && (
          <div className="provider-pill" title={`Processed by ${activeProvider}`}>
            <Cpu size={14} className="provider-icon" />
            <span>{activeProvider}</span>
          </div>
        )}

        {/* AI Provider Settings Button */}
        <button
          className="header-settings-btn"
          onClick={openSettings}
          title="AI Provider Settings"
          aria-label="AI Provider Settings"
        >
          <Settings size={15} />
          <span className="btn-text">Settings</span>
        </button>

        {/* Prominent Header New Chat Button (replaces the old restart/refresh icon) */}
        <button
          className="header-new-chat-btn"
          onClick={createSession}
          title="Start new conversation"
          aria-label="Start new conversation"
        >
          <Plus size={15} />
          <span className="btn-text">New Chat</span>
        </button>
      </div>
    </header>
  );
};
