import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LoadStatusService {
  isSearching: boolean = false;
  isFiltering: boolean = false;
  isCityFiltering: boolean = false;
  currentQuery: string = '';
  currentCity: string | null = null;
  currentGenreId: number | null = null;
  IsBook: boolean = true;

  setSearchState(isSearching: boolean, query: string): void {
    this.isSearching = isSearching;
    this.currentQuery = query;
  }
  setBookState(isBook: boolean): void {
    this.IsBook = isBook;
  }

  setFilterState(isFiltering: boolean, genreId: number | null): void {
    this.isFiltering = isFiltering;
    this.currentGenreId = genreId;
  }

  setCityState(isCityFiltering: boolean, city: string | null): void {
    this.isCityFiltering = isCityFiltering;
    this.currentCity = city;
  }
}
