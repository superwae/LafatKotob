import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { UserPreference } from '../../Models/UserPreference';
import { registerModel } from '../../Models/registerModel';

@Injectable({
  providedIn: 'root',
})
export class GenreService {
  private baseUrl = 'https://localhost:7139/api';
  private userDetailsSource = new BehaviorSubject<registerModel | null>(null);
  public userDetails$ = this.userDetailsSource.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  postUserPreferences(preferences: UserPreference[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/postBatch`, preferences);
  }

  registerWithPreferences(
    formData: FormData,
    selectedGenres: number[],
  ): Observable<any> {
    selectedGenres.forEach((genreId, index) => {
      formData.append(`GenreIds`, genreId.toString());
    });
    const params = new HttpParams().set('role', 'User');
    return this.http.post(
      `${this.baseUrl}/AppUser/RegisterWithPreferences`,
      formData,
      { params },
    );
  }

  setUserDetails(details: registerModel): void {
    this.userDetailsSource.next(details);
  }
}
