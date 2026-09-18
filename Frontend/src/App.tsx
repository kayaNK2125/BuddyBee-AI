import React from 'react';
import { ConversationProvider } from './state/conversationContext';
import { useConversation } from './hooks/useConversation';
import { Header } from './components/layout/Header';
import { ChatContainer } from './components/chat/ChatContainer';
import { ChrysalisCanvas } from './components/three/ChrysalisCanvas';
import './App.css';

const MainLayout: React.FC = () => {
  const { status } = useConversation();

  return (
    <div className="app-shell">
      {/* 1. Header with Connectivity Status */}
      <Header />

      {/* 2. Kinetic Chrysalis 3D Presence Layer */}
      <ChrysalisCanvas state={status} />

      {/* 3. Subtle Ambient Light Halo */}
      <div className={`ambient-glow glow-${status}`} aria-hidden="true" />

      {/* 4. Conversational Workspace */}
      <ChatContainer />
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
