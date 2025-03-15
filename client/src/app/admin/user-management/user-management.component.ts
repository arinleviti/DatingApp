import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
/* THis is the modal i created in modals\roles-modal */
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  imports: [],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private adminService = inject(AdminService)
  users: User[] = [];
  /* BsModalService is provided by the ngx-bootstrap library. It is responsible for opening and closing modals.
  You inject it into your component and use its .show() method to open modals*/
  private modalService = inject(BsModalService);
  
  /* BsModalRef is a Bootstrap Modal reference provided by the ngx-bootstrap library.
  BsModalRef is an object representing the currently open modal. 
  It allows us to store a reference to the modal so we can later close it or pass data*/
  
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  
  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  openRolesModal (user: User) {
    const initialState: ModalOptions ={
      /* class: 'modal-lg' → Makes the modal large.
       initialState → Passes data to the modal component.*/
      class: 'modal-lg',
      initialState: {
        title: 'User roles',
        username: user.username,
        selectedRoles: [...user.roles],
        availableRoles: ['Admin', 'Moderator', 'Member'],
        users: this.users,
        rolesUpdated: false
      }
    }
    /* Calls this.modalService.show() to open the modal. 
    The first parameter is the component to show (RolesModalComponent).
The second parameter is the configuration (initialState).*/
    this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if (this.bsModalRef.content && this.bsModalRef.content.rolesUpdated) {
          const selectedRoles = this.bsModalRef.content.selectedRoles;
          this.adminService.updateUserRoles(user.username, selectedRoles).subscribe({
            next: roles => user.roles = roles
          })
        }
      }
    })
  }

  getUsersWithRoles() {
    this.adminService.getUserWithRoles().subscribe({
      next: users => this.users = users
    })
  }

 
}
