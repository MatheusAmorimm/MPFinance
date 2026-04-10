import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth';

/** Protege rotas autenticadas — redireciona para /login se não tiver token. */
export const authGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.getToken()) return true;

    router.navigate(['/login']);
    return false;
};

/** Protege rotas públicas — redireciona para /home se já estiver logado. */
export const guestGuard: CanActivateFn = () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.getToken()) {
        router.navigate(['/home']);
        return false;
    }

    return true;
};
