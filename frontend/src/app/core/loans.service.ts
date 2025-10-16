import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { JwtService } from './jwt.service';

export interface Applicant {
  fullName: string;
}

export interface Loan {
  id: number;
  currentBalance: number;
  applicant: Applicant;
  status: 'ACTIVE' | 'PAID';
  loanAmount?: number;
}

export interface PaginationData {
  items: Loan[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  data: T;
  isSuccess: boolean;
  message: string;
  statusCode: number;
  errors: string[];
}

@Injectable({
  providedIn: 'root'
})
export class LoansService {
  private baseApiUrl = `${environment.apiUrl}/loans`;

  constructor(
    private http: HttpClient,
    private jwtService: JwtService
  ) {}

  private getEndpointUrl(): string {
    const isAdmin = this.jwtService.isAdmin();
    return isAdmin ? this.baseApiUrl : `${this.baseApiUrl}/my`;
  }

  getLoans(page: number = 1, pageSize: number = 10): Observable<ApiResponse<PaginationData>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    const url = this.getEndpointUrl();

    return this.http.get<ApiResponse<PaginationData>>(url, {
      params,
      withCredentials: true
    });
  }


}
