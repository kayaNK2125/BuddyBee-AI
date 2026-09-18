import React, { useRef, useState } from 'react';
import { ArrowUp, Loader2 } from 'lucide-react';
import { useConversation } from '../../hooks/useConversation';
import './CommandCapsule.css';

export const CommandCapsule: React.FC = () => {
  const { sendMessage, status, setStatus } = useConversation();
  const [inputText, setInputText] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);
  const typingTimeoutRef = useRef<number | null>(null);

  const isBusy = status === 'sending' || status === 'thinking';

  // Auto-resize textarea
  const handleInputChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const value = e.target.value;
    setInputText(value);

    // Communicate typing state to 3D chrysalis
    if (value.trim().length > 0 && status === 'idle') {
      setStatus('typing');
    }

    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }
    typingTimeoutRef.current = window.setTimeout(() => {
      if (status === 'typing') {
        setStatus('idle');
      }
    }, 1500);

    // Resize height
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${Math.min(textareaRef.current.scrollHeight, 160)}px`;
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleSend = async () => {
    if (!inputText.trim() || isBusy) return;

    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }

    const message = inputText;
    setInputText('');
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
    }

    await sendMessage(message);
  };

  return (
    <div className="command-capsule-container">
      <div className={`command-capsule ${isBusy ? 'capsule-busy' : ''}`}>
        <textarea
          ref={textareaRef}
          className="capsule-textarea"
          value={inputText}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          placeholder="Ask BuddyBee to analyze, design, or solve..."
          rows={1}
          disabled={isBusy}
          aria-label="Chat input"
        />

        <button
          className={`capsule-send-btn ${inputText.trim() ? 'btn-active' : ''}`}
          onClick={handleSend}
          disabled={!inputText.trim() || isBusy}
          aria-label="Send message"
        >
          {isBusy ? <Loader2 size={16} className="btn-spinner" /> : <ArrowUp size={16} />}
        </button>
      </div>
      <div className="capsule-footer-note">
        <span>Enter to send · Shift+Enter for newline</span>
      </div>
    </div>
  );
};
