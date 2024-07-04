import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';
import { Book } from '../Models/bookModel';
import { AddBookPostLike } from '../Models/addBookPostLike';
import { Genre } from '../../Auth/Models/Genre';
import * as signalR from '@microsoft/signalr';
import { ConnectionsService } from '../../Auth/services/ConnectionService/connections.service';
import { GetGenreForUserModel } from '../Models/GenresForMyBook';
@Injectable({
  providedIn: 'root',
})
export class BookService {
  private booksSubject = new BehaviorSubject<Book[]>([]);
  books$ = this.booksSubject.asObservable();
  private currentBookSubject = new BehaviorSubject<Book | null>(null);
  private baseUrl = 'https://localhost:7139/api/Book';
  private hubConnection!: signalR.HubConnection;
  private pageNumber = 1;
  private pageSize = 10;
  private lastRefreshTime = 0;

  constructor(
    private http: HttpClient,
    private connectionsService: ConnectionsService,
  ) {
    this.registerBookEvents();
  }

  private registerBookEvents(): void {
    this.connectionsService.startConnection();
    this.hubConnection = this.connectionsService.HubConnection;
    this.hubConnection.on('BookAdded', (book: Book) => {
      const currentBooks = this.booksSubject.getValue();
      this.booksSubject.next([book, ...currentBooks]);
    });

    this.hubConnection.on('BookUpdated', (updatedBook: Book) => {
      const updatedBooks = this.booksSubject.getValue().map((book) => {
        if (book.id === updatedBook.id) {
          updatedBook.userName = book.userName;
          updatedBook.coverImage = book.coverImage;
          return updatedBook;
        }
        return book;
      });
      this.booksSubject.next(updatedBooks);
    });

    this.hubConnection.on('BookDeleted', (bookId: number) => {
      const updatedBooks = this.booksSubject
        .getValue()
        .filter((book) => book.id !== bookId);
      this.booksSubject.next(updatedBooks); // Removes the deleted book
    });
  }

  getAllBooks(): Observable<Book[]> {
    let params = new HttpParams()
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    return this.http.get<Book[]>(this.baseUrl + '/GetAllWithUserInfo', {
      params,
    });
  }

  getBookById(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  getMyBookGenres(userId: string): Observable<GetGenreForUserModel> {
    return this.http.get<GetGenreForUserModel>(
      `${this.baseUrl}/GetGenresForUser?userId=${userId}`,
    );
  }

  getRecommendedBooks(
    GenresForMyBooks: GetGenreForUserModel,
  ): Observable<any[]> {
    return this.http.post<any[]>(
      `http://127.0.0.1:5000/recommend`,
      GenresForMyBooks,
    );
  }
  GetBooksByIsbn(isbn: string[]): Observable<Book[]> {
    return this.http.post<Book[]>(`${this.baseUrl}/GetBooksByIsbn`, isbn);
  }

  getBooksByUserName(username: string): Observable<Book[]> {
    let params = new HttpParams()
      .set('username', username)
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    return this.http.get<Book[]>(`${this.baseUrl}/GetBooksByUserName`, {
      params,
    });
  }

  registerBook(formData: FormData): Observable<any> {
    return this.http.post<Book>(`${this.baseUrl}/post`, formData).pipe(
      tap((book) => {
        const currentBooks = this.booksSubject.getValue();
        this.booksSubject.next([...currentBooks, book]);
      }),
    );
  }

  registerBookWithGenres(formData: FormData): Observable<any> {
    return this.http.post<Book>(`${this.baseUrl}/PostBookWithGenres`, formData);
  }

  searchBooks(query: string): Observable<Book[]> {
    let params = new HttpParams()
      .set('query', query)
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    return this.http.get<Book[]>(`${this.baseUrl}/search`, { params });
  }
  refreshBooksByQuery(query: string): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 1000) {
      return;
    }
    this.lastRefreshTime = currentTime;
    this.searchBooks(query)
      .pipe(
        catchError((error) => {
          console.error('Error fetching books with query:', error);
          this.booksSubject.next([]);
          return of([]);
        }),
      )
      .subscribe((books) => {
        if (books.length > 0) {
          if (this.pageNumber === 1) {
            this.booksSubject.next(books);
          } else {
            const currentBooks = this.booksSubject.getValue();
            this.booksSubject.next([...currentBooks, ...books]);
          }
          this.pageNumber++;
        }
      });
  }

  getBooksFilteredByGenres(genreIds: number[]): Observable<Book[]> {
    let params = new HttpParams()
      .set('genreIds', genreIds.join(',')) // Assuming your backend can handle comma-separated IDs
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    return this.http.get<Book[]>(`${this.baseUrl}/filter`, { params });
  }
  refreshBooksByGenre(genreIds: number[]): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 1000) {
      return;
    }
    this.lastRefreshTime = currentTime;
    this.getBooksFilteredByGenres(genreIds)
      .pipe(
        catchError((error) => {
          console.error('Failed to load books:', error);
          this.booksSubject.next([]);
          return of([]);
        }),
      )
      .subscribe((books) => {
        if (books.length > 0) {
          if (this.pageNumber === 1) {
            this.booksSubject.next(books);
          } else {
            const currentBooks = this.booksSubject.getValue();
            this.booksSubject.next([...currentBooks, ...books]);
          }
          this.pageNumber++;
        }
      });
  }

  refreshBooks(): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 1000) {
      return;
    }
    this.lastRefreshTime = currentTime;
    this.getAllBooks().subscribe((books) => {
      this.booksSubject.next(
        this.pageNumber === 1
          ? books
          : [...this.booksSubject.getValue(), ...books],
      );
      this.pageNumber++;
    });
  }
  reportBook(id: number): Observable<Book> {
    const userId = localStorage.getItem('userId');
    const body = {
      Id: id,
      UserId: userId,
    };
    return this.http.post<Book>(`${this.baseUrl}/ReportBook`, body);
  }
  refreshBooksByUserName(username: string): void {
    if (this.pageNumber === 1) {
      this.booksSubject.next([]);
    }
    this.getBooksByUserName(username).subscribe((books) => {
      if (this.pageNumber === 1) {
        this.booksSubject.next(books);
      } else {
        const currentBooks = this.booksSubject.getValue();
        this.booksSubject.next([...currentBooks, ...books]);
      }
      this.pageNumber++;
    });
  }

  checkBookLike(data: AddBookPostLike): Observable<any> {
    const params = new HttpParams()
      .set('userId', data.userId)
      .set('bookId', data.bookId.toString());
    return this.http.get<any>(
      `https://localhost:7139/api/BookPostLike/getbyid`,
      { params },
    );
  }
  AddBookLike(data: AddBookPostLike): Observable<any> {
    return this.http.post(`https://localhost:7139/api/BookPostLike/post`, data);
  }
  removeBookLike(data: AddBookPostLike): Observable<any> {
    const params = new HttpParams()
      .set('userId', data.userId)
      .set('bookId', data.bookId.toString());
    return this.http.delete<any>(
      `https://localhost:7139/api/BookPostLike/delete`,
      { params },
    );
  }
  checkBulkLikes(
    userId: string,
    bookIds: number[],
  ): Observable<{ [key: number]: boolean }> {
    let params = new HttpParams().set('userId', userId);
    bookIds.forEach((id) => {
      params = params.append('bookIds', id.toString());
    });

    return this.http.get<{ [key: number]: boolean }>(
      `https://localhost:7139/api/BookPostLike/checkBulkLikes`,
      { params },
    );
  }
  DeleteBook(id: number): Observable<any> {
    if (localStorage.getItem('role') == 'Admin') {
      return this.http.delete(`${this.baseUrl}/DeleteByAdmin?bookId=${id}`);
    } else {
      return this.http.delete(`${this.baseUrl}/delete?bookId=${id}`).pipe(
        tap({
          next: () => {
            const updatedBooks = this.booksSubject
              .getValue()
              .filter((book) => book.id !== id);
            this.booksSubject.next(updatedBooks);
          },
          error: (error) => {
            console.error('Failed to delete the book:', error);
          },
        }),
      );
    }
  }

  getGenresForBook(bookId: number): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${this.baseUrl}/${bookId}/genres`);
  }
  setCurrentBook(book: Book): void {
    this.currentBookSubject.next(book);
  }
  getCurrentBook(): Observable<Book | null> {
    return this.currentBookSubject.asObservable();
  }

  updateBook(formData: FormData): Observable<any> {
    return this.http.put(`${this.baseUrl}/UpdateBookWithGenres`, formData);
  }
  SellBook(bookId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/SellBook?bookId=${bookId}`, {});
  }

  setPageSize(size: number): void {
    this.pageSize = size;
  }
  setPageNumber(page: number): void {
    this.pageNumber = page;
  }
  getPageSize(): number {
    return this.pageSize;
  }
  getPageNumber(): number {
    return this.pageNumber;
  }
}
