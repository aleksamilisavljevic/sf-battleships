import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { LoginService } from './login.service';
import { User } from '../model/user.type';
import { catchError, Subscription } from 'rxjs';
import { DataService } from '../data.service';
import { Router } from "@angular/router";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit, OnDestroy {
  loginService = inject(LoginService);
  user : User | null = null;
  subscription: Subscription = new Subscription;
  message = "";

  constructor(private data: DataService, private router: Router) { }
  login(usernameInput : string, password: string) {
    this.message = "Please wait..."
    this.loginService.loginUser(usernameInput, password).pipe(catchError((err) => {console.log(err); this.message = "Server can't be reached!"; throw err;})).subscribe((result) => {
      if (result == null) {
        this.message = "Please check your username and password.";
      }
      else {
        let user : User = (Object.values(Object.values(result)[0])[0] as unknown as User);
        let token : string = (Object.values(Object.values(result)[0])[1] as unknown as string);
        this.data.changeUser(user);
        this.data.changeToken(token);
        this.router.navigate(['']);
      }
    });
  }
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
  ngOnInit(): void {
    this.subscription = this.data.currentUser.subscribe(user => this.user = user);
  }
}
