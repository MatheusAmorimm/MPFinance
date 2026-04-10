import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { Register } from './features/register/register';
import { VerifyEmail } from './features/verify-email/verify-email';
import { authGuard, guestGuard } from './core/guards/auth';
import { Home } from './features/home/home';

export const routes: Routes = [
    { path: '',             redirectTo: 'login', pathMatch: 'full' },
    { path: 'login',        component: Login,        canActivate: [guestGuard] },
    { path: 'register',     component: Register,     canActivate: [guestGuard] },
    { path: 'verify-email', component: VerifyEmail,  canActivate: [guestGuard] },
    { path: 'home',         component: Home,         canActivate: [authGuard]  },
];
