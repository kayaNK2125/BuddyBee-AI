import { useEffect, useRef, useState } from 'react';

export function useAutoScroll(dependency: unknown) {
  const scrollRef = useRef<HTMLDivElement | null>(null);
  const [isAutoScrollLocked, setIsAutoScrollLocked] = useState(false);

  const handleScroll = () => {
    if (!scrollRef.current) return;
    const { scrollTop, scrollHeight, clientHeight } = scrollRef.current;
    // If user scrolled up by more than 80px, lock auto-scroll
    const isAtBottom = scrollHeight - (scrollTop + clientHeight) < 80;
    setIsAutoScrollLocked(!isAtBottom);
  };

  useEffect(() => {
    if (!isAutoScrollLocked && scrollRef.current) {
      scrollRef.current.scrollTo({
        top: scrollRef.current.scrollHeight,
        behavior: 'smooth',
      });
    }
  }, [dependency, isAutoScrollLocked]);

  const scrollToBottom = () => {
    if (scrollRef.current) {
      scrollRef.current.scrollTo({
        top: scrollRef.current.scrollHeight,
        behavior: 'smooth',
      });
      setIsAutoScrollLocked(false);
    }
  };

  return { scrollRef, handleScroll, isAutoScrollLocked, scrollToBottom };
}
