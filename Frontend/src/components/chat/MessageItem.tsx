import React from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { User, Cpu, Copy, Check } from 'lucide-react';
import type { Message } from '../../types';
import { useTypewriter } from '../../hooks/useTypewriter';
import './MessageItem.css';

interface MessageItemProps {
  message: Message;
  isLatest: boolean;
}

export const MessageItem: React.FC<MessageItemProps> = ({ message, isLatest }) => {
  const isUser = message.sender === 'User';
  const [copied, setCopied] = React.useState(false);

  // Progressive typewriter effect only on latest bot response
  const shouldTypewrite = !isUser && message.isProgressive && isLatest;
  const { displayedText } = useTypewriter(message.text, shouldTypewrite, 4, 16);

  const contentToRender = shouldTypewrite ? displayedText : message.text;

  const handleCopy = () => {
    navigator.clipboard.writeText(message.text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  // Safe link renderer: enforces target="_blank", noopener, and scheme validation
  const safeLinkRenderer = ({ href, children }: { href?: string; children?: React.ReactNode }) => {
    if (!href) return <>{children}</>;
    const isSafe = /^https?:\/\//i.test(href) || /^mailto:/i.test(href);
    if (!isSafe) {
      return <span>{children}</span>;
    }
    return (
      <a href={href} target="_blank" rel="noopener noreferrer">
        {children}
      </a>
    );
  };

  return (
    <article className={`message-row ${isUser ? 'row-user' : 'row-bot'}`}>
      <div className="message-avatar">
        {isUser ? (
          <User size={15} />
        ) : (
          <div className="bot-avatar-mark">
            <Cpu size={14} />
          </div>
        )}
      </div>

      <div className={`message-bubble ${isUser ? 'bubble-user' : 'bubble-bot'}`}>
        <header className="message-header">
          <span className="message-sender">{isUser ? 'You' : 'BuddyBee'}</span>
          {!isUser && message.provider && (
            <span className="message-provider-tag">{message.provider}</span>
          )}
          <span className="message-time">
            {new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
          </span>
        </header>

        <div className="message-body">
          {isUser ? (
            <p className="user-text">{contentToRender}</p>
          ) : (
            <ReactMarkdown
              remarkPlugins={[remarkGfm]}
              components={{
                a: safeLinkRenderer,
              }}
            >
              {contentToRender}
            </ReactMarkdown>
          )}
        </div>

        {!isUser && (
          <footer className="message-actions">
            <button
              className="action-copy-btn"
              onClick={handleCopy}
              title="Copy message text"
              aria-label="Copy message text"
            >
              {copied ? <Check size={13} className="text-amber" /> : <Copy size={13} />}
              <span>{copied ? 'Copied' : 'Copy'}</span>
            </button>
          </footer>
        )}
      </div>
    </article>
  );
};
