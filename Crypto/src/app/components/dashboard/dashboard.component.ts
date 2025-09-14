import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardService } from '../../services/dashboard.service';
import { AuthService } from '../../services/auth.service';
import { LoadingService } from '../../services/loading.service';
import { DashboardResponse } from '../../models/dashboard.models';
import { User } from '../../models/auth.models';
import { AiInsightComponent } from './ai-insight/ai-insight.component';
import { MarketNewsComponent } from './market-news/market-news.component';
import { CoinPricesComponent } from './coin-prices/coin-prices.component';
import { CryptoMemeComponent } from './crypto-meme/crypto-meme.component';
import { LoadingComponent } from '../loading/loading.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, AiInsightComponent, MarketNewsComponent, CoinPricesComponent, CryptoMemeComponent, LoadingComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {
  dashboardData: DashboardResponse | null = null;
  currentUser: User | null = null;
  isLoading = false;

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private loadingService: LoadingService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.currentUser = this.authService.currentUserValue;
  }

  ngOnInit(): void {
    // Subscribe to loading state
    this.loadingService.loading$.subscribe(loading => {
      this.isLoading = loading;
      this.cdr.markForCheck();
    });
    
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.dashboardService.getDashboardContent().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.cdr.markForCheck();
      }
    });
  }

  onFeedbackSubmitted(event: {contentType: string, contentId: string, isPositive: boolean}): void {
    const feedback = { contentType: event.contentType, contentId: event.contentId, isPositive: event.isPositive };

    this.dashboardService.submitFeedback(feedback).subscribe({
      next: () => {
        // Update the local feedback state
        this.updateLocalFeedback(event.contentType, event.contentId, event.isPositive);
        this.cdr.markForCheck();
      }
    });
  }

  private updateLocalFeedback(contentType: string, contentId: string, isPositive: boolean): void {
    if (!this.dashboardData) return;

    const feedbackValue = isPositive ? 1 : -1;

    switch (contentType) {
      case 'news':
        const newsItem = this.dashboardData.news.find(n => n.id === contentId);
        if (newsItem) {
          newsItem.userFeedback = newsItem.userFeedback === feedbackValue ? null : feedbackValue;
        }
        break;
      case 'price':
        const priceItem = this.dashboardData.prices.find(p => p.id === contentId);
        if (priceItem) {
          priceItem.userFeedback = priceItem.userFeedback === feedbackValue ? null : feedbackValue;
        }
        break;
      case 'ai_insight':
        if (this.dashboardData.aiInsight && this.dashboardData.aiInsight.id === contentId) {
          this.dashboardData.aiInsight.userFeedback = 
            this.dashboardData.aiInsight.userFeedback === feedbackValue ? null : feedbackValue;
        }
        break;
      case 'meme':
        if (this.dashboardData.meme && this.dashboardData.meme.id === contentId) {
          this.dashboardData.meme.userFeedback = 
            this.dashboardData.meme.userFeedback === feedbackValue ? null : feedbackValue;
        }
        break;
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}