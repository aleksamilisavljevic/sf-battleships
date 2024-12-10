import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { User } from '../model/user.type';


@Injectable({
  providedIn: 'root'
})
export class RegisterService {
  http = inject(HttpClient);
  constructor() { }
  registerUser(username : string, password : string, firstName: string, lastName: string) {
    const url = `http://localhost:8748/Battleship/register`
    let user : User = {username, password, firstName, lastName};
    return this.http.post(url, user, {
      responseType: 'text'
    });
  }
}
