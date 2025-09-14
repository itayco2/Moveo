import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoinPrice } from '../../../models/dashboard.models';

@Component({
  selector: 'app-coin-prices',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './coin-prices.component.html',
  styleUrl: './coin-prices.component.css'
})
export class CoinPricesComponent {
  @Input() prices: CoinPrice[] = [];
  @Output() feedbackSubmitted = new EventEmitter<{contentType: string, contentId: string, isPositive: boolean}>();

  onFeedbackSubmit(coin: CoinPrice, isPositive: boolean): void {
    this.feedbackSubmitted.emit({
      contentType: 'price',
      contentId: coin.id,
      isPositive
    });
  }

  onImageError(event: Event, symbol: string): void {
    const img = event.target as HTMLImageElement;
    img.src = `https://ui-avatars.com/api/?name=${symbol}&size=40&background=1a1a1a&color=ffffff`;
  }

  formatPrice(price: number): string {
    if (price >= 1) {
      return price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    } else {
      return price.toFixed(6);
    }
  }
}
