import { Component, Inject, OnInit } from '@angular/core';
import { Book } from '../../Models/bookModel';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService } from '../../Service/BookService';
import { CommonModule, DOCUMENT } from '@angular/common';
import { AppUserModel } from '../../../Auth/Models/AppUserModel';
import { AddBookPostLike } from '../../Models/addBookPostLike';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ConversationService } from '../../../conversation/Services/ConversationService/conversation.service';
import { ConversationWithIdsModel } from '../../../conversation/models/ConversationWithIdsModel';

@Component({
  selector: 'app-book-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './book-details.component.html',
  styleUrls: ['./book-details.component.css'],
})
export class BookDetailsComponent implements OnInit {
  book!: Book;
  user: AppUserModel | null = null;
  BookPostLike: AddBookPostLike | undefined;
  BookPostLikeId?: number;
  userId = localStorage.getItem('userId');
  userRole = localStorage.getItem('role');
  showDropdown: any;
  isLoggedIn = false;
  defaultImageUrl: string = 'assets/images/defaultCover.jpg';

  // Properties for handling the book cover image and alt text
  bookCoverImage: string = 'assets/images/defaultCover.jpg';
  bookCoverImageAlt: string = 'Default book cover image';

  cities: string[] = [
    'Ramallah',
    'Jerusalem',
    'Gaza',
    'Nablus',
    'Hebron',
    'Jenin',
    'Tulkarm',
    'Qalqilya',
    'Bethlehem',
    'Jericho',
    'Tubas',
    'Salfit',
  ];

  constructor(
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private bookService: BookService,
    private router: Router,
    private conversationService: ConversationService,
    @Inject(DOCUMENT) private document: Document,
  ) {}

  ngOnInit(): void {
    this.document.body.classList.add('hide-scrollbar');

    if (localStorage.getItem('token')) {
      this.isLoggedIn = true;
    }

    const bookId = this.route.snapshot.params['id'];
    const userId = localStorage.getItem('userId');
    this.bookService.getBookById(bookId).subscribe({
      next: (data) => {
        this.book = data;
        this.setBookCoverImage();

        if (userId) {
          this.BookPostLike = {
            bookId: +bookId,
            userId: userId,
            dateLiked: new Date(),
          };
          this.bookService
            .checkBookLike(this.BookPostLike)
            .subscribe((isLiked) => {
              this.book.isLikedByCurrentUser = true;
            });
        }
      },
      error: (err) => {
        console.error(err);
      },
    });
  }

  setBookCoverImage() {
    if (this.book.coverImage) {
      this.bookCoverImage = this.book.coverImage;
      this.bookCoverImageAlt = `Cover image of ${this.book.title}`;
    }
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/defaultCover.jpg';
    this.bookCoverImageAlt = 'Default book cover image';
  }

  // Other methods ...

  checkBookLikeStatus(): void {
    const userId = localStorage.getItem('userId');
    if (userId && this.book && this.book.id !== undefined) {
      this.bookService
        .checkBulkLikes(userId, [this.book.id])
        .subscribe((isLikedMap) => {
          if (this.book) {
            this.book.isLikedByCurrentUser = !!isLikedMap[this.book.id];
          }
        });
    }
  }

  onLikeBook(bookId: number, event: MouseEvent): void {
    event.stopPropagation();
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
    const userId = localStorage.getItem('userId');

    if (!userId) {
      console.error('User must be logged in to like a book');
      return;
    }

    if (this.book.isLikedByCurrentUser) {
      const bookPostLikeData: AddBookPostLike = {
        bookId: bookId,
        userId: userId,
        dateLiked: new Date(),
      };

      this.bookService.removeBookLike(bookPostLikeData).subscribe({
        next: (data) => {
          this.book.isLikedByCurrentUser = false;
        },
        error: (err) => {
          console.error(err);
        },
      });
    } else {
      const bookPostLikeData: AddBookPostLike = {
        bookId: bookId,
        userId: userId,
        dateLiked: new Date(),
      };

      this.bookService.AddBookLike(bookPostLikeData).subscribe({
        next: (data) => {
          this.book.isLikedByCurrentUser = true;
        },
        error: (err) => {
          console.error(err);
        },
      });
    }

    this.book.isLikedByCurrentUser = !this.book.isLikedByCurrentUser;
  }

  onOpenChat(userId: string, event: MouseEvent): void {
    event.stopPropagation();
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
    const conversationwithIds: ConversationWithIdsModel = {
      userIds: [this.userId!, userId],
      LastMessage: '',
      LastMessageDate: new Date(),
      lastMessageSender: this.userId!,
    };
    this.conversationService.createConversation(conversationwithIds);
    this.router.navigate(['/conversations']);
  }

  editBook(event: MouseEvent): void {
    event.stopPropagation();
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
    this.router.navigate(['/edit-book', this.book.id]);
  }

  deleteBook(bookId: number, event: MouseEvent) {
    event.stopPropagation();
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
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '500px',
      data: { message: 'Are you sure you want to delete this book?' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.bookService.DeleteBook(bookId).subscribe({
          next: () => {
            this.router.navigate(['/home']);
          },
          error: (err) => {
            console.error(err);
          },
        });
      } else {
      }
    });
  }

  close(): void {
    this.router.navigate(['/']);
  }

  reportBook(bookId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '500px',
      data: { message: 'Are you sure you want to Report this book?' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.bookService.reportBook(bookId).subscribe({
          next: () => {},
          error: (err) => {
            console.error(err);
          },
        });
      } else {
      }
    });
  }

  sellBook(book: any, event: MouseEvent): void {
    event.stopPropagation();
    if (book.partnerUserId === this.userId) {
      return;
    }
    book.partnerUserId = this.userId;
    this.bookService.SellBook(book.id).subscribe({
      next: (response) => {},
      error: (error) => {
        console.error('Failed to sell the book:', error);
      },
    });
  }
  onImageLoad(event: any): void {
    const img: HTMLImageElement = event.target;

    // Check if the image is too small
    if (img.naturalWidth <= 1 || img.naturalHeight <= 1) {
      img.src = this.defaultImageUrl;
      return;
    }

    // Use a canvas to check if the image is blank
    const canvas = document.createElement('canvas');
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    const ctx = canvas.getContext('2d');
    ctx!.drawImage(img, 0, 0);

    const imageData = ctx!.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData.data;
    let isBlank = true;

    for (let i = 0; i < data.length; i += 4) {
      if (data[i] !== 255 || data[i + 1] !== 255 || data[i + 2] !== 255) {
        // not white
        isBlank = false;
        break;
      }
    }

    if (isBlank) {
      img.src = this.defaultImageUrl;
    }
  }
  ngOnDestroy(): void {
    this.document.body.classList.remove('hide-scrollbar');
  }

  languages = ['English', 'Spanish', 'French', 'German', 'Chinese']; // Example array
}
