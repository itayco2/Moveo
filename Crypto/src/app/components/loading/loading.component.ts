import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../services/loading.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="loading-overlay" *ngIf="isLoading">
      <div class="loading-spinner">
        <div class="spinner"></div>
        <p class="loading-text">Loading...</p>
      </div>
    </div>
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 10000;
      backdrop-filter: blur(2px);
    }

    .loading-spinner {
      display: flex;
      flex-direction: column;
      align-items: center;
      background: rgba(255, 255, 255, 0.1);
      padding: 2rem;
      border-radius: 12px;
      backdrop-filter: blur(10px);
      border: 1px solid rgba(255, 255, 255, 0.2);
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 4px solid rgba(255, 255, 255, 0.3);
      border-top: 4px solid #ffffff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin-bottom: 1rem;
    }

    .loading-text {
      color: #ffffff;
      font-size: 14px;
      font-weight: 500;
      margin: 0;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class LoadingComponent implements OnInit, OnDestroy {
  isLoading = false;
  private subscription: Subscription = new Subscription();

  constructor(private loadingService: LoadingService) {}

  ngOnInit(): void {
    this.subscription = this.loadingService.loading$.subscribe(loading => {
      this.isLoading = loading;
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
