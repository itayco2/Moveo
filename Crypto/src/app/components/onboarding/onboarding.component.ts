import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OnboardingService } from '../../services/onboarding.service';
import { AuthService } from '../../services/auth.service';
import { AVAILABLE_CRYPTOS } from '../../constants/crypto-list';
import { INVESTOR_TYPES } from '../../constants/investor-types';
import { CONTENT_TYPES } from '../../constants/content-types';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './onboarding.component.html',
  styleUrl: './onboarding.component.css'
})
export class OnboardingComponent {
  onboardingForm: FormGroup;
  selectedCryptos: string[] = [];
  selectedContentTypes: string[] = [];

  availableCryptos = AVAILABLE_CRYPTOS;
  investorTypes = INVESTOR_TYPES;
  contentTypes = CONTENT_TYPES;

  constructor(
    private fb: FormBuilder,
    private onboardingService: OnboardingService,
    private authService: AuthService,
    private router: Router
  ) {
    this.onboardingForm = this.fb.group({
      investorType: ['', [Validators.required]]
    });
  }

  onCryptoChange(event: any): void {
    const value = event.target.value;
    if (event.target.checked) {
      // Store display name instead of ID
      const crypto = this.availableCryptos.find(c => c.id === value);
      this.selectedCryptos.push(crypto?.name || value);
    } else {
      const crypto = this.availableCryptos.find(c => c.id === value);
      const displayName = crypto?.name || value;
      const index = this.selectedCryptos.indexOf(displayName);
      if (index > -1) {
        this.selectedCryptos.splice(index, 1);
      }
    }
  }

  onContentTypeChange(event: any): void {
    const value = event.target.value;
    if (event.target.checked) {
      this.selectedContentTypes.push(value);
    } else {
      const index = this.selectedContentTypes.indexOf(value);
      if (index > -1) {
        this.selectedContentTypes.splice(index, 1);
      }
    }
  }

  isFormValid(): boolean {
    return (
      this.onboardingForm.valid &&
      this.selectedCryptos.length > 0 &&
      this.selectedContentTypes.length > 0
    );
  }

  onSubmit(): void {
    if (this.isFormValid()) {
      const preferences = {
        interestedCryptos: this.selectedCryptos,
        investorType: this.onboardingForm.value.investorType,
        preferredContentTypes: this.selectedContentTypes
      };

      this.onboardingService.completeOnboarding(preferences).subscribe({
        next: async (response) => {
          // Refresh user data and wait for completion before navigating
          try {
            await this.authService.refreshCurrentUser();
            this.router.navigate(['/dashboard']);
          } catch (error) {
            // Error handling is done by the global error system
          }
        }
      });
    }
  }
}