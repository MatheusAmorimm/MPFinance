import {
    ChangeDetectionStrategy,
    Component,
    OnInit,
    computed,
    inject,
    signal,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HomeService } from '../../core/services/home.service';
import { HomeSummary } from '../../core/models/home-summary.model';
import { TutorialService } from '../../core/services/tutorial.service';

@Component({
    selector: 'app-home',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [CurrencyPipe, DatePipe],
    templateUrl: './home.html',
    styleUrl: './home.scss',
})
export class Home implements OnInit {
    private readonly homeService  = inject(HomeService);
    private readonly tutorialService = inject(TutorialService);

    readonly summary = signal<HomeSummary | null>(null);
    readonly loading = signal(true);
    readonly error = signal<string | null>(null);
    readonly currentMonth = signal('');

    readonly isPositiveBalance = computed(() => (this.summary()?.balance ?? 0) >= 0);

    ngOnInit(): void {
        this.tutorialService.startFor('home');
        const now = new Date();
        const label = now.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' });
        this.currentMonth.set(label.charAt(0).toUpperCase() + label.slice(1));
        this.loadSummary(now.getMonth() + 1, now.getFullYear());
    }

    private loadSummary(month: number, year: number): void {
        this.homeService.getSummary(month, year).subscribe({
            next: (data) => {
                this.summary.set(data);
                this.loading.set(false);
            },
            error: () => {
                this.error.set('Não foi possível carregar o resumo financeiro.');
                this.loading.set(false);
            },
        });
    }
}
