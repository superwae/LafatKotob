import { Component, OnInit } from '@angular/core';
import { RecommendationComponent } from '../../../shared/components/recommendation/recommendation.component';
import { SearchBarComponent } from '../../../shared/components/search-bar/search-bar.component';
import { BooksComponent } from '../../../Book/components/books/books.component';
import { BookService } from '../../../Book/Service/BookService';
import { Book } from '../../../Book/Models/bookModel';
import { MagicTextComponent } from '../magic-text/magic-text.component';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { CommonModule } from '@angular/common';
import { FilterComponent } from '../../../shared/components/filter/filter.component';
import { EventService } from '../../../Event/Service/event.service';
import { EventModel } from '../../../Event/Models/EventModels';
import { EventFilterComponent } from '../../../shared/components/event-filter/event-filter.component';
import { EventsComponent } from '../../../Event/components/events/events.component';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { LoadStatusService } from '../../../Book/Service/load-status.service';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    RecommendationComponent,
    SearchBarComponent,
    BooksComponent,
    MagicTextComponent,
    CommonModule,
    FilterComponent,
    EventFilterComponent,
    EventsComponent,
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
})
export class HomePageComponent implements OnInit {
  searchResults: Book[] = [];
  searchResultsEvents: EventModel[] = [];
  filteredBooks: Book[] = [];
  viewType: 'books' | 'events' = 'books';
  events: EventModel[] = [];
  filteredEvents: EventModel[] = [];
  pageNumber = 1;
  pageSize = 10;
  allEventsLoaded = false;
  loadingEvents = false;
  currentQuery: string = '';
  currentGenreId: number | null = null;
  currentCity: string | null = null;
  isSearching: boolean = false;
  isFiltering: boolean = false;
  isLoggedIn: boolean = false;
  private searchTerms = new Subject<string>();

  constructor(
    private bookService: BookService,
    private AppUsereService: AppUsereService,
    private eventService: EventService,
    private loadStatusService: LoadStatusService,
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.AppUsereService.isLoggedIn();
    this.loadAllBooks();
    this.searchTerms
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term: string) => {
          this.currentQuery = term;
          this.isSearching = true;

          this.isFiltering = false;
          this.loadStatusService.setSearchState(this.isSearching, term);
          this.bookService.setPageNumber(1);
          return this.bookService.searchBooks(term);
        }),
      )
      .subscribe((books) => {
        this.searchResults = books;
      });
  }

  handleSearch(query: string): void {
    if (this.viewType === 'events') {
      this.loadStatusService.setBookState(false);
    } else {
      this.loadStatusService.setBookState(true);
    }
    this.loadStatusService.setSearchState(true, query);
    this.loadStatusService.setCityState(false, null);
    this.loadStatusService.setFilterState(false, null);
    this.searchOrFilter();
  }

  handleFilterChange(filterType: 'books' | 'events'): void {
    this.viewType = filterType;
    if (filterType === 'books') {
      this.loadStatusService.setBookState(true);
      this.loadAllBooks();
    } else {
      this.loadStatusService.setBookState(false);
      this.loadAllEvents();
    }
  }

  handleGenreSelect(genreId: number | null): void {
    this.loadStatusService.setFilterState(true, genreId);
    this.loadStatusService.setCityState(false, null);
    this.loadStatusService.setSearchState(false, '');
    this.loadStatusService.setBookState(true);
    this.searchOrFilter();
  }

  private loadAllBooks(): void {
    this.bookService.refreshBooks();
  }
  private loadAllEvents(): void {
    this.eventService.refreshEvents();
  }

  handleCitySelect(city: string | null): void {
    if (!city) {
      this.eventService.refreshEvents();
      return;
    }
    this.currentCity = city;
    this.loadStatusService.setSearchState(false, '');
    this.loadStatusService.setBookState(false);
    this.loadStatusService.setFilterState(false, null);
    this.loadStatusService.setCityState(true, city);
    this.searchOrFilter();
  }

  searchOrFilter(): void {
    this.bookService.setPageNumber(1);
    this.eventService.setPageNumber(1);

    if (this.viewType === 'events') {
      if (this.loadStatusService.currentCity) {
        this.eventService.refreshEventsByCity(
          this.loadStatusService.currentCity,
        );
      } else if (
        this.loadStatusService.currentQuery &&
        !this.loadStatusService.IsBook
      ) {
        this.eventService.refreshEventsByQuery(
          this.loadStatusService.currentQuery,
        );
      }
    } else {
      if (this.loadStatusService.isSearching && this.loadStatusService.IsBook) {
        this.bookService.refreshBooksByQuery(
          this.loadStatusService.currentQuery,
        );
      } else if (this.loadStatusService.isFiltering) {
        const genreIds: number[] = [];
        genreIds.push(this.loadStatusService.currentGenreId!);
        this.bookService.refreshBooksByGenre(genreIds);
      }
    }
  }

  loadMoreItems(): void {
    this.bookService.setPageNumber(this.bookService.getPageNumber() + 1);
    this.searchOrFilter();
  }
}
