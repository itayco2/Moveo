import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { NewsItem } from '../models/dashboard.models';

@Injectable({
  providedIn: 'root'
})
export class NewsService {
  private showAllNewsSubject = new BehaviorSubject<boolean>(false);
  public showAllNews$ = this.showAllNewsSubject.asObservable();
  
  private readonly NEWS_LIMIT = 6;

  getNewsLimit(): number {
    return this.NEWS_LIMIT;
  }

  getDisplayedNews(news: NewsItem[]): NewsItem[] {
    const showAll = this.showAllNewsSubject.value;
    return showAll ? news : news.slice(0, this.NEWS_LIMIT);
  }

  hasMoreNews(news: NewsItem[]): boolean {
    return news.length > this.NEWS_LIMIT;
  }

  toggleNewsView(): void {
    const currentValue = this.showAllNewsSubject.value;
    this.showAllNewsSubject.next(!currentValue);
  }

  getShowAllNews(): boolean {
    return this.showAllNewsSubject.value;
  }
}
