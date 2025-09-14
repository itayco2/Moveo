import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {
  signupForm: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.signupForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(4)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6), this.passwordValidator]]
    });
  }

  onSubmit(): void {
    if (this.signupForm.valid) {
      this.errorMessage = '';
      this.isLoading = true;
      
      this.authService.signup(this.signupForm.value).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.router.navigate(['/onboarding']);
        },
        error: (error) => {
          this.isLoading = false;
          if (error.status === 400) {
            // Check if it's an email already exists error
            if (error.error?.message?.includes('already exists') || 
                error.error?.message?.includes('Email is already registered')) {
              this.errorMessage = 'This email is already registered. Please use a different email or try logging in.';
            } else {
              this.errorMessage = 'Please check your information and try again.';
            }
          } else if (error.status === 500) {
            this.errorMessage = 'Server error. Please try again later.';
          } else {
            this.errorMessage = 'Signup failed. Please try again.';
          }
        }
      });
    }
  }

  private passwordValidator(control: any) {
    const value = control.value;
    if (!value) return null;
    
    const hasUppercase = /[A-Z]/.test(value);
    const hasLowercase = /[a-z]/.test(value);
    const hasNumber = /[0-9]/.test(value);
    
    if (!hasUppercase) {
      return { noUppercase: true };
    }
    if (!hasLowercase) {
      return { noLowercase: true };
    }
    if (!hasNumber) {
      return { noNumber: true };
    }
    
    return null;
  }

}
