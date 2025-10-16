import { Injectable } from '@angular/core';

interface JwtPayload {
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  [key: string]: any;
}


@Injectable({
  providedIn: 'root'
})
export class JwtService {

  decodeToken(token: string): JwtPayload | null {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      return null;
    }
  }



  getRoleFromToken(): string | null {
    const token = sessionStorage.getItem('jwt_token');

    if (!token) return null;

    const payload = this.decodeToken(token);
    return payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
  }

  isAdmin(): boolean {
    const role = this.getRoleFromToken();
    return role?.toLowerCase() === 'admin';
  }
}
