import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  getUserWithRoles() {
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  /* The HttpClient.post() method in Angular requires a body parameter.
Since you're sending roles via query parameters (?roles=), there's no actual body data to send.
{} is used as an empty object to satisfy the method's signature, even though no data is sent in the body. */
  updateUserRoles(username: string, roles: string []) {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles, {})
  }
}
