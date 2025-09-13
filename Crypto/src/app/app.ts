import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ErrorNotificationsComponent } from './components/error-notifications/error-notifications.component';
import { LoadingComponent } from './components/loading/loading.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ErrorNotificationsComponent, LoadingComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Crypto');
}
