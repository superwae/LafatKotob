import { NotificationServiceService } from './../../service/notification-service.service';
import { NotificationModel } from './../../Models/Notification';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NotificationComponent } from '../notification/notification.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-notification-page',
  standalone: true,
  imports: [CommonModule, NotificationComponent],
  templateUrl: './notification-page.component.html',
  styleUrl: './notification-page.component.css',
})
export class NotificationPageComponent {
  notifications!: NotificationModel[];
  notification!: NotificationModel;
  constructor(
    private notificationService: NotificationServiceService,
    private cdRef: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.notificationService.Notifications$.subscribe((newNotifications) => {
      // This line seems incorrect because it prepends newNotifications to itself, potentially duplicating entries.
      this.notifications = newNotifications;

      // Detect changes to update the view
      this.cdRef.detectChanges();
    });

    this.notificationService
      .getnotifications(localStorage.getItem('userId')!)
      .subscribe((data) => {
        this.notifications = data;
      });
  }
  get Notifications$(): Observable<NotificationModel[]> {
    return this.notificationService.Notifications$;
  }

  trackByNotification(index: number, notification: NotificationModel): number {
    return notification.id;
  }
}
