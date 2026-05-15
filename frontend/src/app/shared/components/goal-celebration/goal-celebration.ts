import {
    ChangeDetectionStrategy,
    Component,
    computed,
    effect,
    ElementRef,
    inject,
    OnDestroy,
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { GoalCelebrationService } from '../../../core/services/goal-celebration.service';
import lottie, { AnimationItem } from 'lottie-web';

@Component({
    selector: 'app-goal-celebration',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [CurrencyPipe],
    templateUrl: './goal-celebration.html',
    styleUrl:    './goal-celebration.scss',
})
export class GoalCelebrationComponent implements OnDestroy {
    private readonly svc = inject(GoalCelebrationService);
    private readonly el  = inject(ElementRef<HTMLElement>);

    readonly goal    = computed(() => this.svc.celebratingGoal());
    readonly visible = computed(() => !!this.svc.celebratingGoal());

    private anim?: AnimationItem;

    constructor() {
        effect(() => {
            if (this.visible()) {
                setTimeout(() => this.initLottie(), 50);
                this.playVictorySound();
            } else {
                this.destroyAnim();
            }
        });
    }

    ngOnDestroy(): void {
        this.destroyAnim();
    }

    dismiss(): void {
        this.svc.dismiss();
    }

    private initLottie(): void {
        const container = (this.el.nativeElement as HTMLElement)
            .querySelector<HTMLElement>('.lottie-player');

        if (!container) {
            console.warn('[GoalCelebration] container .lottie-player não encontrado');
            return;
        }

        this.destroyAnim();

        this.anim = lottie.loadAnimation({
            container,
            renderer: 'svg',
            loop: false,
            autoplay: true,
            path: '/done.json',
        });

        this.anim.addEventListener('error', (e) => {
            console.error('[GoalCelebration] Erro ao carregar animação Lottie:', e);
        });
    }

    private destroyAnim(): void {
        this.anim?.destroy();
        this.anim = undefined;
    }

    private playVictorySound(): void {
        try {
            const w = window as unknown as Record<string, unknown>;
            const AudioCtx = (w['AudioContext'] ?? w['webkitAudioContext']) as typeof AudioContext | undefined;
            if (!AudioCtx) return;
            const ctx = new AudioCtx();
            const t   = ctx.currentTime;

            const notes = [
                { freq: 523.25, start: 0,    dur: 0.45 },
                { freq: 659.25, start: 0.18, dur: 0.45 },
                { freq: 783.99, start: 0.36, dur: 0.45 },
                { freq: 1046.5, start: 0.54, dur: 0.9  },
            ];

            for (const n of notes) {
                const osc  = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.value = n.freq;
                gain.gain.setValueAtTime(0, t + n.start);
                gain.gain.linearRampToValueAtTime(0.22, t + n.start + 0.05);
                gain.gain.exponentialRampToValueAtTime(0.001, t + n.start + n.dur);
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.start(t + n.start);
                osc.stop(t + n.start + n.dur + 0.1);
            }
        } catch { /* ignore audio errors */ }
    }
}
