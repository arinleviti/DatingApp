import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  baseUrl = 'https://localhost:5001/api/';
  currentUser =signal<User | null>(null);
  
  /* The post() method will send the data (model) to the server at that URL, 
  and it will return an Observable that you can subscribe to for handling the server's response. 
  
  The <User> part tells TypeScript what type the response will be.
   The <T> in post<T> determines the type of data returned by the HTTP request, which in turn affects what is passed to .set().
   localStorage is a built-in Web API that allows storing key-value pairs in the user's browser persistently (even after a page refresh).*/
  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
      }
      )
    );
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
        return user;
      }
      )
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
