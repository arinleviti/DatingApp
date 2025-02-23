import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

/* CanActivateFn is a Function Type, which is like an interface in C# */
export const authGuard: CanActivateFn = (route, state) => {
  let accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  if (accountService.currentUser()) {
    return true;
  } else {
    toastr.error('You shall not pass!');  
    return false;
  }
};
