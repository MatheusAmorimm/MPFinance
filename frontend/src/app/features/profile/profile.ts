import {
    ChangeDetectionStrategy,
    Component,
    computed,
    inject,
    OnInit,
    signal,
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ProfileService, UserProfile } from '../../core/services/profile.service';
import { AuthService } from '../../core/services/auth';
import { Router } from '@angular/router';
import { TutorialService } from '../../core/services/tutorial.service';

type PasswordStep = 'idle' | 'sending' | 'otp' | 'new-password' | 'done';
type EmailStep    = 'idle' | 'sending' | 'otp-current' | 'new-email' | 'sending-new' | 'otp-new' | 'done';

@Component({
    selector: 'app-profile',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [ReactiveFormsModule, DatePipe],
    templateUrl: './profile.html',
    styleUrl: './profile.scss',
})
export class Profile implements OnInit {
    private profileService  = inject(ProfileService);
    private authService     = inject(AuthService);
    private router          = inject(Router);
    private fb              = inject(FormBuilder);
    private tutorialService = inject(TutorialService);

    profile   = signal<UserProfile | null>(null);
    loading   = signal(true);
    error     = signal<string | null>(null);
    submitting = signal(false);

    // ─── Password change state ────────────────────────────────────────────────
    passwordStep = signal<PasswordStep>('idle');
    passwordOtp  = signal('');
    passwordOtpError = signal<string | null>(null);

    passwordForm = this.fb.group({
        newPassword:     ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', Validators.required],
    }, { validators: this.passwordsMatch });

    // ─── Email change state ───────────────────────────────────────────────────
    emailStep     = signal<EmailStep>('idle');
    emailOtp      = signal('');
    emailOtpError = signal<string | null>(null);
    emailDaysLeft = signal<number | null>(null);

    emailForm = this.fb.group({
        newEmail:     ['', [Validators.required, Validators.email]],
        confirmEmail: ['', [Validators.required, Validators.email]],
    });

    emailNewOtp      = signal('');
    emailNewOtpError = signal<string | null>(null);

    // ─── Computed ─────────────────────────────────────────────────────────────
    initials = computed(() => {
        const name = this.profile()?.name ?? '';
        return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase();
    });

    canChangeEmail = computed(() => this.emailDaysLeft() === null || this.emailDaysLeft() === 0);

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    ngOnInit(): void {
        this.tutorialService.startFor('profile');
        this.profileService.getProfile().subscribe({
            next: p => { this.profile.set(p); this.loading.set(false); },
            error: () => { this.error.set('Erro ao carregar perfil.'); this.loading.set(false); },
        });
    }

    // ─── Password change ──────────────────────────────────────────────────────
    requestPasswordCode(): void {
        this.passwordStep.set('sending');
        this.profileService.requestPasswordChange().subscribe({
            next: () => this.passwordStep.set('otp'),
            error: () => { this.passwordStep.set('idle'); this.showError('Erro ao enviar código.'); },
        });
    }

    submitPasswordOtp(): void {
        if (this.passwordOtp().length !== 6) {
            this.passwordOtpError.set('Digite os 6 dígitos do código.');
            return;
        }
        this.passwordOtpError.set(null);
        this.passwordStep.set('new-password');
    }

    submitNewPassword(): void {
        if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
        this.submitting.set(true);
        const { newPassword } = this.passwordForm.getRawValue();
        this.profileService.confirmPasswordChange(this.passwordOtp(), newPassword!).subscribe({
            next: () => {
                this.submitting.set(false);
                this.passwordStep.set('done');
                this.passwordOtp.set('');
                this.passwordForm.reset();
            },
            error: (err) => {
                this.submitting.set(false);
                const msg = err?.error?.message ?? 'Código inválido ou expirado.';
                this.passwordOtpError.set(msg);
                this.passwordStep.set('otp');
            },
        });
    }

    cancelPasswordChange(): void {
        this.passwordStep.set('idle');
        this.passwordOtp.set('');
        this.passwordOtpError.set(null);
        this.passwordForm.reset();
    }

    // ─── Email change ─────────────────────────────────────────────────────────
    requestEmailCode(): void {
        this.emailStep.set('sending');
        this.profileService.requestEmailChange().subscribe({
            next: () => this.emailStep.set('otp-current'),
            error: (err) => {
                const daysLeft = err?.error?.daysLeft as number | undefined;
                if (daysLeft) {
                    this.emailDaysLeft.set(daysLeft);
                    this.emailStep.set('idle');
                    this.showError(err.error.message);
                } else {
                    this.emailStep.set('idle');
                    this.showError('Erro ao enviar código.');
                }
            },
        });
    }

    submitEmailCurrentOtp(): void {
        if (this.emailOtp().length !== 6) {
            this.emailOtpError.set('Digite os 6 dígitos do código.');
            return;
        }
        this.emailOtpError.set(null);
        this.emailStep.set('new-email');
    }

    submitNewEmailForm(): void {
        if (this.emailForm.invalid) { this.emailForm.markAllAsTouched(); return; }
        const { newEmail, confirmEmail } = this.emailForm.getRawValue();
        this.emailStep.set('sending-new');
        this.profileService.submitNewEmail(this.emailOtp(), newEmail!, confirmEmail!).subscribe({
            next: () => this.emailStep.set('otp-new'),
            error: (err) => {
                this.emailStep.set('new-email');
                this.showError(err?.error?.message ?? 'Erro ao enviar código.');
            },
        });
    }

    submitEmailNewOtp(): void {
        if (this.emailNewOtp().length !== 6) {
            this.emailNewOtpError.set('Digite os 6 dígitos do código.');
            return;
        }
        this.submitting.set(true);
        this.profileService.verifyNewEmail(this.emailNewOtp()).subscribe({
            next: (res) => {
                this.submitting.set(false);
                this.emailStep.set('done');
                this.profile.update(p => p ? { ...p, email: res.newEmail } : p);
                this.emailNewOtp.set('');
                this.emailForm.reset();
            },
            error: (err) => {
                this.submitting.set(false);
                this.emailNewOtpError.set(err?.error?.message ?? 'Código inválido ou expirado.');
            },
        });
    }

    cancelEmailChange(): void {
        this.emailStep.set('idle');
        this.emailOtp.set('');
        this.emailNewOtp.set('');
        this.emailOtpError.set(null);
        this.emailNewOtpError.set(null);
        this.emailForm.reset();
    }

    // ─── Logout ───────────────────────────────────────────────────────────────
    logout(): void {
        this.authService.logout();
        this.router.navigate(['/login']);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private showError(msg: string): void {
        this.error.set(msg);
        setTimeout(() => this.error.set(null), 4000);
    }

    private passwordsMatch(group: AbstractControl): ValidationErrors | null {
        const pw  = group.get('newPassword')?.value;
        const cpw = group.get('confirmPassword')?.value;
        return pw && cpw && pw !== cpw ? { mismatch: true } : null;
    }
}
