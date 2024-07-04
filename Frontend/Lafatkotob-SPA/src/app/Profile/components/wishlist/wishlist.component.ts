import { Component, OnInit } from '@angular/core';
import { MyTokenPayload } from '../../../shared/Models/MyTokenPayload';
import { jwtDecode } from 'jwt-decode';
import { BookInWishList } from '../../Models/BookInWishList';
import { CommonModule } from '@angular/common';
import { WishListService } from '../../Service/wish-list.service';
import {
  FormGroup,
  ReactiveFormsModule,
  FormBuilder,
  Validators,
} from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TooltipDirective } from '../../../shared/directives/tooltip.directive';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TooltipDirective],
  templateUrl: './wishlist.component.html',
  styleUrls: ['./wishlist.component.css'],
})
export class wishlistComponent implements OnInit {
  Books: BookInWishList[] | null = null;
  form: FormGroup;
  showForm: boolean = false;
  languages: string[] = [
    'English',
    'Spanish',
    'French',
    'German',
    'Chinese',
    'Japanese',
    'Arabic',
    'Russian',
    'Portuguese',
    'Hindi',
  ]; // List of languages

  constructor(
    private dialog: MatDialog,
    private WishListService: WishListService,
    private fb: FormBuilder,
  ) {
    this.form = this.fb.group({
      Name: ['', Validators.required],
      author: ['', Validators.required],
      isbn: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      Language: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    const userId = localStorage.getItem('userId');
    if (userId) {
      this.WishListService.getWishListByUserId(userId).subscribe({
        next: (data) => {
          this.Books = data;
        },
        error: (err) => {
          console.error('Error fetching books:', err);
        },
      });
    }
  }

  getUserInfoFromToken(): MyTokenPayload | undefined {
    const token = localStorage.getItem('token');
    if (token) {
      return jwtDecode<MyTokenPayload>(token);
    }
    return undefined;
  }

  deleteBook(book: BookInWishList): void {
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

    const dialogRefDelete = this.dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: {
        message: 'Are you sure ?',
      },
    });

    dialogRefDelete.afterClosed().subscribe((result: boolean) => {
      if (!result) {
        return;
      }

      else {
        if (this.Books != null) {
          const index = this.Books.indexOf(book);
          if (index !== -1) {
            this.Books.splice(index, 1);
            this.WishListService.deleteBookFromWishedBook(book.id).subscribe(
              (b) => {
                this.Books;
              },
            );
            this.WishListService.deleteBookFromWishList(book.id).subscribe((b) => {
              this.Books;
            });
          }
        }
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
        } else {
          return;
        }
      });
      return;
    }
    this.showForm = true;
  }

  CloseForm() {
    this.showForm = false;
  }

  AddBook() {
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
      const formData = this.form.value;
      const book1: BookInWishList = {
        title: formData.Name,
        author: formData.author,
        id: 0,
        isbn: formData.isbn,
        language: formData.Language,
        addedDate: new Date(),
      };

      if (book1 != null) {
        var id = localStorage.getItem('userId');
        this.WishListService.addBookToBooksInWishList(book1!, id!).subscribe(
          (b) => {
            this.Books;
          },
        );
        if (this.Books != null) this.Books.push(book1);
      }
      this.CloseForm();
    }
  }
}
