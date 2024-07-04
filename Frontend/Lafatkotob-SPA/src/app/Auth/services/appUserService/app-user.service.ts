import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, model } from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  catchError,
  map,
  pipe,
  tap,
  throwError,
} from 'rxjs';
import { login } from '../../Models/LoginModel';
import { ResetPasswordModel } from '../../Models/ResetPasswordModel';
import { AppUserModel } from '../../Models/AppUserModel';
import { LoginResponse } from '../../Models/Loginresponse';
import { SetUserHistoryModel } from '../../Models/SetUserHistoryModel';
import { Route, Router } from '@angular/router';
import { ChangePasswordModel } from '../../Models/ChangePasswordModel';
import { UpdateUserSettingModel } from '../../Models/UpdateUserSettingModel';
import { __values } from 'tslib';
import { AppUserAdminPageModel } from '../../Models/AppUserAdminPageModel';
import { SendEmailModel } from '../../../shared/Models/SendEmailModel';

@Injectable({
  providedIn: 'root',
})
export class AppUsereService {
  sendEmail(formData: SendEmailModel):Observable<SendEmailModel> {

    return this.http.post<SendEmailModel>(`${this.baseUrl}/Contact`,formData);

  }
  

  private userSubject = new BehaviorSubject<AppUserAdminPageModel[]>([]);
  users$ = this.userSubject.asObservable();

  private baseUrl = 'https://localhost:7139/api/AppUser';
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(
    this.isLoggedIn(),
  );
  public isAuthenticated = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}
  loginUser(loginData: login): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/Login`, loginData)
      .pipe(
        map((response) => {
          localStorage.setItem('token', response.token);
          localStorage.setItem('userId', response.userId);
          localStorage.setItem('userName', response.userName);
          localStorage.setItem('profilePicture', response.profilePicture);
          localStorage.setItem('role', response.role);
          localStorage.setItem(
            'emailConformed',
            response.emailConfirmed.toString(),
          );
          this.isAuthenticatedSubject.next(true);
          window.location.href = '/home';
          return response;
        }),
      );
  }

  signup(formData: FormData, role: string): Observable<any> {
    const params = new HttpParams().set('role', role);
    return this.http.post(`${this.baseUrl}/Register`, formData, { params });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/forgot-password`, { email });
  }

  resetPassword(data: ResetPasswordModel): Observable<any> {
    return this.http.put(`${this.baseUrl}/reset-password`, data);
  }

  getUserById(id: string): Observable<AppUserModel> {
    return this.http.get<AppUserModel>(`${this.baseUrl}/getbyid?userId=${id}`);
  }
  getALlUser(
    pageNumber: number = 1,
    pageSize: number = 20,
  ): Observable<AppUserAdminPageModel[]> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<AppUserAdminPageModel[]>(`${this.baseUrl}/getall`, {
      params,
    });
  }

  updateUserHistoryId(data: SetUserHistoryModel): Observable<any> {
    return this.http.put(`${this.baseUrl}/set-historyId`, data);
  }
  updateUserProfilePicture(data: FormData): Observable<any> {
    return this.http.put(`${this.baseUrl}/UpdateProfilePicture`, data);
  }
  UpdateUser(data: AppUserModel): Observable<any> {
    return this.http.put(`${this.baseUrl}/UpdateUser`, data);
  }

  updateUserBio(id: string, about: string) {
    const payload = {
      id: id,
      about: about,
    };
    return this.http.put(`${this.baseUrl}/updateBio`, payload);
  }
  updateUserCity(id: string, City: string) {
    const payload = {
      Id: id,
      City: City,
    };
    return this.http.put(`${this.baseUrl}/UpdateCity`, payload);
  }

  isLoggedIn(): boolean {
    const token = localStorage.getItem('token');
    if (!token) {
      return false;
    }

    const payload = this.decodeToken(token);
    return !this.isTokenExpired(payload);
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('userName');
    localStorage.removeItem('profilePicture');
    localStorage.removeItem('role');
    localStorage.removeItem('emailConformed');
    this.isAuthenticatedSubject.next(false);
    window.location.href = '/home';
  }

  private decodeToken(token: string): any {
    try {
      const base64Payload = token.split('.')[1];
      const jsonPayload = atob(base64Payload);
      return JSON.parse(jsonPayload);
    } catch (e) {
      console.error('Error decoding token', e);
      return null;
    }
  }

  private isTokenExpired(payload: any): boolean {
    if (!payload || !payload.exp) {
      return true;
    }
    const currentTime = Math.floor(Date.now() / 1000);
    return payload.exp < currentTime;
  }
  updatePassword(Model: ChangePasswordModel) {
    return this.http.put(`${this.baseUrl}/change-password`, Model);
  }

  updateUserSettings(model: UpdateUserSettingModel): Observable<any> {
    return this.http.put(`${this.baseUrl}/UpdateUserSetting`, model);
  }

  GetUserByUserName(userName: string): Observable<AppUserModel> {
    return this.http.get<AppUserModel>(
      `${this.baseUrl}/GetUserByUserName?userName=${userName}`,
    );
  }

  resendVerificationEmail(userId: string, email: string): Observable<any> {
    let params = new HttpParams();
    params = params.set('userId', userId);
    params = params.set('email', email);

    return this.http.post(`${this.baseUrl}/resendemail`, null, { params });
  }
  refreshUsers(pageNumber: number = 1, pageSize: number = 20): void {
    this.getALlUser(pageNumber, pageSize).subscribe((users) => {
      if (pageNumber === 1) {
        this.userSubject.next(users);
      } else {
        const currentUsers = this.userSubject.getValue();
        this.userSubject.next([...currentUsers, ...users]);
      }
    });
  }
  updateUserRole(userId: string, role: string): Observable<any> {
    const url = `${this.baseUrl}/updateUserRole?userId=${userId}&role=${role}`;

    return this.http
      .put<any>(url, null, { responseType: 'json' }) // Explicitly set responseType to 'json' if not already
      .pipe(
        catchError((error) => {
          console.error('Error updating user role:', error);
          return throwError(
            () =>
              new Error('Error updating user role. Please try again later.'),
          );
        }),
      );
  }

  searchUsers(query: string): Observable<AppUserAdminPageModel[]> {
    return this.http
      .get<
        AppUserAdminPageModel[]
      >(`${this.baseUrl}/search`, { params: { query } })
      .pipe(
        tap((users) => {
          this.userSubject.next(users);
        }),
      );
  }

  toggleDelete(userId: string): Observable<boolean> {
    return this.http.put<boolean>(`${this.baseUrl}/ToggleDelete`, null, {
      params: { userId },
    });
  }

  vote(
    voterUserId: string,
    targetUserId: string,
    isUpvote: boolean,
  ): Observable<any> {
    return this.http.post(`https://localhost:7139/api/votes/vote`, {
      voterUserId,
      targetUserId,
      isUpvote,
    });
  }
}
