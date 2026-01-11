import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: false // <--- ADD THIS LINE TO FIX THE ERROR
})
export class AppComponent implements OnInit {
  public gameStatus: any;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    // This asks your new GameController for data
    this.http.get('/api/game/status').subscribe(
      (result) => {
        this.gameStatus = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }
}
