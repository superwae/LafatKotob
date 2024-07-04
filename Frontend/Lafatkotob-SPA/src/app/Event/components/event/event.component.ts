import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { MyTokenPayload } from '../../../shared/Models/MyTokenPayload';
import { EventService } from '../../Service/event.service';
import { EventModel } from '../../Models/EventModels';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { UserEventModel } from '../../Models/userEventModel';
import { AppUserModel } from '../../../Auth/Models/AppUserModel';
import { WhoseProfileService } from '../../../shared/Service/WhoseProfileService/whose-profile.service';
@Component({
  selector: 'app-event',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './event.component.html',
  styleUrls: ['./event.component.css'],
})
export class EventComponent implements OnInit {
  @Input() event: EventModel | null = null;
  unregesterButtonState = false;
  User: AppUserModel | null = null;
  constructor(
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private eventService: EventService,
    private cdRef: ChangeDetectorRef,
    private WhoseProfileService: WhoseProfileService,
  ) { }

  ngOnInit(): void {
    const eventId = this.route.snapshot.params['id'];
    const userId =
      this.getUserInfoFromToken()?.[
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
      ];
    console.log("banana");
    console.log(" eventId:" + eventId);
    console.log("userId:" + userId);
    if (userId && eventId) {
      this.eventService.getEventById(eventId).subscribe({
        next: (data) => {
          this.event = data;
          this.unregesterButtonState = this.eventService.isRegester();

        },
        error: (err) => {
          console.error(err);
        },
      });


    }
    if (this.event?.id) {
      this.eventService.getUserByEventId(this.event?.id).subscribe({
        next: (data) => {
          if (data != null) {
            this.User = data;
            this.cdRef.detectChanges();
            console.log("banana2" + this.User.name);
          }
        }
        ,
        error: (err) => {
          console.error(err);
        },
      });

      console.log("banana3");
    }
  }

  GetInfoForUserProfile(isOther: boolean, userName: string) {
    this.WhoseProfileService.isOtherProfile = isOther;
    this.WhoseProfileService.otherUserName = userName;
  }

  getUserInfoFromToken(): MyTokenPayload | undefined {
    const token = localStorage.getItem('token');
    if (token) {
      return jwtDecode<MyTokenPayload>(token);
    }
    return undefined;
  }

  register(event: MouseEvent): void {
    if (localStorage.getItem('emailConformed') === 'false') {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '1000px',
        data: {
          message: 'You need to confirm your email to activate your account',
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (!result) {
          return;
        }
      });
      return;
    }
    const eventuser: UserEventModel = {
      Id: 0,
      EventId: this.event!.id,
      UserId: localStorage.getItem('userId')!,
      isRegested: true,
    };
    this.eventService.postUserEvent(eventuser).subscribe({
      next: () => { },
      error: (err) => {
        console.error('Error registering for event:', err);
      },
    });

    event.stopPropagation();
  }

  Unregister(event: MouseEvent): void {
    if (localStorage.getItem('emailConformed') === 'false') {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '1000px',
        data: {
          message: 'You need to confirm your email to activate your account',
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (!result) {
          return;
        } else {
          return;
        }
      });
      return;
    }
    const userId = localStorage.getItem('userId');
    if (userId != null) {
      this.eventService.DeleteUserEvent(this.event!.id, userId).subscribe({
        next: () => { },
        error: (err) => {
          console.error('Error unregistering for event:', err);
        },
      });
    }
  }
}
