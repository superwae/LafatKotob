import { state } from '@angular/animations';
import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EventService } from '../../Service/event.service';
import { EventModel } from '../../Models/EventModels';
import { CommonModule } from '@angular/common';
import { UserEventModel } from '../../Models/userEventModel';
import { MatDialog } from '@angular/material/dialog';

import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-event-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-details.component.html',
  styleUrls: ['./event-details.component.css'],
})
export class EventDetailsComponent implements OnInit {
  @Input() events: EventModel[] = [];
  event!: EventModel;
  IfPre = false;
  IfUpdate = false;
  eventIdForDelete = 0;
  newEvent: any;
  form: FormGroup;
  unregesterButtonState!: boolean | null;
  isDisabled = false;
  isDisabled2 = false;
  resendTimer: number = 30;
  resendInterval: any;
  resendTimer2: number = 30;
  resendInterval2: any;

  selectedImage: File | null = null;
  selectedImageUrl: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private eventService: EventService,
    private dialog: MatDialog,
    private fb: FormBuilder,
  ) {
    this.form = this.fb.group({
      HostName: ['', Validators.required],
      EventName: ['', Validators.required],
      Description: ['', Validators.required],
      DateScheduled: ['', Validators.required],
      Location: ['', Validators.required],
      attendances: [0, Validators.required],
    });
  }

  ngOnInit(): void {
    const eventId = this.route.snapshot.params['id'];
    this.eventIdForDelete = eventId;
    const role = localStorage.getItem('role');
    if (role === 'Admin' || role === 'Premium') {
      this.IfPre = true;
    }
    if (eventId) {
      this.eventService.getEventById(eventId).subscribe({
        next: (data) => {
          this.event = data;
        },
        error: (err) => {
          console.error('Error fetching event:', err);
        },
      });
      if (localStorage.getItem('userId') && eventId) {
        this.eventService
          .getUserEvent(localStorage.getItem('userId')!, eventId)
          .subscribe({
            next: (data) => {
              if (data) this.unregesterButtonState = true;
            },
            error: (err: any) => {
              if (err.status === 404) this.unregesterButtonState = false;
              console.error('Error fetching user event:', err);
            },
          });
      }
    }
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
    this.isDisabled = true;
    this.event.attendances++;
    const eventuser: UserEventModel = {
      Id: 0,
      EventId: this.event!.id,
      UserId: localStorage.getItem('userId')!,
      isRegested: true,
    };

    this.unregesterButtonState = true;

    this.eventService.postUserEvent(eventuser).subscribe({
      next: () => {
        this.resendInterval = setInterval(() => {
          this.resendTimer--;
          if (this.resendTimer === 0) {
            clearInterval(this.resendInterval);
            this.isDisabled = false;
            this.resendTimer = 30;
          }
        }, 1000);
      },
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
        }
      });
      return;
    }
    this.isDisabled2 = true;
    this.event.attendances--;
    const userId = localStorage.getItem('userId');
    this.event.isRegested = false;
    this.unregesterButtonState = this.event.isRegested;

    if (userId) {
      this.eventService.DeleteUserEvent(this.event.id, userId).subscribe({
        next: () => {
          this.resendInterval2 = setInterval(() => {
            this.resendTimer2--;
            if (this.resendTimer2 === 0) {
              clearInterval(this.resendInterval2);
              this.isDisabled2 = false;
              this.resendTimer2 = 30;
            }
          }, 1000);
        },
        error: (err) => {
          console.error('Error unregistering for event:', err);
        },
      });
    }
  }

  openForm() {
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
    this.IfUpdate = true;
  }

  CloseForm() {
    this.IfUpdate = false;
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    const files = target.files;

    if (files && files.length) {
      const file = files[0];
      this.selectedImage = file;

      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        this.selectedImageUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  removeSelectedImage($event: Event) {
    $event.stopPropagation();
    this.selectedImage = null;
    this.selectedImageUrl = null;
  }

  DeleteEvent(event: MouseEvent): void {
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
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '500px',
      data: { message: 'Are you sure you want to delete this event?' },
    });

    dialogRef.afterClosed().subscribe((result: boolean) => {
      if (result) {
        this.eventService
          .DeleteAllRelationAndEvent(this.eventIdForDelete)
          .subscribe({
            next: () => {
              this.CloseForm();
            },
            error: (error) => {
              console.error('Error deleting event:', error);
            },
          });
      }
    });
  }

  UpdateEvent(): void {
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
    if (this.form.valid) {
      const formData = new FormData();
      formData.append('eventName', this.form.value.EventName);
      formData.append('description', this.form.value.Description);
      formData.append('dateScheduled', this.form.value.DateScheduled);
      formData.append('location', this.form.value.Location);
      formData.append('hostUserId', localStorage.getItem('userId')!);
      formData.append('attendances', this.form.value.attendances);
      formData.append('ImagePath', 'banana');

      if (this.selectedImage) {
        formData.append(
          'imageFile',
          this.selectedImage,
          this.selectedImage.name,
        );
      }
      this.eventService.updateEvent(this.eventIdForDelete, formData).subscribe({
        next: () => {
          this.CloseForm();
        },
        error: (error) => {
          console.error('Error updating event:', error);
        },
      });
    }
  }
}
