import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-nav-bar',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [RouterLink, RouterLinkActive],
    templateUrl: './nav-bar.html',
    styleUrl: './nav-bar.scss',
})
export class NavBar {
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    menuOpen = signal(false);

    constructor() {
        this.router.events.pipe(
            filter(e => e instanceof NavigationEnd),
            takeUntilDestroyed()
        ).subscribe(() => this.menuOpen.set(false));
    }

    toggleMenu(): void {
        this.menuOpen.update(v => !v);
    }

    logout(): void {
        this.authService.logout();
        this.router.navigate(['/login']);
    }
}
