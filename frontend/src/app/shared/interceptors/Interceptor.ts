import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = sessionStorage.getItem('jwt_token');

  let headers = req.headers;
  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  const clonedReq = req.clone({
    headers: headers,
    withCredentials: true
  });

  return next(clonedReq);
};
