import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorService, ErrorMessage } from '../../services/error.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-error-notifications',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="error-container" *ngIf="errors.length > 0">
      <div 
        *ngFor="let error of errors" 
        class="error-message"
        [ngClass]="'error-' + error.type"
        (click)="removeError(error.id)">
        <div class="error-content">
          <span class="error-icon">{{ getIcon(error.type) }}</span>
          <span class="error-text">{{ error.message }}</span>
        </div>
        <button class="error-close" (click)="removeError(error.id)">×</button>
      </div>
    </div>
  `,
  styles: [`
    .error-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      max-width: 400px;
    }

    .error-message {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      margin-bottom: 8px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      cursor: pointer;
      transition: all 0.3s ease;
      animation: slideIn 0.3s ease;
    }

    .error-message:hover {
      transform: translateX(-5px);
    }

    .error-error {
      background: linear-gradient(135deg, #ff6b6b, #ee5a52);
      color: white;
    }

    .error-warning {
      background: linear-gradient(135deg, #ffa726, #ff9800);
      color: white;
    }

    .error-info {
      background: linear-gradient(135deg, #42a5f5, #2196f3);
      color: white;
    }

    .error-success {
      background: linear-gradient(135deg, #66bb6a, #4caf50);
      color: white;
    }

    .error-content {
      display: flex;
      align-items: center;
      flex: 1;
    }

    .error-icon {
      margin-right: 8px;
      font-size: 16px;
    }

    .error-text {
      flex: 1;
      font-size: 14px;
      font-weight: 500;
    }

    .error-close {
      background: none;
      border: none;
      color: inherit;
      font-size: 18px;
      font-weight: bold;
      cursor: pointer;
      padding: 0;
      margin-left: 8px;
      opacity: 0.7;
      transition: opacity 0.2s ease;
    }

    .error-close:hover {
      opacity: 1;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @media (max-width: 768px) {
      .error-container {
        top: 10px;
        right: 10px;
        left: 10px;
        max-width: none;
      }
    }
  `]
})
export class ErrorNotificationsComponent implements OnInit, OnDestroy {
  errors: ErrorMessage[] = [];
  private subscription: Subscription = new Subscription();

  constructor(private errorService: ErrorService) {}

  ngOnInit(): void {
    this.subscription = this.errorService.errors$.subscribe(errors => {
      this.errors = errors;
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  removeError(errorId: string): void {
    this.errorService.removeError(errorId);
  }

  getIcon(type: string): string {
    switch (type) {
      case 'error': return '⚠️';
      case 'warning': return '⚠️';
      case 'info': return 'ℹ️';
      case 'success': return '✅';
      default: return 'ℹ️';
    }
  }
}
