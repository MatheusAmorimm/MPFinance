import { Routes } from '@angular/router';
import { Login } from './features/login/login';
import { Register } from './features/register/register';
import { authGuard } from './core/guards/auth';
import { Home } from './features/home/home';

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    { path: 'home', component: Home, canActivate: [authGuard] },
    {
        path: 'home',
        component: Home,
        canActivate: [authGuard]
    }
];