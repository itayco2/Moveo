import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardResponse, FeedbackRequest } from '../models/dashboard.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getDashboardContent(): Observable<DashboardResponse> {
    return this.http.get<DashboardResponse>(`${this.API_URL}/dashboard/content`);
  }

  submitFeedback(feedback: FeedbackRequest): Observable<any> {
    return this.http.post(`${this.API_URL}/dashboard/feedback`, feedback);
  }
}
