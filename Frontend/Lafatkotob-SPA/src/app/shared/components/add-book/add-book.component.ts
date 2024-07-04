import { Component, OnInit } from '@angular/core';
import { BookService } from '../../../Book/Service/BookService';
import { CommonModule } from '@angular/common';
import { ModalComponent } from '../modal/modal.component';
import { ModaleService } from '../../Service/ModalService/modal.service';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-add-book',
  standalone: true,
  imports: [CommonModule, ModalComponent],
  templateUrl: './add-book.component.html',
  styleUrls: ['./add-book.component.css'],
})
export class AddBookComponent implements OnInit {
  showModal: boolean = false;
  emailConformed: boolean = true;

  constructor(
    private dialog: MatDialog,
    private modalService: ModaleService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.emailConformed = localStorage.getItem('emailConformed') === 'true';
  }

  shouldHideButton(): boolean {
    const currentRoute = this.router.url;
    return (
      [
        '/login',
        '/forgot-password',
        '/reset-password',
        '/conversations',
        '/wishlist',
        '/events',
        '/user-settings',
        '/userPreferences',
        '/adminpage',
        '/contact-us',
      ].some((route) => currentRoute.includes(route)) ||
      this.showModal ||
      !this.emailConformed
    );
  }

  openRegisterBookModal() {
    if (!this.emailConformed) {
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
          console.log('Deletion canceled.');
          return;
        }
      });
      return;
    }

    this.showModal = true;
    this.modalService.setShowModal(true);
  }

  closeModal() {
    this.showModal = false;
    this.modalService.setShowModal(false);
  }
}
