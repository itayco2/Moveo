import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, throwError, finalize } from 'rxjs';
import { AuthResponse, LoginRequest, SignupRequest, User } from '../models/auth.models';
import { environment } from '../../environments/environment';
import { ErrorService } from './error.service';
import { LoadingService } from './loading.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private errorService: ErrorService, private loadingService: LoadingService) {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    this.loadingService.show();
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        tap(response => this.handleAuthSuccess(response)),
        catchError(error => {
          this.errorService.addError(this.errorService.handleHttpError(error), 'error');
          return throwError(() => error);
        }),
        finalize(() => this.loadingService.hide())
      );
  }

  signup(userData: SignupRequest): Observable<AuthResponse> {
    this.loadingService.show();
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/signup`, userData)
      .pipe(
        tap(response => this.handleAuthSuccess(response)),
        catchError(error => {
          this.errorService.addError(this.errorService.handleHttpError(error), 'error');
          return throwError(() => error);
        }),
        finalize(() => this.loadingService.hide())
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.API_URL}/auth/me`);
  }

  refreshCurrentUser(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.getCurrentUser().subscribe({
        next: (user) => {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSubject.next(user);
          resolve();
        },
        error: () => {
          // If refresh fails, logout user
          this.logout();
          reject();
        }
      });
    });
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
  }

  private loadUserFromStorage(): void {
    const userStr = localStorage.getItem('user');
    if (userStr && userStr !== 'undefined' && userStr !== 'null') {
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      } catch (error) {
        this.logout();
      }
    }
  }
}
