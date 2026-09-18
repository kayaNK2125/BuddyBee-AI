import React from 'react';
import { Sparkles, Terminal, Compass } from 'lucide-react';
import './EmptyState.css';

interface EmptyStateProps {
  onSelectPrompt: (prompt: string) => void;
}

const SAMPLE_PROMPTS = [
  {
    icon: Terminal,
    title: 'Code & Architecture',
    text: 'Analyze the trade-offs between monolithic and microservice architectures for high-concurrency systems.',
  },
  {
    icon: Sparkles,
    title: 'Algorithmic Problem Solving',
    text: 'Explain how to design an efficient rate-limiter algorithm with sliding window log.',
  },
  {
    icon: Compass,
    title: 'Strategic Decision Making',
    text: 'Help me evaluate the risks and technical feasibility of migrating an on-prem database to a distributed cluster.',
  },
];

export const EmptyState: React.FC<EmptyStateProps> = ({ onSelectPrompt }) => {
  return (
    <div className="empty-state">
      <div className="empty-hero">
        <h1 className="empty-title">
          How can BuddyBee <span className="title-highlight">assist you</span> today?
        </h1>
        <p className="empty-subtitle">
          An intentional conversational partner for engineering, deep problem solving, and rigorous reasoning.
        </p>
      </div>

      <div className="prompt-grid" role="region" aria-label="Suggested Prompts">
        {SAMPLE_PROMPTS.map((item, idx) => {
          const Icon = item.icon;
          return (
            <button
              key={idx}
              className="prompt-card"
              onClick={() => onSelectPrompt(item.text)}
              aria-label={`Prompt: ${item.title}`}
            >
              <div className="prompt-icon-wrapper">
                <Icon size={16} />
              </div>
              <div className="prompt-content">
                <span className="prompt-title">{item.title}</span>
                <span className="prompt-text">{item.text}</span>
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
};
