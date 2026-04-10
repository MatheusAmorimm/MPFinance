import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html'
})
export class Login {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      rememberMe: [false] // Campo do checkbox
    });
  }

  onLogin() {
    if (this.loginForm.valid) {
      const { email, password, rememberMe } = this.loginForm.value;

      this.authService.login({ email, password }, rememberMe).subscribe({
        next: (res) => {
          console.log('Login realizado!');
          this.router.navigate(['/home']);
        },
        error: () => alert('E-mail ou senha inválidos.')
      });
    }
  }
}