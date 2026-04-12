import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly TOKEN_KEY = 'mpfinance_token';

    constructor(private http: HttpClient) { }

    // ESTE É O MÉTODO QUE ESTÁ FALTANDO:
    register(userData: any): Observable<any> {
        // Usamos /api por causa do Proxy que configuramos no angular.json
        return this.http.post<any>('/api/auth/register', userData);
    }

    verifyEmail(email: string, code: string): Observable<{ message: string; token: string }> {
        return this.http.post<{ message: string; token: string }>('/api/auth/verify-email', { email, code }).pipe(
            tap(response => {
                if (response.token) {
                    localStorage.setItem(this.TOKEN_KEY, response.token);
                }
            })
        );
    }

    resendVerification(email: string): Observable<{ message: string }> {
        return this.http.post<{ message: string }>('/api/auth/resend-verification', { email });
    }

    login(credentials: any, rememberMe: boolean): Observable<any> {
        return this.http.post<any>('/api/auth/login', credentials).pipe(
            tap(response => {
                if (response.token) {
                    const storage = rememberMe ? localStorage : sessionStorage;
                    storage.setItem(this.TOKEN_KEY, response.token);
                }
            })
        );
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
    }

    logout() {
        localStorage.removeItem(this.TOKEN_KEY);
        sessionStorage.removeItem(this.TOKEN_KEY);
    }
}