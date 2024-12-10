import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import {MatGridListModule} from '@angular/material/grid-list';
import { catchError, Subscription } from 'rxjs';
import { DataService } from '../data.service';
import { User } from '../model/user.type';
import { StartService } from './start.service';

@Component({
  selector: 'app-start',
  standalone: true,
  imports: [MatGridListModule],
  templateUrl: './start.component.html',
  styleUrl: './start.component.css',
})
export class StartComponent implements OnInit, OnDestroy {
  startService = inject(StartService);
  user : User | null = null;
  token : string | null = null;
  started = 0;
  subscription: Subscription = new Subscription;

  constructor(private data: DataService) { }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
  ngOnInit(): void {
    this.subscription = this.data.currentUser.subscribe(user => this.user = user);
    this.subscription = this.data.currentToken.subscribe(token => this.token = token);
  }
  two = 2;
  three = 2;
  four = 1;
  five = 1;
  won = false;
  lost = false;
  ok = true;
  val:number[][] = [[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0]];
  bot:number[][] = [[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0]];
  init() {
    this.two = 2;
    this.three = 2;
    this.four = 1;
    this.five = 1;
    this.ok = true;
  }
  createRange(n: number){
    return new Array(n);
  }
  clicked(i: number, j: number) {
    this.val[i][j] = 1^this.val[i][j];
    this.reverify();
  }
  reverify() {
    this.init();
    let curVal = [[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0]];
    for(let i = 0;i < 10; i++) {
      for(let j = 0;j < 10; j++) {
        curVal[i][j] = this.val[i][j];
      }
    }
    let c = 2;
    for(let i = 0;i < 10; i++) {
      for(let j = 0;j < 10; j++) {
        if(curVal[i][j]==1) {
          let mi = i;
          let mj = j;
          while(mi + 1 < 10 && curVal[mi+1][j]==1) {
            mi++;
          }
          while(mj + 1 < 10 && curVal[i][mj+1]==1) {
            mj++;
          }
          if(mi > i && mj > j) {
            this.ok = false;
          }
          else {
            if(mi==i && mj==j) {
              this.ok=false;
            }
            else {
              let ci=mi-i+1;
              let cj=mj-j+1;
              let d=ci;
              if(cj>d) {
                d=cj;
              }
              if(d==2) {
                this.two--;
              } 
              else {
                if(d==3) {
                  this.three--;
                }
                else {
                  if(d==4) {
                    this.four--;
                  }
                  else {
                    if(d==5) {
                      this.five--;
                    }
                    else {
                      this.ok=false;
                    }
                  }
                }
              }
              mi = i;
              mj = j;
              curVal[i][j] = c;
              while(mi + 1 < 10 && curVal[mi+1][j]==1) {
                mi++;
                curVal[mi][j] = c;
              }
              while(mj + 1 < 10 && curVal[i][mj+1]==1) {
                mj++;
                curVal[i][mj] = c;
              }
              c++;
            }
          }
        }
      }
    }
    if(this.two<0) {
      this.two=0;
      this.ok=false;
    }
    if(this.three<0) {
      this.three=0;
      this.ok=false;
    }
    if(this.four<0) {
      this.four=0;
      this.ok=false;
    }
    if(this.five<0) {
      this.five=0;
      this.ok=false;
    }
    for(let i = 0;i < 10; i++) {
      for(let j = 0;j < 9; j++) {
        if(curVal[i][j]>0 && curVal[i][j+1]>0 && curVal[i][j]!=curVal[i][j+1]) {
          this.ok=false;
        }
        if(curVal[j][i]>0 && curVal[j+1][i]>0 && curVal[j][i]!=curVal[j+1][i]) {
          this.ok=false;
        }
      }
    }
  }
  startGame() {
    if(this.user && this.token) {
      this.startService.startGame(this.user.username,this.val,this.token).pipe(catchError((err) => {console.log(err); this.started = -1; throw err;})).subscribe();
      if(this.started != -1) {
        this.won = false;
        this.lost = false;
        this.started = 1;
      }
      else {
        this.started = 0;
      }
    }
  }
  guess(i:number, j:number) {
    if(this.user && this.token) {
      let cur: number = 0;
      this.startService.move(this.user.username,i*10+j,this.token).pipe(catchError((err) => {console.log(err); throw err;})).subscribe((val) => {
      cur=val;
      if(cur < 0) {
        if(cur == -100) {
          this.bot[i][j] = 3;
          this.won = true;
        }
        else {
          this.bot[i][j] = 2;
          cur = -cur - 200;
          let ni = Math.floor(cur / 10);;
          let nj = cur%10;
          this.val[ni][nj] += 2;
          this.lost = true;
        }
      }
      else {
        this.bot[i][j] = 2 + Math.floor(cur / 100);;
        cur%=100;
        let ni = Math.floor(cur / 10);;
        let nj = cur%10;
        this.val[ni][nj] += 2;
      }}); 
    }
  }
  refresh() {
    for(let i = 0;i < 10; i++) {
      for(let j = 0;j < 10; j++) {
        this.val[i][j] = 0;
        this.bot[i][j] = 0;
      }
    }
    this.two=2;
    this.three=2;
    this.four=1;
    this.five=1;
    this.won=false;
    this.lost=false;
    this.ok=true;
    this.started = 0;
  }
}
