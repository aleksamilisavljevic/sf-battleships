import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { User } from '../model/user.type';
import { AuthResponse } from '../model/authResponse.type';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  http = inject(HttpClient);
  constructor() { }
  loginUser(username : string, password : string) {
    const url = `http://localhost:8748/Battleship/login?username=`+username+`&password=`+password;
    return this.http.get(url);
  }
}
