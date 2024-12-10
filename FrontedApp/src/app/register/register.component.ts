import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { RegisterService } from './register.service';
import { User } from '../model/user.type';
import { catchError, first, Subscription } from 'rxjs';
import { DataService } from '../data.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit, OnDestroy {
  registerService = inject(RegisterService);
  user : User | null = null;
  subscription: Subscription = new Subscription;

  constructor(private data: DataService) { }
  message = "";
  register(usernameInput : string, password: string, firstName: string, lastName: string) {
    this.message="Please wait..."
    this.registerService.registerUser(usernameInput, password, firstName, lastName).pipe(catchError((err) => {this.message = "Server can't be reached!"; console.log(err); throw err;})).subscribe((message) => {this.message=message;});
  }
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
  ngOnInit(): void {
    this.subscription = this.data.currentUser.subscribe(user => this.user = user);
  }
}
