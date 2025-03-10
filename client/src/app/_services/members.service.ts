import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  user = this.accountService.currentUser();
  baseUrl = environment.apiUrl;
  /* members = signal<Member[]> ([]); */
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();
  userParams = signal<UserParams>(new UserParams(this.user));

resetUserParams() {
  this.userParams.set(new UserParams(this.user));
}


  getMembers() {
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    if (response) return setPaginatedResponse(response, this.paginatedResult);

    console.log(Object.values(this.userParams()).join('-'));

    /* HttpParams It's used to construct the parameters that will be added to the URL when making a request. */
    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);
    params = params.append('minAge', this.userParams().minAge.toString());
    params = params.append('maxAge', this.userParams().maxAge.toString());
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    /* observe: 'response': This means the entire HTTP response is returned (not just the body). 
    The response includes the body, headers, status code, etc.
params: These are the query parameters (pageNumber and pageSize) that were built earlier. */
    return this.http.get<Member[]>(this.baseUrl + 'users', {observe: 'response', params}).subscribe({
      next:response => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      }
      })
   
  }
/* 
  private setPaginatedResponse(response: HttpResponse<Member[]>) {
    this.paginatedResult.set({
      items: response.body as Member[],
      pagination: JSON.parse(response.headers.get('Pagination')!)
    })
  }

  private setPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();
    if (pageNumber && pageSize) {
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
    
  }
  return params;
} */

  getMember(username: string)
  {
    const member : Member = [...this.memberCache.values()]
    .reduce((arr, elem) => arr.concat(elem.body), [])
    .find((m: Member) => m.username === username);

    if (member) return of(member);
    /* const member = this.members().find(x => x.username === username);
    /* of turns member into an observable */
    /*if (member !== undefined) return of(member); */

    return this.http.get<Member>(this.baseUrl + 'users/' + username)
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
     /*  tap(() => {
       this.members.update(members => members.map(m => m.username === member.username ? member : m))
      }) */
    )
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe(
    /*  tap(() => {
      this.members.update(members => members.map(m => {
        if (m.photos.includes(photo)) {
          m.photoUrl = photo.url
        }
        return m;
      }))
    }) */
    )
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      /* tap(() => {
        this.members.update(members => members.map(m => {
          if (m.photos.includes(photo)) {
            m.photos = m.photos.filter(x => x.id !== photo.id)
          }
          return m
        }))
      }) */
    )
  }
}
