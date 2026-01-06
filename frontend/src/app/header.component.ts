import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: `
    <header class="toolbar">
      <a routerLink="/">MiniBank</a>
      <nav>
        <a routerLink="/">Dashboard</a>
        <a routerLink="/login" *ngIf="!(auth.isAuthenticated())">Login</a>
        <button (click)="auth.logout()" *ngIf="auth.isAuthenticated()">Logout</button>
      </nav>
    </header>
  `,
  styles: [`
    .toolbar { display:flex; justify-content: space-between; align-items: center; padding: 1rem; background: #f5f5f5; }
    nav a, nav button { margin-left: 0.5rem; }
    button { border: none; background: #2c3e50; color: #fff; padding: 0.5rem 1rem; cursor:pointer; }
  `]
})
export class HeaderComponent {
  constructor(public auth: AuthService) {}
}
