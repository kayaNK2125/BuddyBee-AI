import React from 'react';
import { ConversationProvider } from './state/conversationContext';
import { useConversation } from './hooks/useConversation';
import { Header } from './components/layout/Header';
import { Sidebar } from './components/layout/Sidebar';
import { ChatContainer } from './components/chat/ChatContainer';
import { ChrysalisCanvas } from './components/three/ChrysalisCanvas';
import { SettingsModal } from './components/settings/SettingsModal';
import './App.css';

const MainLayout: React.FC = () => {
  const { status, isSidebarOpen, setSidebarOpen, conversationId, isSettingsOpen, closeSettings } = useConversation();

  return (
    <div className="app-shell">
      {/* 1. Header with Connectivity Status, Sidebar Toggle, and New Chat Action */}
      <Header />

      {/* 2. Kinetic Chrysalis 3D Presence Layer (Ambient Background Layer) */}
      <ChrysalisCanvas state={status} />

      {/* 3. Subtle Ambient Light Halo */}
      <div className={`ambient-glow glow-${status}`} aria-hidden="true" />

      {/* 4. Main Body Workspace: Sidebar + Central Chat Container */}
      <div className="app-body">
        <Sidebar isOpen={isSidebarOpen} onClose={() => setSidebarOpen(false)} />
        <ChatContainer key={conversationId} />
      </div>

      {/* 5. AI Provider Settings Modal */}
      <SettingsModal isOpen={isSettingsOpen} onClose={closeSettings} />
    </div>
  );
};

export function App() {
  return (
    <ConversationProvider>
      <MainLayout />
    </ConversationProvider>
  );
}

export default App;
