import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../_directives/has-role.directive';


/* he NavComponent is responsible for gathering the data
 (in this case, the username and password from the login form) and making it available to the HTML template. */
@Component({
  selector: 'app-nav',
  imports: [FormsModule, BsDropdownModule, RouterLink, RouterLinkActive, HasRoleDirective],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  model: any = {};
  accountService = inject(AccountService);
  private router = inject(Router);
  private toaster = inject(ToastrService);

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        this.router.navigateByUrl('/members');
      },
      error: error => this.toaster.error(error.error)
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
