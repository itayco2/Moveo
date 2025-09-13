import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface ErrorMessage {
  id: string;
  message: string;
  type: 'error' | 'warning' | 'info' | 'success';
  timestamp: Date;
  autoClose?: boolean;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorService {
  private errorsSubject = new BehaviorSubject<ErrorMessage[]>([]);
  public errors$ = this.errorsSubject.asObservable();

  constructor() {}

  addError(message: string, type: ErrorMessage['type'] = 'error', autoClose: boolean = true, duration: number = 5000): void {
    const error: ErrorMessage = {
      id: this.generateId(),
      message,
      type,
      timestamp: new Date(),
      autoClose,
      duration
    };

    const currentErrors = this.errorsSubject.value;
    this.errorsSubject.next([...currentErrors, error]);

    if (autoClose) {
      setTimeout(() => {
        this.removeError(error.id);
      }, duration);
    }
  }

  removeError(errorId: string): void {
    const currentErrors = this.errorsSubject.value;
    const filteredErrors = currentErrors.filter(error => error.id !== errorId);
    this.errorsSubject.next(filteredErrors);
  }

  clearAllErrors(): void {
    this.errorsSubject.next([]);
  }

  getCurrentErrors(): ErrorMessage[] {
    return this.errorsSubject.value;
  }

  handleHttpError(error: any): string {
    if (error.error?.message) {
      return error.error.message;
    }
    
    if (error.status === 0) {
      return 'Network error. Please check your connection.';
    }
    
    if (error.status === 401) {
      return 'Unauthorized. Please log in again.';
    }
    
    if (error.status === 403) {
      return 'Access denied. You do not have permission to perform this action.';
    }
    
    if (error.status === 404) {
      return 'Resource not found.';
    }
    
    if (error.status === 429) {
      return 'Too many requests. Please try again later.';
    }
    
    if (error.status >= 500) {
      return 'Server error. Please try again later.';
    }
    
    return 'An unexpected error occurred. Please try again.';
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
}
