import { Component, inject, Input } from '@angular/core';
import { HomeService } from './home.service';
import { User } from '../model/user.type';
import { catchError, Subscription } from 'rxjs';
import { DataService } from '../data.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  homeService = inject(HomeService);
  user : User | null = null;
  subscription: Subscription = new Subscription;

  constructor(private data: DataService) { }
  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
  ngOnInit(): void {
    this.subscription = this.data.currentUser.subscribe(user => this.user = user);
  }
}
