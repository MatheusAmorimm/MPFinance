import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // Se houver token em QUALQUER um dos storages, permite a entrada
    if (authService.getToken()) {
        return true;
    }

    // Caso contrário, manda de volta para o login
    router.navigate(['/login']);
    return false;
};