import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private presenceService = inject(PresenceService);
  baseUrl = environment.apiUrl;
  currentUser =signal<User | null>(null);
  roles= computed(() => {
    const user = this.currentUser();
    if (user && user.token) {
      const role = JSON.parse(atob(user.token.split('.')[1])).role;
      return Array.isArray(role) ? role : [role]
    }
    return [];
  })
  private likeService = inject(LikesService);
  /* The post() method will send the data (model) to the server at that URL, 
  and it will return an Observable that you can subscribe to for handling the server's response. 
  
  The <User> part tells TypeScript what type the response will be.
   The <T> in post<T> determines the type of data returned by the HTTP request, which in turn affects what is passed to .set().
   localStorage is a built-in Web API that allows storing key-value pairs in the user's browser persistently (even after a page refresh).*/

  /* Why Doesn’t login() Have subscribe()?
  Observables Should Be Subscribed to Where They Are Used

  login() returns an Observable<User>, so the component calling it should subscribe.
  The AccountService just prepares the request, while the component decides what to do with the response. */

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      }
      )
    );
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
        return user;
      }
      )
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.presenceService.stopHubConnection();
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);

          this.likeService.getLikesIds();
          this.presenceService.createHubConnection(user);
  }
}
