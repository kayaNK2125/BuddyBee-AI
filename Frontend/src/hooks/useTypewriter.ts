import { useEffect, useState } from 'react';

/**
 * useTypewriter Hook
 * 
 * Progressive text presentation (client-side typewriter pacing).
 * Note: This is an intentional client-side visual reveal to pace the completed
 * backend text comfortably for reading; it is NOT backend token streaming.
 */
export function useTypewriter(
  fullText: string,
  enabled: boolean = true,
  charsPerTick: number = 3,
  tickMs: number = 18,
  onComplete?: () => void
) {
  const [displayedText, setDisplayedText] = useState(enabled ? '' : fullText);
  const [isTyping, setIsTyping] = useState(enabled && fullText.length > 0);

  useEffect(() => {
    if (!enabled || !fullText) {
      setDisplayedText(fullText);
      setIsTyping(false);
      onComplete?.();
      return;
    }

    let currentIndex = 0;
    setIsTyping(true);
    setDisplayedText('');

    const interval = setInterval(() => {
      currentIndex += charsPerTick;
      if (currentIndex >= fullText.length) {
        setDisplayedText(fullText);
        setIsTyping(false);
        clearInterval(interval);
        onComplete?.();
      } else {
        setDisplayedText(fullText.slice(0, currentIndex));
      }
    }, tickMs);

    return () => clearInterval(interval);
  }, [fullText, enabled, charsPerTick, tickMs]);

  return { displayedText, isTyping };
}
