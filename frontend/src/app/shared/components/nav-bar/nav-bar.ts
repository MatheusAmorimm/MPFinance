import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';

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

    logout(): void {
        this.authService.logout();
        this.router.navigate(['/login']);
    }
}
