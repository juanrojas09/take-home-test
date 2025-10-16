export interface ApiResponse<T>{
  data?: T;
  isSuccess: boolean;
  message?: string;
  statusCode?: number;

}

export interface Pagination<T> {
  Items: T;
  TotalCount: number;
  TotalPages: number;
  CurrentPage: number;
  PageSize: number;
}
