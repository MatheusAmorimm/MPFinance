import { Injectable, signal, computed } from '@angular/core';

export interface TutorialStep {
    target: string | null;
    title: string;
    body: string;
    position?: 'top' | 'bottom' | 'left' | 'right';
}

const TUTORIALS: Record<string, TutorialStep[]> = {
    home: [
        {
            target: null,
            title: 'Bem-vindo ao MPFinance!',
            body: 'Seu painel financeiro pessoal. Vamos te mostrar como tudo funciona em poucos passos.',
        },
        {
            target: '.summary-cards',
            title: 'Resumo do mês',
            body: 'Veja suas receitas totais, despesas e saldo atual de forma rápida.',
            position: 'bottom',
        },
        {
            target: '.bill-list',
            title: 'Contas a pagar',
            body: 'Suas despesas fixas que vencem este mês aparecem aqui para você não esquecer.',
            position: 'top',
        },
        {
            target: '.tx-list',
            title: 'Lançamentos recentes',
            body: 'As últimas movimentações do mês ficam listadas aqui.',
            position: 'top',
        },
        {
            target: 'a[href="/transactions"]',
            title: 'Registrar lançamentos',
            body: 'Clique em "Lançamentos" no menu para registrar receitas, despesas e transferências para metas.',
            position: 'bottom',
        },
        {
            target: 'a[href="/goals"]',
            title: 'Criar metas',
            body: 'Clique em "Metas" para criar objetivos financeiros, acompanhar seu progresso e saber quanto poupar por mês.',
            position: 'bottom',
        },
        {
            target: 'a[href="/profile"]',
            title: 'Meu Perfil',
            body: 'Em "Meu Perfil" você altera sua senha e e-mail com verificação por código de segurança.',
            position: 'bottom',
        },
    ],
    transactions: [
        {
            target: null,
            title: 'Lançamentos',
            body: 'Aqui você registra e acompanha todas as suas receitas e despesas.',
        },
        {
            target: '.form-card',
            title: 'Novo lançamento',
            body: 'Preencha este formulário para registrar uma nova entrada ou saída de dinheiro.',
            position: 'bottom',
        },
        {
            target: '.table-nav',
            title: 'Filtro por mês',
            body: 'Navegue entre os meses para ver o histórico dos seus lançamentos.',
            position: 'bottom',
        },
    ],
    goals: [
        {
            target: null,
            title: 'Metas financeiras',
            body: 'Defina objetivos, acompanhe o progresso e saiba quanto poupar por mês para chegar lá.',
        },
        {
            target: '.goals-page__header',
            title: 'Nova meta',
            body: 'Crie uma meta com valor alvo, prazo e foto de capa. Você pode definir quantas quiser.',
            position: 'bottom',
        },
        {
            target: '.goal-card',
            title: 'Cartão de meta',
            body: 'Cada card mostra o progresso, o prazo e quanto você precisa poupar por mês para atingir o objetivo.',
            position: 'bottom',
        },
    ],
    profile: [
        {
            target: null,
            title: 'Meu Perfil',
            body: 'Gerencie suas informações de conta com segurança.',
        },
        {
            target: '.info-card',
            title: 'Seus dados',
            body: 'Aqui ficam seu nome e e-mail cadastrado.',
            position: 'bottom',
        },
        {
            target: '.security-card:first-of-type',
            title: 'Alterar senha',
            body: 'Troque sua senha com um código de verificação enviado ao seu e-mail.',
            position: 'bottom',
        },
        {
            target: '.security-card:last-of-type',
            title: 'Alterar e-mail',
            body: 'Você pode trocar seu e-mail a cada 15 dias, com dupla verificação de segurança.',
            position: 'top',
        },
    ],
};

@Injectable({ providedIn: 'root' })
export class TutorialService {
    private readonly STORAGE_KEY = 'mpfinance_tutorial_seen';

    private _steps = signal<TutorialStep[]>([]);
    private _index  = signal(0);
    private _screen = signal('');

    readonly isActive   = signal(false);
    readonly stepIndex  = computed(() => this._index());
    readonly totalSteps = computed(() => this._steps().length);
    readonly isFirst    = computed(() => this._index() === 0);
    readonly isLast     = computed(() => this._index() === this._steps().length - 1);
    readonly currentStep = computed<TutorialStep | null>(() =>
        this.isActive() ? (this._steps()[this._index()] ?? null) : null
    );

    startFor(screen: string): void {
        if (this.hasSeen(screen)) return;
        const steps = TUTORIALS[screen];
        if (!steps?.length) return;

        this._screen.set(screen);
        this._steps.set(steps);
        this._index.set(0);
        this.isActive.set(true);
    }

    next(): void {
        if (this.isLast()) {
            this.finish();
        } else {
            this._index.update(i => i + 1);
        }
    }

    prev(): void {
        this._index.update(i => Math.max(0, i - 1));
    }

    skip(): void {
        this.finish();
    }

    private finish(): void {
        this.markSeen(this._screen());
        this.isActive.set(false);
    }

    private hasSeen(screen: string): boolean {
        try {
            const seen: string[] = JSON.parse(localStorage.getItem(this.STORAGE_KEY) ?? '[]');
            return seen.includes(screen);
        } catch {
            return false;
        }
    }

    private markSeen(screen: string): void {
        try {
            const seen: string[] = JSON.parse(localStorage.getItem(this.STORAGE_KEY) ?? '[]');
            if (!seen.includes(screen)) {
                seen.push(screen);
                localStorage.setItem(this.STORAGE_KEY, JSON.stringify(seen));
            }
        } catch { /* ignore */ }
    }
}
