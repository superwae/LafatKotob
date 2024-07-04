import { Injectable } from '@angular/core';
import { register } from '../../Models/SignupModel';
import { HttpClient, HttpClientModule, HttpParams } from '@angular/common/http';
import { login } from '../../Models/login';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AppUserServiceService {
  private apiurl="https://localhost:7139"


  constructor(private http: HttpClient) { }

  createUser(userData: register, role: string): Observable<register> {
    console.log('Sending user data to backend:', userData);
    console.log('With role:', role);
    const params = new HttpParams().set('role', role);
    return this.http.post<register>(`${this.apiurl}/api/AppUser/register`, userData, { params });
  }
  
  loginUser(userData: login): Observable<login> {
    return this.http.post<login>(`${this.apiurl}/api/AppUser/Login`, userData);
  }
}