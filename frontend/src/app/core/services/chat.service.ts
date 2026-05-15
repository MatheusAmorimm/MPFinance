import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatMessageItem {
    role: 'user' | 'bot';
    content: string;
}

export interface ChatRequest {
    message: string;
    shareFinancialData: boolean;
    month: number;
    year: number;
    history: ChatMessageItem[];
}

export interface ChatResponse {
    reply: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
    private readonly http = inject(HttpClient);

    ask(request: ChatRequest): Observable<ChatResponse> {
        return this.http.post<ChatResponse>('/api/chat', request);
    }
}
