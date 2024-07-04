import { NotificationModel } from './../Models/Notification';
import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { ConnectionsService } from '../../Auth/services/ConnectionService/connections.service';
@Injectable({
  providedIn: 'root',
})
export class NotificationServiceService implements OnDestroy {
  constructor(
    private http: HttpClient,
    private connectionsService: ConnectionsService,
  ) {
    this.registerNotificationEvents();
  }

  private NotificationsSubject = new BehaviorSubject<NotificationModel[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$: Observable<number> = this.unreadCountSubject.asObservable();
  Notifications$ = this.NotificationsSubject.asObservable();
  private chatHubUrl = 'https://localhost:7139/chatNotifacation';
  private hubConnection!: signalR.HubConnection;

  initializeNotifications(userId: string): void {
    this.getnotifications(userId).subscribe((notifications) => {
      this.NotificationsSubject.next(notifications);
    });
  }

  private registerNotificationEvents(): void {
    this.connectionsService.startConnection();
    this.hubConnection = this.connectionsService.HubConnection;

    this.hubConnection.on(
      'NotificationModel',
      (notification: NotificationModel) => {
        const currentNotifications = this.NotificationsSubject.getValue();
        this.NotificationsSubject.next([notification, ...currentNotifications]);

        if (!notification.isRead) {
          this.updateUnreadCountBy(1);
        }
      },
    );
    this.hubConnection.on('NotificationModelDelete', (data: any) => {
      this.updateUnreadCountBy(-1);
      const notificationId = data.id;
      const currentNotifications = this.NotificationsSubject.getValue();
      const updatedNotifications = currentNotifications.filter(
        (notification) => notification.id !== notificationId,
      );
      this.NotificationsSubject.next(updatedNotifications);
    });

    // Ensure the connection is started
    this.hubConnection
      .start()
      .catch((err) => console.error('Error while starting connection: ' + err));
  }

  public updateUnreadCountBy(change: number): void {
    const currentCount = this.unreadCountSubject.getValue();
    this.unreadCountSubject.next(currentCount + change);
  }

  fetchInitialUnreadCount(userId: string): void {
    this.getnotificationsUnRead(userId).subscribe((count) => {
      this.unreadCountSubject.next(count);
    });
  }
  private baseUrl = 'https://localhost:7139/api/notification';

  PostNotification(notification: NotificationModel): Observable<void> {
    return this.http.post<void>(this.baseUrl + '/post', notification);
  }
  GetNotificationById(id: number): Observable<NotificationModel> {
    const params = new HttpParams().set('id', id);
    return this.http.get<NotificationModel>(this.baseUrl + '/getbyid', {
      params,
    });
  }

  private baseUr2 = 'https://localhost:7139/api/NotificationUser';

  PostUserNotification(
    notification: NotificationModel,
    id: string,
  ): Observable<void> {
    const params = new HttpParams().set('Id', id);
    return this.http.post<void>(this.baseUr2 + '/post', notification, {
      params,
    });
  }

  getnotifications(userid: string) {
    const params = new HttpParams().set('userid', userid);
    return this.http.get<NotificationModel[]>(this.baseUr2 + '/getbyUserid', {
      params,
    });
  }
  getnotificationsNavBar(userid: string) {
    const params = new HttpParams().set('userid', userid);
    return this.http.get<NotificationModel[]>(
      this.baseUr2 + '/getbyUseridFive',
      { params },
    );
  }
  getnotificationsUnRead(userid: string) {
    const params = new HttpParams().set('userid', userid);
    return this.http.get<number>(this.baseUr2 + '/getbyUseridFalse', {
      params,
    });
  }
  updateNotification(
    notification: NotificationModel,
  ): Observable<NotificationModel> {
    const url = `${this.baseUrl}/update`;
    return this.http.put<NotificationModel>(url, notification).pipe(
      map((updatedNotification) => {
        return updatedNotification;
      }),
    );
  }
  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => console.log('Connection stopped'));
    }
  }
}
