import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, finalize } from 'rxjs/operators';
import { OnboardingRequest, UserPreferences } from '../models/onboarding.models';
import { LoadingService } from './loading.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OnboardingService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient, private loadingService: LoadingService) {}

  completeOnboarding(preferences: OnboardingRequest): Observable<UserPreferences> {
    this.loadingService.show();
    return this.http.post<UserPreferences>(`${this.API_URL}/onboarding/complete`, preferences)
      .pipe(
        finalize(() => this.loadingService.hide())
      );
  }

  getUserPreferences(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(`${this.API_URL}/onboarding/preferences`);
  }
}
