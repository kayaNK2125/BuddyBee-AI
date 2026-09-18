export interface ChatRequestDto {
  conversationId: string;
  userId: string;
  message: string;
}

export interface ChatResponseDto {
  reply: string;
  provider: string; // e.g. "Gemini" | "OpenAI"
}

export interface PingResponseDto {
  message: string;
}
