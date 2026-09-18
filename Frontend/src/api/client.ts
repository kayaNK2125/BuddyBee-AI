import { ENDPOINTS } from './endpoints';
import type { ChatRequestDto, ChatResponseDto, PingResponseDto } from './types';
import type { AppError } from '../types';

const API_BASE = (import.meta.env.VITE_API_URL || 'http://localhost:5168').replace(/\/+$/, '');

class ApiClient {
  private base: string;

  constructor(baseUrl: string) {
    this.base = baseUrl;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {},
    timeoutMs = 50000
  ): Promise<T> {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

    try {
      const response = await fetch(`${this.base}${endpoint}`, {
        ...options,
        signal: controller.signal,
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
          ...(options.headers || {}),
        },
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        let errorText = '';
        try {
          errorText = await response.text();
        } catch {
          errorText = response.statusText;
        }

        const appError: AppError = {
          message: `API error (${response.status}): ${response.statusText}`,
          details: errorText,
          status: response.status,
          retryable: response.status >= 500 || response.status === 429,
        };
        throw appError;
      }

      return (await response.json()) as T;
    } catch (err: unknown) {
      clearTimeout(timeoutId);

      if (err && typeof err === 'object' && 'message' in err && (err as { name?: string }).name === 'AbortError') {
        const timeoutError: AppError = {
          message: 'Request timed out while waiting for AI generation.',
          details: 'The backend did not respond within 50 seconds.',
          retryable: true,
        };
        throw timeoutError;
      }

      if (err && typeof err === 'object' && 'status' in err) {
        throw err as AppError;
      }

      const networkError: AppError = {
        message: 'Could not connect to BuddyBee backend.',
        details: 'Check if BuddyBee.Api is running on ' + this.base,
        retryable: true,
      };
      throw networkError;
    }
  }

  public async ping(): Promise<PingResponseDto> {
    return this.request<PingResponseDto>(ENDPOINTS.ping, { method: 'GET' }, 8000);
  }

  public async postChat(request: ChatRequestDto): Promise<ChatResponseDto> {
    return this.request<ChatResponseDto>(ENDPOINTS.chat, {
      method: 'POST',
      body: JSON.stringify(request),
    });
  }
}

export const api = new ApiClient(API_BASE);
