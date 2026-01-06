import { HttpClient, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../environments/environment';

@Injectable()
export class AuthService {
  private tokenKey = 'minibank_token';

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string) {
    return this.http.post<{ token: string }>(`${environment.apiUrl}/api/users/login`, { email, password });
  }

  register(email: string, password: string) {
    return this.http.post<{ token: string }>(`${environment.apiUrl}/api/users/register`, { email, password });
  }

  saveToken(token: string) { localStorage.setItem(this.tokenKey, token); }
  logout() { localStorage.removeItem(this.tokenKey); this.router.navigate(['/login']); }
  isAuthenticated() { return !!localStorage.getItem(this.tokenKey); }
  get token() { return localStorage.getItem(this.tokenKey); }

  static attachToken: HttpInterceptorFn = (req, next) => {
    const token = localStorage.getItem('minibank_token');
    if (token) {
      req = new HttpRequest(req.method, req.url, req.body, {
        headers: req.headers.set('Authorization', `Bearer ${token}`),
        context: req.context,
        reportProgress: req.reportProgress,
        responseType: req.responseType,
        withCredentials: req.withCredentials
      });
    }
    return next(req);
  };
}
