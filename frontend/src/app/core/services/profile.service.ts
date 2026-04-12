import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserProfile {
    id: string;
    name: string;
    email: string;
    createdAt: string;
    emailChangedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
    private http = inject(HttpClient);

    getProfile(): Observable<UserProfile> {
        return this.http.get<UserProfile>('/api/profile');
    }

    requestPasswordChange(): Observable<{ message: string }> {
        return this.http.post<{ message: string }>('/api/profile/change-password/request', {});
    }

    confirmPasswordChange(code: string, newPassword: string): Observable<{ message: string }> {
        return this.http.post<{ message: string }>('/api/profile/change-password/confirm', { code, newPassword });
    }

    requestEmailChange(): Observable<{ message: string }> {
        return this.http.post<{ message: string }>('/api/profile/change-email/request', {});
    }

    submitNewEmail(code: string, newEmail: string, confirmEmail: string): Observable<{ message: string }> {
        return this.http.post<{ message: string }>('/api/profile/change-email/submit', { code, newEmail, confirmEmail });
    }

    verifyNewEmail(code: string): Observable<{ message: string; newEmail: string }> {
        return this.http.post<{ message: string; newEmail: string }>('/api/profile/change-email/verify-new', { code });
    }
}
