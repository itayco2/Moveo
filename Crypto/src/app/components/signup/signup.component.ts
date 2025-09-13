import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ErrorService } from '../../services/error.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {
  signupForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private errorService: ErrorService
  ) {
    this.signupForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(4)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.signupForm.valid) {
      this.authService.signup(this.signupForm.value).subscribe({
        next: (response) => {
          this.errorService.addError('Account created successfully!', 'success');
          this.router.navigate(['/onboarding']);
        },
        error: (error) => {
          // Error is already handled by the auth service
        }
      });
    }
  }

}
