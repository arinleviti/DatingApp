import { HttpInterceptorFn } from '@angular/common/http';
import { BusyService } from '../_services/busy.service';
import { inject } from '@angular/core';
import { delay, finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);
  
  busyService.busy();
  
  return next(req).pipe(
    delay(1000),
    finalize(() => {
      busyService.idle()
    })
  );
};

/* The finalize operator can contain a method (or any code you want to execute)
 that you want to run once the observable has finished, whether the HTTP request is successful or results in an error. */
