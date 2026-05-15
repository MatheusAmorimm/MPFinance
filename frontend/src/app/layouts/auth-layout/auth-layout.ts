import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from '../../shared/components/nav-bar/nav-bar';
import { TutorialComponent } from '../../shared/components/tutorial/tutorial';
import { GoalCelebrationComponent } from '../../shared/components/goal-celebration/goal-celebration';

@Component({
    selector: 'app-auth-layout',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [RouterOutlet, NavBar, TutorialComponent, GoalCelebrationComponent],
    template: `
        <app-nav-bar />
        <main class="page-content">
            <router-outlet />
        </main>
        <app-tutorial />
        <app-goal-celebration />
    `,
    styles: [`
        .page-content {
            padding-top: 64px;
            min-height: 100vh;
        }
    `],
})
export class AuthLayout {}
