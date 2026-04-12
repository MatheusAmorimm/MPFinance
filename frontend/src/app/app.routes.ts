import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { Register } from './features/register/register';
import { VerifyEmail } from './features/verify-email/verify-email';
import { authGuard, guestGuard } from './core/guards/auth';
import { AuthLayout } from './layouts/auth-layout/auth-layout';
import { Home } from './features/home/home';
import { Transactions } from './features/transactions/transactions';
import { Goals } from './features/goals/goals';
import { Profile } from './features/profile/profile';

export const routes: Routes = [
    { path: '',             redirectTo: 'login', pathMatch: 'full' },
    { path: 'login',        component: Login,       canActivate: [guestGuard] },
    { path: 'register',     component: Register,    canActivate: [guestGuard] },
    { path: 'verify-email', component: VerifyEmail, canActivate: [guestGuard] },
    {
        path: '',
        component: AuthLayout,
        canActivate: [authGuard],
        children: [
            { path: 'home',         component: Home         },
            { path: 'transactions', component: Transactions },
            { path: 'goals',        component: Goals        },
            { path: 'profile',      component: Profile      },
        ],
    },
];
