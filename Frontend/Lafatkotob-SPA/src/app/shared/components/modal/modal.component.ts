import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { ModaleService } from '../../Service/ModalService/modal.service';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { AppUserModel } from '../../../Auth/Models/AppUserModel';
import { ModalGenreComponent } from '../modal-genre/modal-genre.component';
import { TooltipDirective } from '../../directives/tooltip.directive';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ModalGenreComponent,
    TooltipDirective,
  ],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.css'],
})
export class ModalComponent implements OnInit {
  @Input() title: string = '';
  @Input() show: boolean = false;
  @Output() closeEvent = new EventEmitter<void>();
  bookForm!: FormGroup;
  showGenreSelection: boolean = false;
  registrationData: any;
  showModalGenre: boolean = false;
  isLookingFor: boolean = false;
  userid: string | null = localStorage.getItem('userId');

  constructor(
    private modalService: ModaleService,
    private fb: FormBuilder,
    private userService: AppUsereService,
  ) {}

  ngOnInit() {
    if (localStorage.getItem('token') && this.userid) {
      this.userService.getUserById(this.userid).subscribe({
        next: (user: AppUserModel) => {
          this.initializeForm(user.historyId);
        },
        error: (error) => {
          console.error('Error fetching user info', error);
          this.initializeForm();
        },
      });
    } else {
      this.initializeForm();
    }
  }
  initializeForm(historyId?: number) {
    this.bookForm = this.fb.group({
      Title: ['', Validators.required],
      Author: ['', Validators.required],
      Description: ['', Validators.required],
      CoverImage: [null],
      UserId: [localStorage.getItem('userId')],
      HistoryId: [historyId],
      PublicationDate: ['', Validators.required],
      ISBN: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      PageCount: [null, Validators.required],
      Condition: ['', Validators.required],
      Status: ['Available', Validators.required],
      Type: ['', Validators.required],
      PartnerUserId: ['c98dd230-e629-4325-8194-5157a5d39423'],

      Language: ['', Validators.required],
      AddedDate: [new Date().toISOString()],
    });
  }

  async register() {
    this.bookForm.markAllAsTouched();
    if (this.bookForm.valid) {
      this.proceedWithRegistration();
    } else {
      console.error('Form is not valid');
    }
  }

  onTypeChange(event: any): void {
    this.isLookingFor = event.target.value === 'buy';
    if (!this.isLookingFor) {
      this.bookForm.get('Condition')!.reset();
      this.bookForm.get('Condition')!.setValidators(Validators.required);
      this.bookForm.get('Condition')!.updateValueAndValidity();
    } else {
      // Set to any if buying (example)
      this.bookForm.get('Condition')!.setValue('Any');
      this.bookForm.get('Condition')!.clearValidators();
      this.bookForm.get('Condition')!.updateValueAndValidity();
    }
  }

  async proceedWithRegistration() {
    const formData = new FormData();

    Object.keys(this.bookForm.value).forEach((key) => {
      formData.append(key, this.bookForm.value[key]);
    });
    this.registrationData = formData;
    this.showGenreSelection = true;
  }

  handleProceedToGenreSelection(data: any) {
    this.registrationData = data;
    this.showModalGenre = true;
  }
  handleGenreModalClose() {
    this.showModalGenre = false;
  }
  close() {
    this.show = false;
    this.modalService.setShowModal(false);
    this.closeEvent.emit();
    this.resetForm();
  }
  closeEvent2(): void {
    this.close();
  }
  onAddAnotherBook(): void {
    this.showGenreSelection = false;
    this.show = true;
    this.resetForm();
  }
  onClosePopup(): void {
    this.showGenreSelection = false;
    this.show = false;
    this.modalService.setShowModal(false);
    this.closeEvent.emit();
  }

  resetForm() {
    const userId = this.bookForm.get('UserId')?.value;
    const historyId = this.bookForm.get('HistoryId')?.value;
    const PartnerUserId = this.bookForm.get('PartnerUserId')?.value;
    this.bookForm.reset({
      Title: '',
      Author: '',
      Description: '',
      CoverImage: null,
      PublicationDate: '',
      ISBN: '',
      PageCount: '',
      Condition: '',
      Status: 'Available',
      Type: '',
      PartnerUserId: '',
      Language: '',
      AddedDate: new Date().toISOString(),
    });
    this.isLookingFor = false;
    this.bookForm.patchValue({
      UserId: userId,
      HistoryId: historyId,
      PartnerUserId: PartnerUserId,
    });
  }
}
