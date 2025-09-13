import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OnboardingRequest, UserPreferences } from '../models/onboarding.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OnboardingService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  completeOnboarding(preferences: OnboardingRequest): Observable<UserPreferences> {
    return this.http.post<UserPreferences>(`${this.API_URL}/onboarding/complete`, preferences);
  }

  getUserPreferences(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(`${this.API_URL}/onboarding/preferences`);
  }
}
