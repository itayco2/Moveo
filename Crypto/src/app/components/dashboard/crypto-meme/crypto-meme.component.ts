import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Meme } from '../../../models/dashboard.models';

@Component({
  selector: 'app-crypto-meme',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './crypto-meme.component.html',
  styleUrl: './crypto-meme.component.css'
})
export class CryptoMemeComponent {
  @Input() meme: Meme | null = null;
  @Output() feedbackSubmitted = new EventEmitter<{contentType: string, contentId: string, isPositive: boolean}>();

  onFeedbackSubmit(isPositive: boolean): void {
    if (this.meme) {
      this.feedbackSubmitted.emit({
        contentType: 'meme',
        contentId: this.meme.id,
        isPositive
      });
    }
  }
}
