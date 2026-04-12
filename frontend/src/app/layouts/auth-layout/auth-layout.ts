import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from '../../shared/components/nav-bar/nav-bar';

@Component({
    selector: 'app-auth-layout',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [RouterOutlet, NavBar],
    template: `
        <app-nav-bar />
        <main class="page-content">
            <router-outlet />
        </main>
    `,
    styles: [`
        .page-content {
            padding-top: 64px;
            min-height: 100vh;
        }
    `],
})
export class AuthLayout {}
