import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class HistoryService {
  private baseUrl = 'https://localhost:7139/api/History';
  constructor(private http: HttpClient) {}

  postHistory(userId: string): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
    });
    return this.http.post<any>(this.baseUrl + '/post', JSON.stringify(userId), {
      headers: headers,
    });
  }
}
