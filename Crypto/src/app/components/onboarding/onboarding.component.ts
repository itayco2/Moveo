import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OnboardingService } from '../../services/onboarding.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './onboarding.component.html',
  styleUrl: './onboarding.component.css'
})
export class OnboardingComponent {
  onboardingForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  selectedCryptos: string[] = [];
  selectedContentTypes: string[] = [];

  availableCryptos = [
    { id: 'bitcoin', name: 'Bitcoin', symbol: 'BTC' },
    { id: 'ethereum', name: 'Ethereum', symbol: 'ETH' },
    { id: 'cardano', name: 'Cardano', symbol: 'ADA' },
    { id: 'solana', name: 'Solana', symbol: 'SOL' },
    { id: 'binancecoin', name: 'BNB', symbol: 'BNB' },
    { id: 'polkadot', name: 'Polkadot', symbol: 'DOT' },
    { id: 'chainlink', name: 'Chainlink', symbol: 'LINK' },
    { id: 'polygon', name: 'Polygon', symbol: 'MATIC' }
  ];

  investorTypes = [
    {
      value: 'HODLer',
      name: 'HODLer',
      description: 'Long-term investor who believes in holding crypto assets'
    },
    {
      value: 'Day Trader',
      name: 'Day Trader',
      description: 'Active trader looking for short-term opportunities'
    },
    {
      value: 'NFT Collector',
      name: 'NFT Collector',
      description: 'Interested in digital art and collectibles'
    },
    {
      value: 'DeFi Enthusiast',
      name: 'DeFi Enthusiast',
      description: 'Focused on decentralized finance protocols'
    }
  ];

  contentTypes = [
    { value: 'Market News', name: 'Market News & Analysis' },
    { value: 'Charts', name: 'Price Charts & Data' },
    { value: 'Social', name: 'Social Sentiment' },
    { value: 'Fun', name: 'Memes & Community' }
  ];

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
      this.isLoading = true;
      this.errorMessage = '';

      const preferences = {
        interestedCryptos: this.selectedCryptos,
        investorType: this.onboardingForm.value.investorType,
        preferredContentTypes: this.selectedContentTypes
      };

      this.onboardingService.completeOnboarding(preferences).subscribe({
        next: async (response) => {
          this.isLoading = false;
          // Refresh user data and wait for completion before navigating
          try {
            await this.authService.refreshCurrentUser();
            this.router.navigate(['/dashboard']);
          } catch (error) {
            this.errorMessage = 'Failed to refresh user data. Please try again.';
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Setup failed. Please try again.';
        }
      });
    }
  }
}