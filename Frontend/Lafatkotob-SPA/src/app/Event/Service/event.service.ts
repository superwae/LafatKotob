import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, of, tap } from 'rxjs';
import { EventModel } from '../Models/EventModels';
import { UserEventModel } from '../Models/userEventModel';
import { ConnectionsService } from '../../Auth/services/ConnectionService/connections.service';
import { AppUserModel } from '../../Auth/Models/AppUserModel';
@Injectable({
  providedIn: 'root',
})
export class EventService {
  private eventsSubject = new BehaviorSubject<EventModel[]>([]);
  events$ = this.eventsSubject.asObservable();
  private baseUrl = 'https://localhost:7139/api/Event';
  private hubConnection!: signalR.HubConnection;
  private RegerstStateVar = true;
  private lastRefreshTime = 0;
  private pageNumber = 1;
  private pageSize = 20;

  constructor(
    private http: HttpClient,
    private connectionsService: ConnectionsService,
  ) {
    this.registerEvents();
  }

  private registerEvents(): void {
    this.connectionsService.startConnection();
    this.hubConnection = this.connectionsService.HubConnection;
    this.hubConnection.on('EventAdded', (event: EventModel) => {
      const currentevents = this.eventsSubject.getValue();
      this.eventsSubject.next([event, ...currentevents]);
    });

    this.hubConnection.on('EventUpdated', (updatedevent: EventModel) => {
      const updatedBooks = this.eventsSubject.getValue().map((event) => {
        if (event.id === updatedevent.id) {
          return updatedevent;
        }
        return event;
      });
      this.eventsSubject.next(updatedBooks);
    });

    this.hubConnection.on('EventDeleted', (eventId: number) => {
      const updatedEvents = this.eventsSubject
        .getValue()
        .filter((event) => event.id !== eventId);
      this.eventsSubject.next(updatedEvents);
    });
  }

  DeleteAllRelationAndEvent(eventId: number): Observable<any> {
    return this.http.delete(
      `${this.baseUrl}/DeleteAllRelationAndEvent?id=${eventId}`,
    );
  }

  getAllEvents(): Observable<EventModel[]> {
    let params = new HttpParams()
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());
    return this.http.get<EventModel[]>(`${this.baseUrl}/getall`, { params });
  }
  //the id of event
  getEventById(id: number): Observable<EventModel> {
    return this.http.get<EventModel>(`${this.baseUrl}/getbyid?EventId=${id}`);
  }

  getEventsByUserName(
    username: string,
    pageNumber: number = 1,
    pageSize: number = 20,
    GetRegisterd: boolean = true,
  ): Observable<EventModel[]> {
    let params = new HttpParams()
      .set('username', username)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString())
      .set('GetRegisterd', GetRegisterd.toString());

    return this.http.get<EventModel[]>(`${this.baseUrl}/GetEventsByUserName`, {
      params,
    });
  }
  getEventByHostId(HostId: string): Observable<EventModel[]> {
    return this.http.get<EventModel[]>(`${this.baseUrl}/GetEventsByHostId`, {
      params: { HostId },
    });
  }
  //the user that regested in it
  GetEventsByUserId(userId: string): Observable<EventModel[]> {
    return this.http.get<EventModel[]>(`${this.baseUrl}/GetEventsByUserId?`, {
      params: { userId },
    });
  }

  getUserEvent(UserId: string, EventId: number): Observable<EventModel> {
    const params = new HttpParams()
      .set('UserId', UserId.toString())
      .set('EventId', EventId);
    return this.http.get<EventModel>(`${this.baseUr2}/GetUserEvent?`, {
      params: { UserId, EventId },
    });
  }

  postEvent(formData: FormData): Observable<any> {
    return this.http.post(`${this.baseUrl}/post`, formData);
  }

  updateEvent(eventId: number, eventData: FormData): Observable<any> {
    return this.http.put(`${this.baseUrl}/update/${eventId}`, eventData);
  }

  deleteEvent(eventId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/delete?eventId=${eventId}`);
  }

  private baseUr2 = 'https://localhost:7139/api/UserEvent';

  postUserEvent(UserEventModel: UserEventModel): Observable<any> {
    this.RegerstStateVar = true;
    UserEventModel.isRegested = true;
    return this.http.post(`${this.baseUr2}/PostUserEvent`, UserEventModel);
  }

  DeleteUserEvent(EventId: number, UserId: string): Observable<any> {
    this.RegerstStateVar = false;

    const params = new HttpParams()
      .set('EventId', EventId.toString())
      .set('UserId', UserId);
    return this.http.delete(`${this.baseUr2}/DeleteUserEventByUserId`, {
      params,
    });
  }

  RegerstState(bool: boolean): void {
    this.RegerstStateVar = bool;
  }
  isRegester(): boolean {
    return this.RegerstStateVar;
  }

  getEventsFilteredByCity(city: string): Observable<EventModel[]> {
    let params = new HttpParams()
      .set('city', city)
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    return this.http.get<EventModel[]>(`${this.baseUrl}/ByEventCity`, {
      params,
    });
  }
  refreshEventsByCity(city: string): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 500) {
      return;
    }
    this.lastRefreshTime = currentTime;

    this.getEventsFilteredByCity(city)
      .pipe(
        catchError((error) => {
          this.eventsSubject.next([]);
          return of([]);
        }),
      )
      .subscribe((events) => {
        if (events.length > 0) {
          if (this.pageNumber === 1) {
            this.eventsSubject.next(events);
          } else {
            const currentEvents = this.eventsSubject.getValue();
            this.eventsSubject.next([...currentEvents, ...events]);
          }
          this.pageNumber++;
        }
      });
  }

  refreshEvents(): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 500) {
      return;
    }
    this.lastRefreshTime = currentTime;

    this.getAllEvents()
      .pipe(
        catchError((error) => {
          console.error('Error fetching all events:', error);
          this.eventsSubject.next([]); // Setting the subject to an empty array
          return of([]); // Return an observable with an empty array
        }),
      )
      .subscribe((events) => {
        if (this.pageNumber === 1) {
          this.eventsSubject.next(events);
        } else {
          const currentEvents = this.eventsSubject.getValue();
          this.eventsSubject.next([...currentEvents, ...events]);
        }
        this.pageNumber++;
      });
  }
  refreshEventsByUserName(
    username: string,
    GetRegistered: boolean = true,
  ): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 500) {
      return;
    }
    this.lastRefreshTime = currentTime;

    this.getEventsByUserName(
      username,
      this.pageNumber,
      this.pageSize,
      GetRegistered,
    )
      .pipe(
        catchError((error) => {
          console.error('Error fetching events by username:', error);
          this.eventsSubject.next([]); // Setting the subject to an empty array
          return of([]); // Return an observable with an empty array
        }),
      )
      .subscribe((events) => {
        if (this.pageNumber === 1) {
          this.eventsSubject.next(events);
        } else {
          const currentEvents = this.eventsSubject.getValue();
          this.eventsSubject.next([...currentEvents, ...events]);
        }
        this.pageNumber++;
      });
  }

  searchEvents(query: string): Observable<EventModel[]> {
    let params = new HttpParams()
      .set('query', query)
      .set('pageNumber', this.pageNumber.toString())
      .set('pageSize', this.pageSize.toString());

    return this.http.get<EventModel[]>(`${this.baseUrl}/search`, { params });
  }
  refreshEventsByQuery(query: string): void {
    const currentTime = Date.now();
    const timeSinceLastRefresh = currentTime - this.lastRefreshTime;

    if (timeSinceLastRefresh < 500) {
      return;
    }
    this.lastRefreshTime = currentTime;
    this.searchEvents(query)
      .pipe(
        catchError((error) => {
          console.error('Error searching events with query:', error);
          this.eventsSubject.next([]);
          return of([]);
        }),
      )
      .subscribe((events) => {
        if (events.length > 0) {
          if (this.pageNumber === 1) {
            this.eventsSubject.next(events);
          } else {
            const currentEvents = this.eventsSubject.getValue();
            this.eventsSubject.next([...currentEvents, ...events]);
          }
          this.pageNumber++;
        }
      });
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
  getUserByEventId(eventId: number): Observable<AppUserModel> {
    const params = new HttpParams()
      .set('EventId', eventId);
    return this.http.get<AppUserModel>(`${this.baseUrl}/GetUserByEvent?`, {
      params: { eventId },
    });
  }
}
