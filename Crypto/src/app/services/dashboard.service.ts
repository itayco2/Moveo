import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, finalize } from 'rxjs/operators';
import { DashboardResponse, FeedbackRequest } from '../models/dashboard.models';
import { LoadingService } from './loading.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient, private loadingService: LoadingService) {}

  getDashboardContent(): Observable<DashboardResponse> {
    this.loadingService.show();
    return this.http.get<DashboardResponse>(`${this.API_URL}/dashboard/content`)
      .pipe(
        finalize(() => this.loadingService.hide())
      );
  }

  submitFeedback(feedback: FeedbackRequest): Observable<any> {
    return this.http.post(`${this.API_URL}/dashboard/feedback`, feedback);
  }
}
