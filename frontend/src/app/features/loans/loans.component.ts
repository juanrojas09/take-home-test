import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LoansService, Loan, PaginationData } from '../../core/loans.service';
import { AuthService } from '../../core/auth.service';
import { JwtService } from '../../core/jwt.service';

@Component({
  selector: 'app-loans',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loans.component.html',
  styleUrls: ['./loans.component.scss']
})
export class LoansComponent implements OnInit {

  jwtSevice=inject(JwtService)

  loans = signal<Loan[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  currentPage = signal<number>(1);
  pageSize = signal<number>(5);
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);

  visiblePages = signal<number[]>([]);

  userRole=signal<boolean>(this.jwtSevice.isAdmin() || false);


  hasLoans = computed(() => this.loans().length > 0);
  showPagination = computed(() => !this.isLoading() && this.totalPages() > 1);
  paginationInfo = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize() + 1;
    const end = Math.min(this.currentPage() * this.pageSize(), this.totalCount());
    const total = this.totalCount();
    return { start, end, total };
  });


  Math = Math;

  constructor(
    private loansService: LoansService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadLoans();

  }

  loadLoans(page: number = 1): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.currentPage.set(page);

    this.loansService.getLoans(page, this.pageSize()).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.loans.set(response.data.items);
          this.totalCount.set(response.data.totalCount);
          this.currentPage.set(response.data.currentPage);
          this.pageSize.set(response.data.pageSize);
          this.totalPages.set(response.data.totalPages || Math.ceil(response.data.totalCount / this.pageSize()));
          this.updateVisiblePages();
        } else {
          this.errorMessage.set(response.message || 'Error loading loans.');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Error loading loans:', error);

        if (error.status === 401) {
          this.errorMessage.set('Expired Session .');
          setTimeout(() => this.router.navigate(['/login']), 2000);
        } else {
          this.errorMessage.set('There was an error loading loans.');
        }
      }
    });
  }

  updateVisiblePages(): void {
    const maxVisible = 5;
    const current = this.currentPage();
    const total = this.totalPages();

    let start = Math.max(1, current - Math.floor(maxVisible / 2));
    let end = Math.min(total, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    const pages: number[] = [];
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    this.visiblePages.set(pages);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) {
      return;
    }
    this.loadLoans(page);
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.loadLoans(this.currentPage() + 1);
    }
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.loadLoans(this.currentPage() - 1);
    }
  }

  getStatusClass(status: string): string {
    return status === 'ACTIVE' ? 'status-active' : 'status-paid';
  }

  logout(): void {
    this.authService.logout();
  }
}
