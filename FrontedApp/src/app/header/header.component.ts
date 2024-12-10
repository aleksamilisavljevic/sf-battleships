import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { User } from '../model/user.type';
import { DataService } from '../data.service';
import { Subscription } from 'rxjs';
import {MatMenuModule} from '@angular/material/menu';
import {MatButtonModule} from '@angular/material/button';


@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatMenuModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit, OnDestroy {
  user : User | null = null;
  subscription: Subscription = new Subscription;

  constructor(private data: DataService, private router: Router) { }

  logout() {
    this.data.changeUser(null);
    this.data.changeToken(null);
    this.router.navigate(['']);
  }
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
  ngOnInit(): void {
    this.subscription = this.data.currentUser.subscribe(user => this.user = user);
  }
}
