import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NewsItem } from '../../../models/dashboard.models';
import { NewsService } from '../../../services/news.service';

@Component({
  selector: 'app-market-news',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './market-news.component.html',
  styleUrl: './market-news.component.css'
})
export class MarketNewsComponent {
  @Input() news: NewsItem[] = [];
  @Output() feedbackSubmitted = new EventEmitter<{contentType: string, contentId: string, isPositive: boolean}>();

  constructor(public newsService: NewsService) {}

  get displayedNews(): NewsItem[] {
    return this.newsService.getDisplayedNews(this.news);
  }

  get hasMoreNews(): boolean {
    return this.newsService.hasMoreNews(this.news);
  }

  toggleNewsView(): void {
    this.newsService.toggleNewsView();
  }

  onFeedbackSubmit(newsItem: NewsItem, isPositive: boolean): void {
    this.feedbackSubmitted.emit({
      contentType: 'news',
      contentId: newsItem.id,
      isPositive
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
