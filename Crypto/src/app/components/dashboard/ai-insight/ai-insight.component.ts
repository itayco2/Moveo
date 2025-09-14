import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiInsight } from '../../../models/dashboard.models';

@Component({
  selector: 'app-ai-insight',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ai-insight.component.html',
  styleUrl: './ai-insight.component.css'
})
export class AiInsightComponent {
  @Input() aiInsight: AiInsight | null = null;
  @Output() feedbackSubmitted = new EventEmitter<{contentType: string, contentId: string, isPositive: boolean}>();

  onFeedbackSubmit(isPositive: boolean): void {
    if (this.aiInsight) {
      this.feedbackSubmitted.emit({
        contentType: 'ai_insight',
        contentId: this.aiInsight.id,
        isPositive
      });
    }
  }
}
