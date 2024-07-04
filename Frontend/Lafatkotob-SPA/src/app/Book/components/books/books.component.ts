import { WhoseProfileService } from './../../../shared/Service/WhoseProfileService/whose-profile.service';
import {
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { BookService } from '../../Service/BookService';
import { Book } from '../../Models/bookModel';
import { CommonModule } from '@angular/common';
import {
  ActivatedRoute,
  NavigationEnd,
  Router,
  UrlSegment,
} from '@angular/router';
import { BookComponent } from '../book/book.component';
import { Observable, Subscription, filter } from 'rxjs';
import { LoadStatusService } from '../../Service/load-status.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [CommonModule, BookComponent, RouterModule],
  templateUrl: './books.component.html',
  styleUrl: './books.component.css',
})
export class BooksComponent implements OnInit, OnDestroy, OnChanges {
  @Input() books: Book[] | null = null;
  allBooksLoaded = false;
  loadingBooks = false;
  currentUsername: string | null = null;
  stopOther: boolean = false;

  private booksSubscription: Subscription = new Subscription();

  constructor(
    private bookService: BookService,
    private route: ActivatedRoute,
    private cdRef: ChangeDetectorRef,
    private WhoseProfileService: WhoseProfileService,
    private loadStatusService: LoadStatusService,
    private router: Router,
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['book']) {
    }
  }

  ngOnInit(): void {
    this.bookService.setPageNumber(1);
    this.route.url.subscribe((segments: UrlSegment[]) => {
      const hasUserSegment = segments.some(
        (segment) => segment.path === 'user',
      );
    });

    this.bookService.books$.subscribe((books) => {
      this.books = books;
      this.cdRef.detectChanges();
    });
    window.addEventListener('scroll', () => {
      const scrollableHeight = document.documentElement.scrollHeight;
      const currentBottomPosition = window.scrollY + window.innerHeight;
      const threshold = 100;
      const distanceFromBottom = scrollableHeight - currentBottomPosition;
      const urlContainsUser = this.router.url.includes('user');
      if (
        distanceFromBottom <= threshold &&
        !this.allBooksLoaded &&
        !this.loadingBooks &&
        !urlContainsUser
      ) {
        if (urlContainsUser) {
          this.bookService.refreshBooksByUserName(
            this.WhoseProfileService.otherUserName,
          );
        } else {
          this.loadMoreBooks();
        }
      }
    });

    this.route.parent!.paramMap.subscribe((params) => {
      if (this.WhoseProfileService.isOtherProfile) {
        this.currentUsername = this.WhoseProfileService.otherUserName;
        this.WhoseProfileService.isOtherProfile = false;
      } else {
        this.currentUsername = params.get('username');
      }

      if (this.currentUsername != null) {
        this.bookService.refreshBooksByUserName(this.currentUsername);
      } else {
        this.bookService.refreshBooks();
      }
    });

    this.bookService.books$.subscribe((books) => {
      this.checkBooksLikeStatus(books);
    });
  }

  loadMoreBooks(): void {
    if (!this.allBooksLoaded && !this.loadingBooks) {
      if (this.loadStatusService.isSearching && this.loadStatusService.IsBook) {
        this.bookService.refreshBooksByQuery(
          this.loadStatusService.currentQuery,
        );
        this.loadingBooks = false;
      } else if (this.loadStatusService.isFiltering) {
        this.bookService.refreshBooksByGenre([
          this.loadStatusService.currentGenreId!,
        ]);
      } else {
        this.bookService.refreshBooks();
      }
    }
  }

  loadBooks(): void {
    this.loadingBooks = true;
    this.bookService.getAllBooks().subscribe(
      (books) => {
        if (books.length < this.bookService.getPageSize()) {
          this.allBooksLoaded = true;
        }

        this.bookService.refreshBooks();

        this.bookService.setPageNumber(this.bookService.getPageNumber() + 1);
        this.loadingBooks = false;
      },
      () => {
        this.loadingBooks = false; // Ensure loading is reset even on error
      },
    );
  }

  checkBooksLikeStatus(books: Book[]): void {
    const userId = localStorage.getItem('userId');
    if (userId && books.length > 0) {
      const bookIds = books.map((book) => book.id);
      this.bookService
        .checkBulkLikes(userId, bookIds)
        .subscribe((isLikedMap) => {
          books.forEach((book) => {
            book.isLikedByCurrentUser = !!isLikedMap[book.id];
          });
        });
    }
  }

  get books$(): Observable<Book[]> {
    return this.bookService.books$;
  }

  trackByBook(index: number, book: Book): number {
    return book.id;
  }

  ngOnDestroy(): void {
    if (this.booksSubscription) {
      this.booksSubscription.unsubscribe();
    }
  }
}
