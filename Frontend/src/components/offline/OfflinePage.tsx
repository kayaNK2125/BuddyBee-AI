import React from 'react';
import './OfflinePage.css';

export const OfflinePage: React.FC = () => (
  <div className="offline-page">
    <div className="offline-bg-glow" aria-hidden="true" />

    <main className="offline-content">
      {/* Brand */}
      <div className="offline-brand">
        <svg className="offline-hexagon" viewBox="0 0 32 32" fill="none" aria-label="BuddyBee logo">
          <polygon points="16,2 29,9.5 29,24.5 16,32 3,24.5 3,9.5" fill="#141821" stroke="#F59E0B" strokeWidth="2" />
          <circle cx="16" cy="16" r="3.5" fill="#FDE68A" />
        </svg>
        <span className="offline-brand-name">BuddyBee</span>
      </div>

      {/* Status */}
      <div className="offline-status-badge" role="status">
        <span className="offline-status-dot" aria-hidden="true" />
        Demo temporarily offline
      </div>

      {/* Heading */}
      <h1 className="offline-heading">We're building beyond chat.</h1>

      {/* Supporting text */}
      <p className="offline-subtext">
        BuddyBee's hosted demo is temporarily unavailable. The open-source project remains available on GitHub, and you can self-host it using the published code.
      </p>

      {/* GitHub CTA */}
      <div className="offline-actions">
        <a
          id="offline-github-link"
          className="offline-btn-primary"
          href="https://github.com/kayaNK2125/BuddyBee-AI"
          target="_blank"
          rel="noopener noreferrer"
        >
          <svg className="offline-btn-icon" width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
            <path d="M12 2C6.477 2 2 6.477 2 12c0 4.418 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.009-.868-.013-1.703-2.782.604-3.369-1.342-3.369-1.342-.454-1.154-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.578 9.578 0 0 1 12 6.836a9.59 9.59 0 0 1 2.504.337c1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.203 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.163 22 16.418 22 12c0-5.523-4.477-10-10-10z" />
          </svg>
          <span className="offline-btn-label">Explore on GitHub</span>
        </a>
      </div>

      {/* Roadmap */}
      <section className="offline-roadmap" aria-label="What we're working toward">
        <h2 className="offline-roadmap-title">What we're working toward</h2>
        <ul className="offline-roadmap-list">
          <li className="offline-roadmap-item">
            <div className="offline-roadmap-icon" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                <polyline points="14 2 14 8 20 8" />
              </svg>
            </div>
            <div className="offline-roadmap-item-text">
              <span className="offline-roadmap-item-title">Document intelligence</span>
              <span className="offline-roadmap-item-desc">Upload and reason over PDFs, notes, and structured data.</span>
            </div>
          </li>
          <li className="offline-roadmap-item">
            <div className="offline-roadmap-icon" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
              </svg>
            </div>
            <div className="offline-roadmap-item-text">
              <span className="offline-roadmap-item-title">Grounded web research</span>
              <span className="offline-roadmap-item-desc">Improving source coverage, citations, and research quality.</span>
            </div>
          </li>
          <li className="offline-roadmap-item">
            <div className="offline-roadmap-icon" aria-hidden="true">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" />
              </svg>
            </div>
            <div className="offline-roadmap-item-text">
              <span className="offline-roadmap-item-title">More useful AI workflows</span>
              <span className="offline-roadmap-item-desc">Composable tools that turn conversations into real actions.</span>
            </div>
          </li>
        </ul>
      </section>

      {/* Footer */}
      <footer className="offline-footer">
        <span className="offline-footer-text">Open source · Self-hostable · Built in public</span>
      </footer>
    </main>

    <span className="offline-version" aria-hidden="true">v1.2.0</span>
  </div>
);
