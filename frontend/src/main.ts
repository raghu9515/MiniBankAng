import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { DashboardComponent } from './app/dashboard.component';
import { LoginComponent } from './app/login.component';
import { AuthService } from './app/auth.service';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'login', component: LoginComponent }
];

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(withInterceptors([AuthService.attachToken])),
    provideRouter(routes),
    AuthService
  ]
}).catch(err => console.error(err));
