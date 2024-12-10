import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { User } from '../model/user.type';
import { BattleshipGrid } from '../model/battleshipGrid.type';

@Injectable({
  providedIn: 'root'
})
export class StartService {
  http = inject(HttpClient);
  constructor() { }
  startGame(username : string, grid: number[][],token : string) {
    const url = `http://localhost:8748/Battleship/start?username=`+username;

    // Add a header
    const header = new HttpHeaders().set('Authorization', `Bearer ${token}`)
    let b : BattleshipGrid = {grid};
    const headers = { headers: header };
    return this.http.post(url, b, headers);
  }
  move(username : string, move: number,token : string) {
    const url = `http://localhost:8748/Battleship/move?username=`+username+`&playermove=`+move;

    // Add a header
    const header = new HttpHeaders().set('Authorization', `Bearer ${token}`)
    const headers = { headers: header };
    return this.http.post<number>(url,null,headers);
  }
}
