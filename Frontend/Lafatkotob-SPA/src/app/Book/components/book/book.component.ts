import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  HostListener,
  Input,
  OnInit,
} from '@angular/core';
import { Book } from '../../Models/bookModel';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService } from '../../Service/BookService';
import { AddBookPostLike } from '../../Models/addBookPostLike';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { WhoseProfileService } from '../../../shared/Service/WhoseProfileService/whose-profile.service';
import { ConversationWithIdsModel } from '../../../conversation/models/ConversationWithIdsModel';
import { ConversationService } from '../../../conversation/Services/ConversationService/conversation.service';

@Component({
  selector: 'app-book',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './book.component.html',
  styleUrls: ['./book.component.css'],
})
export class BookComponent implements OnInit {
  @Input() book!: Book;
  BookPostLike: AddBookPostLike | undefined;
  BookPostLikeId?: number;
  isOtherPro: any = false;
  otherUserId = '';
  showOptionsMap: { [key: string]: boolean } = {};
  userId = localStorage.getItem('userId');
  userRole = localStorage.getItem('role');
  isProcessing: boolean = false;
  isLoggedIn = false;
  defaultImageUrl: string = 'assets/images/defaultCover.jpg';
  coverImageUrl: string = '';

  constructor(
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private bookService: BookService,
    private router: Router,
    private cdRef: ChangeDetectorRef,
    private WhoseProfileService: WhoseProfileService,
    private conversationService: ConversationService,
  ) {}

  ngOnInit(): void {
    if (localStorage.getItem('userName') !== null) {
      this.isLoggedIn = true;
    }
    this.coverImageUrl = this.book.coverImage || this.defaultImageUrl;
    const bookId = this.route.snapshot.params['id'];
    const userId = localStorage.getItem('userId');

    if (bookId) {
      this.bookService.getBookById(bookId).subscribe({
        next: (data) => {
          this.book = data;
          this.coverImageUrl = this.book.coverImage || this.defaultImageUrl;

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

    // Toggle the like status
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
      LastMessage: null,
      LastMessageDate: new Date(),
      lastMessageSender: this.userId!,
    };
    this.conversationService.createConversation(conversationwithIds).subscribe({
      next: () => {
        this.router.navigate(['/conversations']);
      },
    });
  }

  toggleOptions(bookId: number, event: MouseEvent): void {
    event.stopPropagation();
    this.showOptionsMap[bookId] = !this.showOptionsMap[bookId];
  }

  isOptionsShown(bookId: number): boolean {
    return !!this.showOptionsMap[bookId];
  }

  editBook(event: MouseEvent) {
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

    this.bookService.setCurrentBook(this.book);
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
          next: () => {},
          error: (err) => {
            console.error(err);
          },
        });
      } else {
      }
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    let isOpen = false;

    // Check if any dropdown is open
    Object.keys(this.showOptionsMap).forEach((key) => {
      if (this.showOptionsMap[key]) {
        isOpen = true;
      }
    });

    // If any dropdown is open, close them
    if (isOpen) {
      this.closeOptions();
    }
  }

  closeOptions(): void {
    Object.keys(this.showOptionsMap).forEach((key) => {
      this.showOptionsMap[key] = false;
    });
    this.cdRef.detectChanges(); // Ensure Angular updates the view
  }

  reportBook(bookId: number, event: MouseEvent): void {
    // Implementation to report book
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '500px',
      data: { message: 'Are you sure you want to Report this book?' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        event.stopPropagation();
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
    this.isProcessing = true;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: {
        message: `Are you sure you want to sell '${book.title}'? Please confirm to finalize this sale.`,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.bookService.SellBook(book.id).subscribe({
          next: (response) => {
            book.partnerUserId = this.userId;
            this.isProcessing = false;
          },
          error: (error) => {
            console.error('Failed to sell the book:', error);
            this.isProcessing = false;
          },
        });
      } else {
        this.isProcessing = false;
      }
    });
  }

  buyBook(book: any, event: MouseEvent): void {
    event.stopPropagation();

    if (book.partnerUserId === this.userId) {
      return;
    }
    this.isProcessing = true;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '1000px',
      data: {
        message: `Have you completed the purchase of '${book.title}'? Confirming will remove this post.`,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.bookService.SellBook(book.id).subscribe({
          next: (response) => {
            book.partnerUserId = this.userId;
            this.isProcessing = false;
          },
          error: (error) => {
            console.error('Failed to sell the book:', error);
            this.isProcessing = false;
          },
        });
      } else {
        this.isProcessing = false;
      }
    });
  }

  GetInfoForUserProfile(isOther: boolean, userName: string) {
    this.WhoseProfileService.isOtherProfile = isOther;
    this.WhoseProfileService.otherUserName = userName;
  }

  onImageError(event: any): void {
    event.target.src = this.defaultImageUrl;
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
}
