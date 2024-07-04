import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BadgesModel } from '../../Models/BadgesModel';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class BadgesService {
  private baseUrl = 'https://localhost:7139/api/Badge';

  constructor(private http: HttpClient) {}

  getBadgesByUserId(userId: string): Observable<BadgesModel[]> {
    return this.http.get<BadgesModel[]>(`${this.baseUrl}/GetAllBadgesByUser`, {
      params: new HttpParams().set('userId', userId),
    });
  }
}
