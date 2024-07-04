import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { EventService } from '../../Service/event.service';
import { EventModel } from '../../Models/EventModels';
import { EventComponent } from '../event/event.component';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Observable } from 'rxjs';
import { EventFilterComponent } from '../../../shared/components/event-filter/event-filter.component';
import { LoadStatusService } from '../../../Book/Service/load-status.service';

@Component({
  selector: 'app-events',
  standalone: true,
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.css'],
  imports: [
    CommonModule,
    EventComponent,
    ReactiveFormsModule,
    EventFilterComponent,
  ],
})
export class EventsComponent implements OnInit {
  @Input() events: EventModel[] = [];
  HideEvents = false;
  IfPre = false;
  isMine = false;
  isOther = false;
  newEvent: any;
  form: FormGroup;
  currentUsername: string | null = null;
  selectedImage: File | null = null;
  selectedImageUrl: string | null = null;
  isProfilePage: boolean = false;
  allEventsLoaded = false;
  loadingEvents = false;
  lastLoadTime = 0;
  cooldown = 1000;

  constructor(
    private dialog: MatDialog,
    private eventService: EventService,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private cdRef: ChangeDetectorRef,
    private loadStatusService: LoadStatusService,
  ) {
    this.form = this.fb.group({
      HostName: [localStorage.getItem('userName'), Validators.required],
      EventName: ['', Validators.required],
      Description: ['', Validators.required],
      DateScheduled: ['', Validators.required],
      Location: ['', Validators.required],
      subLocation: [''],
      attendances: [0, Validators.required],
    });
  }

  ngOnInit(): void {
    this.eventService.setPageNumber(1);
    this.eventService.events$.subscribe((events) => {
      this.events = events;
      this.cdRef.detectChanges();
    });
    window.addEventListener('scroll', () => {
      const scrollableHeight = document.documentElement.scrollHeight;
      const currentBottomPosition = window.scrollY + window.innerHeight;
      const threshold = 100;
      const distanceFromBottom = scrollableHeight - currentBottomPosition;
      const currentTime = new Date().getTime();
      if (
        distanceFromBottom <= threshold &&
        !this.allEventsLoaded &&
        !this.loadingEvents &&
        currentTime - this.lastLoadTime >= this.cooldown
      ) {
        this.loadMoreEvents();
        this.lastLoadTime = currentTime;
      }
    });
    this.route.parent!.paramMap.subscribe((params) => {
      this.currentUsername = params.get('username');
      this.isProfilePage = this.currentUsername != null;

      if (this.isProfilePage) {
        const role = localStorage.getItem('role');
        if (role == 'Admin' || role == 'Premium') {
          this.IfPre = true;
          this.isMine = true;
          this.GetMine();
        } else {
          this.GetOther();
        }
      } else {
        this.loadAllEvents();
      }
    });
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
        if (result) {
          return;
        } else {
          return;
        }
      });
      return;
    }
    this.HideEvents = true;
  }

  CloseForm() {
    this.HideEvents = false;
  }

  GetMine() {
    this.eventService.setPageNumber(1);
    this.isMine = true;
    this.isOther = false;
    const id = localStorage.getItem('userId')!;
    this.eventService.refreshEventsByUserName(id, false);
    this.eventService.RegerstState(this.isMine);
  }

  GetOther() {
    this.isMine = false;
    this.isOther = true;
    this.eventService.setPageNumber(1);
    const id = localStorage.getItem('userId')!;
    this.eventService.refreshEventsByUserName(id, true);
    this.eventService.RegerstState(this.isOther);
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

  AddEvent() {
    if (!this.selectedImage) {
      alert('Please select a picture.');
      return;
    }
    if (localStorage.getItem('emailConformed') === 'false') {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '1000px',
        data: {
          message: 'You need to confirm your email to activate your account',
        },
      });

      dialogRef.afterClosed().subscribe((result: boolean) => {
        if (result) {
          return;
        } else {
          return;
        }
      });

      return;
    }
    this.form.markAllAsTouched();
    if (this.form.valid) {
      const formData = new FormData();
      formData.append('eventName', this.form.value.EventName);
      formData.append('description', this.form.value.Description);
      formData.append('dateScheduled', this.form.value.DateScheduled);
      formData.append('location', this.form.value.Location);
      formData.append('subLocation', this.form.value.subLocation);
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
      this.eventService.postEvent(formData).subscribe({
        next: (response) => {
          this.eventService.refreshEvents();

          this.CloseForm();
        },
        error: (error) => {
          console.error('Error posting event:', error);
        },
      });
    }
  }

  private loadAllEvents(): void {
    this.eventService.refreshEvents();

    if (!this.allEventsLoaded && !this.loadingEvents) {
      this.eventService.refreshEvents();
      this.eventService.setPageNumber(this.eventService.getPageNumber() + 1);
    }
  }

  loadMoreEvents(): void {
    if (!this.allEventsLoaded && !this.loadingEvents) {
      if (
        this.loadStatusService.isSearching &&
        !this.loadStatusService.IsBook
      ) {
        this.eventService.refreshEventsByQuery(
          this.loadStatusService.currentQuery,
        );
      } else if (
        this.loadStatusService.currentCity &&
        !this.loadStatusService.IsBook
      ) {
        this.eventService.refreshEventsByCity(
          this.loadStatusService.currentCity,
        );
      } else {
        this.eventService.refreshEvents();
      }
      this.eventService.getAllEvents().subscribe(
        (events) => {
          this.eventService.setPageNumber(
            this.eventService.getPageNumber() + 1,
          );
          if (events.length < this.eventService.getPageSize()) {
            this.allEventsLoaded = true;
          }

          this.eventService.setPageNumber(
            this.eventService.getPageNumber() + 1,
          );
          if (this.currentUsername) {
            if (this.isMine) {
              this.eventService.refreshEventsByUserName(
                this.currentUsername,
                false,
              );
            } else {
              this.eventService.refreshEventsByUserName(
                this.currentUsername,
                true,
              );
            }
          } else {
            this.eventService.refreshEvents();
          }

          this.loadingEvents = false;
        },
        () => (this.loadingEvents = false),
      );
    }
  }

  get events$(): Observable<EventModel[]> {
    return this.eventService.events$;
  }

  handleCitySelect(city: string): void {
    if (city) {
      this.eventService.getEventsFilteredByCity(city).subscribe({
        next: (events) => (this.events = events),
        error: (err) => (this.events.length = 0),
      });
    } else {
      this.loadAllEvents();
    }
  }
}
