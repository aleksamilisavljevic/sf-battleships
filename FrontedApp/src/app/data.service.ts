import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { User } from './model/user.type';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private userSource = new BehaviorSubject<User|null>(null);
  currentUser = this.userSource.asObservable();

  private tokenSource = new BehaviorSubject<string|null>(null);
  currentToken = this.tokenSource.asObservable();

  constructor() { }

  changeUser(user: User | null) {
    this.userSource.next(user);
  }

  changeToken(token: string | null) {
    this.tokenSource.next(token);
  }
}
