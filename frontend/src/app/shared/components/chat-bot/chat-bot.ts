import {
    AfterViewChecked,
    ChangeDetectionStrategy,
    Component,
    ElementRef,
    ViewChild,
    inject,
    signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatMessageItem } from '../../../core/services/chat.service';

@Component({
    selector: 'app-chat-bot',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [FormsModule],
    templateUrl: './chat-bot.html',
    styleUrl: './chat-bot.scss',
})
export class ChatBotComponent implements AfterViewChecked {
    private readonly chatService = inject(ChatService);

    readonly open         = signal(false);
    readonly consentGiven = signal<boolean | null>(null);
    readonly shareData    = signal(false);
    readonly messages     = signal<ChatMessageItem[]>([]);
    readonly loading      = signal(false);
    readonly inputText    = signal('');

    private shouldScroll = false;

    @ViewChild('messageList') private messageList?: ElementRef<HTMLElement>;

    constructor() {
        const stored = localStorage.getItem('chat_consent');
        if (stored !== null) {
            this.consentGiven.set(true);
            this.shareData.set(stored === 'true');
            this.pushWelcome();
        }
    }

    ngAfterViewChecked(): void {
        if (this.shouldScroll) {
            this.scrollToBottom();
            this.shouldScroll = false;
        }
    }

    toggle(): void {
        this.open.update(v => !v);
    }

    giveConsent(share: boolean): void {
        this.shareData.set(share);
        this.consentGiven.set(true);
        localStorage.setItem('chat_consent', String(share));
        this.pushWelcome();
    }

    send(): void {
        const text = this.inputText().trim();
        if (!text || this.loading()) return;

        const history = this.messages();
        this.messages.update(m => [...m, { role: 'user', content: text }]);
        this.inputText.set('');
        this.loading.set(true);
        this.shouldScroll = true;

        const now = new Date();
        this.chatService.ask({
            message: text,
            shareFinancialData: this.shareData(),
            month: now.getMonth() + 1,
            year: now.getFullYear(),
            history,
        }).subscribe({
            next: res => {
                this.messages.update(m => [...m, { role: 'bot', content: res.reply }]);
                this.loading.set(false);
                this.shouldScroll = true;
            },
            error: (err) => {
                const msg = err?.error?.reply ?? 'Erro ao conectar. Tente novamente.';
                this.messages.update(m => [...m, { role: 'bot', content: msg }]);
                this.loading.set(false);
                this.shouldScroll = true;
            },
        });
    }

    onKeyDown(event: KeyboardEvent): void {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            this.send();
        }
    }

    private pushWelcome(): void {
        this.messages.set([{
            role: 'bot',
            content: 'Olá! Sou o FinBot 👋 Posso te ajudar com dúvidas sobre o MPFinance ou analisar seus gastos do mês. Como posso ajudar?',
        }]);
        this.shouldScroll = true;
    }

    private scrollToBottom(): void {
        const el = this.messageList?.nativeElement;
        if (el) el.scrollTop = el.scrollHeight;
    }
}
