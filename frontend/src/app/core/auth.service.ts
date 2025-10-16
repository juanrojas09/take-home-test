import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { ApiResponse } from '../shared/types/ApiResponse';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token?: string;

}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  login(email: string, password: string): Observable<ApiResponse<LoginResponse>> {
    const loginData: LoginRequest = { email, password };

    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, loginData, {
      withCredentials: true
    }).pipe(
      tap(response => {
        if (response.data?.token) {
          sessionStorage.setItem('jwt_token', response.data.token);
        }
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem('jwt_token');
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        this.router.navigate(['/login']);
      }
    });
  }


}
