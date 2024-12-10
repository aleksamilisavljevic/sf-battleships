import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Input } from '@angular/core';
import { User } from '../model/user.type';

@Injectable({
  providedIn: 'root'
})
export class HomeService {
  http = inject(HttpClient);
}
