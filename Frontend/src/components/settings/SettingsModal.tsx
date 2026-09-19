import React, { useState, useEffect } from 'react';
import { X, Key, Shield, Sparkles, CheckCircle2, Trash2, Cpu, ExternalLink, Info } from 'lucide-react';
import {
  getProviderMode,
  setProviderMode,
  saveProviderKey,
  removeProviderKey,
  hasProviderKey,
  getMaskedKeyHint,
  type ProviderMode,
} from '../../utils/keyStorage';
import './SettingsModal.css';

interface SettingsModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const SettingsModal: React.FC<SettingsModalProps> = ({ isOpen, onClose }) => {
  const [mode, setMode] = useState<ProviderMode>(() => getProviderMode());
  const [geminiKeyInput, setGeminiKeyInput] = useState('');
  const [openAiKeyInput, setOpenAiKeyInput] = useState('');
  const [geminiConfigured, setGeminiConfigured] = useState<boolean>(() => hasProviderKey('gemini'));
  const [openAiConfigured, setOpenAiConfigured] = useState<boolean>(() => hasProviderKey('openai'));
  const [geminiHint, setGeminiHint] = useState<string | null>(() => getMaskedKeyHint('gemini'));
  const [openAiHint, setOpenAiHint] = useState<string | null>(() => getMaskedKeyHint('openai'));
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  // Sync keys and mode state whenever the modal is opened
  useEffect(() => {
    if (isOpen) {
      setMode(getProviderMode());
      setGeminiConfigured(hasProviderKey('gemini'));
      setOpenAiConfigured(hasProviderKey('openai'));
      setGeminiHint(getMaskedKeyHint('gemini'));
      setOpenAiHint(getMaskedKeyHint('openai'));
      setStatusMessage(null);
    }
  }, [isOpen]);

  // Handle ESC key to close
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const handleModeChange = (newMode: ProviderMode) => {
    setMode(newMode);
    setProviderMode(newMode);
    if (newMode === 'custom') {
      setGeminiConfigured(hasProviderKey('gemini'));
      setOpenAiConfigured(hasProviderKey('openai'));
      setGeminiHint(getMaskedKeyHint('gemini'));
      setOpenAiHint(getMaskedKeyHint('openai'));
    }
    setStatusMessage(
      newMode === 'managed'
        ? 'Switched to BuddyBee Managed capacity.'
        : 'Switched to custom API keys mode.'
    );
  };

  const handleSaveGemini = () => {
    if (!geminiKeyInput.trim()) return;
    saveProviderKey('gemini', geminiKeyInput.trim());
    setGeminiConfigured(true);
    setGeminiHint(getMaskedKeyHint('gemini'));
    setGeminiKeyInput('');
    if (mode !== 'custom') {
      setMode('custom');
      setProviderMode('custom');
    }
    setStatusMessage('Gemini API key saved for this session.');
  };

  const handleRemoveGemini = () => {
    removeProviderKey('gemini');
    setGeminiConfigured(false);
    setGeminiHint(null);
    setGeminiKeyInput('');
    setStatusMessage('Gemini API key removed.');
  };

  const handleSaveOpenAI = () => {
    if (!openAiKeyInput.trim()) return;
    saveProviderKey('openai', openAiKeyInput.trim());
    setOpenAiConfigured(true);
    setOpenAiHint(getMaskedKeyHint('openai'));
    setOpenAiKeyInput('');
    if (mode !== 'custom') {
      setMode('custom');
      setProviderMode('custom');
    }
    setStatusMessage('OpenAI API key saved for this session.');
  };

  const handleRemoveOpenAI = () => {
    removeProviderKey('openai');
    setOpenAiConfigured(false);
    setOpenAiHint(null);
    setOpenAiKeyInput('');
    setStatusMessage('OpenAI API key removed.');
  };

  return (
    <div className="settings-modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="settings-modal-card" onClick={(e) => e.stopPropagation()}>
        {/* Modal Header */}
        <div className="settings-header">
          <div className="settings-title-group">
            <div className="settings-icon-wrapper">
              <Key size={18} />
            </div>
            <div>
              <h2 className="settings-title">AI Provider Settings</h2>
              <p className="settings-subtitle">Manage AI credentials and engine capacity</p>
            </div>
          </div>
          <button className="settings-close-btn" onClick={onClose} aria-label="Close settings">
            <X size={18} />
          </button>
        </div>

        {/* Feedback Alert */}
        {statusMessage && (
          <div className="settings-status-banner" role="status">
            <CheckCircle2 size={15} />
            <span>{statusMessage}</span>
          </div>
        )}

        {/* Provider Mode Selection */}
        <div className="settings-section">
          <span className="section-label">Capacity Mode</span>
          <div className="mode-cards-grid">
            {/* Mode 1: BuddyBee Managed */}
            <div
              className={`mode-card ${mode === 'managed' ? 'selected' : ''}`}
              onClick={() => handleModeChange('managed')}
              role="button"
              tabIndex={0}
            >
              <div className="mode-card-header">
                <div className="mode-radio-box">
                  <span className={`radio-dot ${mode === 'managed' ? 'active' : ''}`} />
                </div>
                <div className="mode-card-title-group">
                  <span className="mode-card-title">BuddyBee Managed</span>
                  <span className="badge-shared">Shared Capacity</span>
                </div>
              </div>
              <p className="mode-card-desc">
                Uses the deployment&apos;s shared provider capacity. Suitable for quick exploration,
                but may experience rate limits, temporary unavailability, or provider errors during peak demand.
              </p>
            </div>

            {/* Mode 2: Custom Key */}
            <div
              className={`mode-card ${mode === 'custom' ? 'selected' : ''}`}
              onClick={() => handleModeChange('custom')}
              role="button"
              tabIndex={0}
            >
              <div className="mode-card-header">
                <div className="mode-radio-box">
                  <span className={`radio-dot ${mode === 'custom' ? 'active' : ''}`} />
                </div>
                <div className="mode-card-title-group">
                  <span className="mode-card-title">Use your own API key</span>
                  <span className="badge-recommended">Recommended for stability</span>
                </div>
              </div>
              <p className="mode-card-desc">
                Connect your personal provider key(s) to use your dedicated quota for consistent
                availability, faster generation, and higher rate limits.
              </p>
            </div>
          </div>
        </div>

        {/* Provider Keys Configuration & Fallback Panel (Visible only when 'custom' mode is selected) */}
        {mode === 'custom' && (
          <>
            <div className="settings-section">
              <div className="section-header-row">
                <span className="section-label">API Keys (Session-Scoped)</span>
                <span className="section-hint">Active only when &quot;Use your own API key&quot; is selected</span>
              </div>

          <div className="provider-inputs-container">
            {/* Gemini Key Config */}
            <div className="provider-card">
              <div className="provider-card-top">
                <div className="provider-name-row">
                  <Sparkles size={16} className="provider-sparkle-gemini" />
                  <span className="provider-title">Google Gemini</span>
                </div>
                <div className="provider-status-badge">
                  {geminiConfigured ? (
                    <span className="status-configured">
                      <CheckCircle2 size={13} /> Configured {geminiHint}
                    </span>
                  ) : (
                    <span className="status-not-configured">Not configured</span>
                  )}
                </div>
              </div>

              <div className="provider-input-row">
                <input
                  type="password"
                  className="key-input"
                  placeholder={geminiConfigured ? 'Enter new key to update...' : 'Enter Gemini API key (AIzaSy...)'}
                  value={geminiKeyInput}
                  onChange={(e) => setGeminiKeyInput(e.target.value)}
                  autoComplete="off"
                  spellCheck="false"
                />
                <button
                  className="btn-save-key"
                  onClick={handleSaveGemini}
                  disabled={!geminiKeyInput.trim()}
                >
                  {geminiConfigured ? 'Update' : 'Save'}
                </button>
                {geminiConfigured && (
                  <button
                    className="btn-remove-key"
                    onClick={handleRemoveGemini}
                    title="Remove Gemini key"
                    aria-label="Remove Gemini key"
                  >
                    <Trash2 size={14} />
                  </button>
                )}
              </div>
              <div className="provider-help-row">
                <a
                  href="https://aistudio.google.com/app/apikey"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="provider-link"
                >
                  <span>Get a Gemini key from Google AI Studio</span>
                  <ExternalLink size={12} />
                </a>
              </div>
            </div>

            {/* OpenAI Key Config */}
            <div className="provider-card">
              <div className="provider-card-top">
                <div className="provider-name-row">
                  <Cpu size={16} className="provider-sparkle-openai" />
                  <span className="provider-title">OpenAI</span>
                </div>
                <div className="provider-status-badge">
                  {openAiConfigured ? (
                    <span className="status-configured">
                      <CheckCircle2 size={13} /> Configured {openAiHint}
                    </span>
                  ) : (
                    <span className="status-not-configured">Not configured</span>
                  )}
                </div>
              </div>

              <div className="provider-input-row">
                <input
                  type="password"
                  className="key-input"
                  placeholder={openAiConfigured ? 'Enter new key to update...' : 'Enter OpenAI API key (sk-...)'}
                  value={openAiKeyInput}
                  onChange={(e) => setOpenAiKeyInput(e.target.value)}
                  autoComplete="off"
                  spellCheck="false"
                />
                <button
                  className="btn-save-key"
                  onClick={handleSaveOpenAI}
                  disabled={!openAiKeyInput.trim()}
                >
                  {openAiConfigured ? 'Update' : 'Save'}
                </button>
                {openAiConfigured && (
                  <button
                    className="btn-remove-key"
                    onClick={handleRemoveOpenAI}
                    title="Remove OpenAI key"
                    aria-label="Remove OpenAI key"
                  >
                    <Trash2 size={14} />
                  </button>
                )}
              </div>
              <div className="provider-help-row">
                <a
                  href="https://platform.openai.com/api-keys"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="provider-link"
                >
                  <span>Get an OpenAI key from OpenAI Platform</span>
                  <ExternalLink size={12} />
                </a>
              </div>
            </div>
          </div>
        </div>

        {/* Fallback & Isolation Information */}
            <div className="settings-info-box">
              <Info size={16} className="info-icon" />
              <div className="info-text">
                <span className="info-title">Intelligent Isolation & Fallback</span>
                <p className="info-desc">
                  When using your own keys, BuddyBee only uses the providers you configure.
                  If you provide both Gemini and OpenAI, BuddyBee will automatically prioritize Gemini
                  and fall back to OpenAI only on temporary rate limits or quota exhaustion.
                  Your personal traffic never falls back to server keys.
                </p>
              </div>
            </div>
          </>
        )}

        {/* Privacy Note */}
        <div className="settings-footer">
          <Shield size={14} className="shield-icon" />
          <span className="footer-privacy-text">
            Session Protection: Keys are stored exclusively in browser session memory and sent via HTTPS.
            They are never persisted in databases, server logs, or shared across sessions.
          </span>
        </div>
      </div>
    </div>
  );
};
