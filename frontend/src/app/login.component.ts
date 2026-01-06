import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="card">
      <h2>Login</h2>
      <form (ngSubmit)="submit()">
        <label>Email<input name="email" [(ngModel)]="email" /></label>
        <label>Password<input name="password" [(ngModel)]="password" type="password" /></label>
        <button type="submit">Login</button>
      </form>
      <p class="hint">Use admin@minibank.test / P@ssw0rd! after seeding.</p>
    </section>
  `,
  styles: [`
    form { display:flex; flex-direction: column; gap: 0.5rem; }
    input { padding: 0.5rem; }
  `]
})
export class LoginComponent {
  email = '';
  password = '';

  constructor(private auth: AuthService) {}

  submit() {
    this.auth.login(this.email, this.password).subscribe(result => {
      this.auth.saveToken(result.token);
      location.href = '/';
    });
  }
}
