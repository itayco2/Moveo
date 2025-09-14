import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const user = this.authService.currentUserValue;
    
    // If not authenticated, redirect to login
    if (!user) {
      this.router.navigate(['/login']);
      return false;
    }
    
    // Check if route requires onboarding completion
    const requiresOnboarding = route.data['requiresOnboarding'] === true;
    const requiresOnboardingIncomplete = route.data['requiresOnboardingIncomplete'] === true;
    
    if (requiresOnboarding && !user.isOnboardingCompleted) {
      this.router.navigate(['/onboarding']);
      return false;
    }
    
    if (requiresOnboardingIncomplete && user.isOnboardingCompleted) {
      this.router.navigate(['/dashboard']);
      return false;
    }
    
    return true;
  }
}



