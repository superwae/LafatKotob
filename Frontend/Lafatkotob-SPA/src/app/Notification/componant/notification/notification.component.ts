import { map } from 'rxjs';
import { NotificationServiceService } from './../../service/notification-service.service';
import { NotificationModel } from '../../Models/Notification';

import { Component, Input, OnInit } from '@angular/core';
import { FormatDatePipe } from '../../../shared/pipes/format-date.pipe';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule, FormatDatePipe],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css',
})
export class NotificationComponent {
  @Input() Notification!: NotificationModel;

  constructor(
    private notificationService: NotificationServiceService,
    private router: Router,
  ) {}
  navigateToBook(): void {
    if (!this.Notification.isRead) {
      this.notificationService.updateUnreadCountBy(-1);
      this.Notification.isRead = true;
    }
    this.notificationService.updateNotification(this.Notification).subscribe(
      (updatedNotification) => {
        this.Notification = updatedNotification;
      },
      (error) => {
        console.error('Error updating notification:', error);
        // Handle the error here
      },
    );

    if (
      this.Notification.message.includes('Liked Your Post') ||
      this.Notification.message.includes('has been reported') ||
      this.Notification.message.includes('Post a book that may interest you')
    ) {
      const bookId = this.extractBookId(this.Notification.message);
      this.router.navigate(['/book-details', bookId]);
    }
    if (
      this.Notification.message.includes('egister to attend') ||
      this.Notification.message.includes(
        'canceled the event attendance registration',
      ) ||
      this.Notification.message.includes(
        'You have an event scheduled for tomorrow',
      )
    ) {
      const eventId = this.extractBookId(this.Notification.message);
      this.router.navigate(['/event-details', eventId]);
    }
    if (this.Notification.message.includes('badge')) {
      this.router.navigate([
        '/user/' + localStorage.getItem('userName') + '/dash-board',
      ]);
    }
  }

  private extractBookId(message: string): string {
    const parts = message.split(':');
    return parts.length > 1 ? parts[1].trim() : '';
  }
  ngOnInit(): void {}
}
