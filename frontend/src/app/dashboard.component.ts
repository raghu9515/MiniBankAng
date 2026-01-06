import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { AuthService } from './auth.service';

interface Account {
  id: string;
  name: string;
  balance: number;
  transactions: { amount: number; description: string; timestamp: string; type: string; }[];
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section>
      <h2>Accounts</h2>
      <p *ngIf="!auth.isAuthenticated()">Please log in to view accounts.</p>
      <article *ngFor="let account of accounts" class="card">
        <header>
          <strong>{{ account.name }}</strong>
          <span class="balance">{{ account.balance | currency }}</span>
        </header>
        <div *ngIf="account.transactions?.length">
          <h4>Recent activity</h4>
          <ul>
            <li *ngFor="let tx of account.transactions">
              <span>{{ tx.timestamp | date:'short' }}</span>
              <span>{{ tx.description }}</span>
              <span [class.credit]="tx.type === 'Credit'" [class.debit]="tx.type === 'Debit'">
                {{ tx.amount | currency }}
              </span>
            </li>
          </ul>
        </div>
      </article>
    </section>
  `,
  styles: [`
    .card { border: 1px solid #ddd; padding: 1rem; margin-bottom: 1rem; border-radius: 4px; }
    header { display: flex; justify-content: space-between; }
    .balance { font-weight: bold; }
    .credit { color: green; }
    .debit { color: red; }
  `]
})
export class DashboardComponent implements OnInit {
  accounts: Account[] = [];

  constructor(private http: HttpClient, public auth: AuthService) {}

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.http.get<Account[]>(`${environment.apiUrl}/api/accounts`).subscribe(accounts => this.accounts = accounts);
    }
  }
}
