import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './header.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, HeaderComponent],
  template: `
    <app-header></app-header>
    <main class="container">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .container { max-width: 960px; margin: 2rem auto; }
  `]
})
export class AppComponent { }
