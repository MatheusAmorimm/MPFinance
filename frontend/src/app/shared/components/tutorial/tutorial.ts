import {
    ChangeDetectionStrategy,
    Component,
    DOCUMENT,
    effect,
    inject,
    signal,
    computed,
} from '@angular/core';
import { TutorialService, TutorialStep } from '../../../core/services/tutorial.service';

interface Rect { top: number; left: number; width: number; height: number; }
interface Pos  { top: string; left: string; transform?: string; }

@Component({
    selector: 'app-tutorial',
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './tutorial.html',
    styleUrl:    './tutorial.scss',
})
export class TutorialComponent {
    readonly tutorial = inject(TutorialService);
    private  doc      = inject(DOCUMENT);

    readonly spotlightRect = signal<Rect | null>(null);
    readonly tooltipPos    = signal<Pos>({ top: '50%', left: '50%', transform: 'translate(-50%,-50%)' });
    readonly animKey       = signal(0); // changes on each step to re-trigger CSS animation

    readonly dotsArray = computed(() =>
        Array.from({ length: this.tutorial.totalSteps() }, (_, i) => i)
    );

    readonly spotlightStyle = computed(() => {
        const r = this.spotlightRect();
        if (!r) return { display: 'none' };
        return {
            top:    `${r.top}px`,
            left:   `${r.left}px`,
            width:  `${r.width}px`,
            height: `${r.height}px`,
        };
    });

    constructor() {
        effect(() => {
            const step = this.tutorial.currentStep();
            this.animKey.update(k => k + 1);

            if (!step) { this.spotlightRect.set(null); return; }

            requestAnimationFrame(() => this.updateLayout(step));
        });
    }

    private updateLayout(step: TutorialStep): void {
        if (!step.target) {
            this.spotlightRect.set(null);
            this.tooltipPos.set({ top: '50%', left: '50%', transform: 'translate(-50%,-50%)' });
            return;
        }

        const el = this.doc.querySelector(step.target);
        if (!el) {
            this.spotlightRect.set(null);
            this.tooltipPos.set({ top: '50%', left: '50%', transform: 'translate(-50%,-50%)' });
            return;
        }

        const raw = el.getBoundingClientRect();
        const pad = 8;
        const rect: Rect = {
            top:    raw.top    - pad,
            left:   raw.left   - pad,
            width:  raw.width  + pad * 2,
            height: raw.height + pad * 2,
        };
        this.spotlightRect.set(rect);
        this.tooltipPos.set(this.calcPos(raw, step.position));
    }

    private calcPos(rect: DOMRect, position?: string): Pos {
        const W   = 300;
        const H   = 200;
        const vw  = this.doc.defaultView?.innerWidth  ?? 800;
        const vh  = this.doc.defaultView?.innerHeight ?? 600;
        const pad = 12;
        const gap = 16;

        let top: number;
        let left: number;

        switch (position) {
            case 'top':
                top  = rect.top - H - gap;
                left = rect.left + rect.width / 2 - W / 2;
                break;
            case 'left':
                top  = rect.top + rect.height / 2 - H / 2;
                left = rect.left - W - gap;
                break;
            case 'right':
                top  = rect.top + rect.height / 2 - H / 2;
                left = rect.right + gap;
                break;
            default: // bottom
                top  = rect.bottom + gap;
                left = rect.left + rect.width / 2 - W / 2;
        }

        // Clamp within viewport
        left = Math.max(pad, Math.min(left, vw - W - pad));
        top  = Math.max(64 + pad, Math.min(top, vh - H - pad));

        return { top: `${top}px`, left: `${left}px` };
    }
}
