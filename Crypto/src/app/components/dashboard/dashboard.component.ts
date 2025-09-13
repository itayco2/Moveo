import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardService } from '../../services/dashboard.service';
import { AuthService } from '../../services/auth.service';
import { DashboardResponse } from '../../models/dashboard.models';
import { User } from '../../models/auth.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  dashboardData: DashboardResponse | null = null;
  isLoading = true;
  errorMessage = '';
  currentUser: User | null = null;
  showAllNews = false;
  newsLimit = 6;

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private router: Router
  ) {
    this.currentUser = this.authService.currentUserValue;
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.dashboardService.getDashboardContent().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to load dashboard. Please try again.';
      }
    });
  }

  submitFeedback(contentType: string, contentId: string, isPositive: boolean): void {
    const feedback = { contentType, contentId, isPositive };

    this.dashboardService.submitFeedback(feedback).subscribe({
      next: () => {
        // Update the local feedback state
        this.updateLocalFeedback(contentType, contentId, isPositive);
      },
      error: (error) => {
        // Handle error silently
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

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatPrice(price: number): string {
    if (price >= 1) {
      return price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    } else {
      return price.toLocaleString('en-US', { minimumFractionDigits: 4, maximumFractionDigits: 6 });
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  onImageError(event: Event, symbol: string): void {
    const img = event.target as HTMLImageElement;
    img.src = `https://ui-avatars.com/api/?name=${symbol}&size=40&background=1a1a1a&color=ffffff`;
  }

  get displayedNews() {
    if (!this.dashboardData?.news) return [];
    return this.showAllNews ? this.dashboardData.news : this.dashboardData.news.slice(0, this.newsLimit);
  }

  get hasMoreNews(): boolean {
    return (this.dashboardData?.news?.length ?? 0) > this.newsLimit;
  }

  toggleNewsView(): void {
    this.showAllNews = !this.showAllNews;
  }
}