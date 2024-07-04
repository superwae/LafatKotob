import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { BookInWishList } from '../Models/BookInWishList';

@Injectable({
  providedIn: 'root',
})
export class WishListService {
  constructor(private http: HttpClient) {}

  private baseUrl = 'https://localhost:7139/api/WishList';
  getWishListByUserId(userId: string): Observable<BookInWishList[]> {
    return this.http.get<BookInWishList[]>(`${this.baseUrl}/getbyidUser`, {
      params: new HttpParams().set('userId', userId),
    });
  }

  private baseUrl2 = 'https://localhost:7139/api/BooksInWishlists';
  deleteBookFromWishList(bookId: number): Observable<void> {
    // return this.http.delete<void>(`${this.baseUrl2}/Delete/${bookId}`);

    return this.http.delete<void>(this.baseUrl2 + '/delete/', {
      params: new HttpParams().set('BooksInWishlistsId', bookId),
    });
  }
  private baseUrl3 = 'https://localhost:7139/api/WishedBook';
  deleteBookFromWishedBook(bookId: number): Observable<void> {
    // return this.http.delete<void>(`${this.baseUrl3}/Delete/${bookId}`);

    return this.http.delete<void>(this.baseUrl3 + '/delete/', {
      params: new HttpParams().set('wishedBookId', bookId),
    });
  }

  addBookToBooksInWishList(book: BookInWishList, Id: string): Observable<void> {
    return this.http.post<void>(this.baseUrl2 + '/postAll', book, {
      params: new HttpParams().set('UserId', Id),
    });
  }
}
