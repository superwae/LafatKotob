import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  catchError,
  map,
  tap,
  throwError,
} from 'rxjs';
import { MessageModel } from '../../models/Message';
import * as signalR from '@microsoft/signalr';
import {
  ConversationModel,
  ConversationsBoxModel,
} from '../../models/ConversationModels';
import { ConversationWithIdsModel } from '../../models/ConversationWithIdsModel';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { ConnectionsService } from '../../../Auth/services/ConnectionService/connections.service';

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  private conversationsSubject = new BehaviorSubject<ConversationsBoxModel[]>(
    [],
  );
  conversations$ = this.conversationsSubject.asObservable();
  private baseUrl = 'https://localhost:7139/api/Conversation';
  private hubConnection!: signalR.HubConnection;

  private selectedConversationSubject =
    new BehaviorSubject<ConversationsBoxModel>({} as any);
  selectedConversation$ = this.selectedConversationSubject.asObservable();

  private unreadConversationCount = new BehaviorSubject<number>(0);
  unreadConversation$ = this.unreadConversationCount.asObservable();

  public messageRecieved = new BehaviorSubject<MessageModel[]>([]);
  public newMessageReceived = new signalR.Subject<MessageModel>();
  messages$ = this.messageRecieved.asObservable();

  public newConversationSubject = new BehaviorSubject<ConversationsBoxModel>(
    {} as ConversationsBoxModel,
  );
  private convestaionSubject = new BehaviorSubject<ConversationsBoxModel[]>([]);
  conversation$ = this.convestaionSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(
    private http: HttpClient,
    private appUserService: AppUsereService,
    private connectionsService: ConnectionsService,
  ) {
    this.registerConversationEvent();
  }

  private registerConversationEvent(): void {
    this.connectionsService.startConnection();
    this.hubConnection = this.connectionsService.HubConnection;
    this.hubConnection.on(
      'ConversationUpdated',
      (conversationUpdate: ConversationModel) => {
        const currentConversations = this.convestaionSubject.getValue();
        const conversationIndex = currentConversations.findIndex(
          (c) => c.conversationId === conversationUpdate.id,
        );
        if (conversationIndex !== -1) {
          currentConversations[conversationIndex].lastMessage =
            conversationUpdate.lastMessage;
          currentConversations[conversationIndex].lastMessageDate =
            conversationUpdate.lastMessageDate;
          currentConversations[conversationIndex].lastMessageSender =
            conversationUpdate.lastMessageSender;
          currentConversations[conversationIndex].hasUnreadMessages =
            conversationUpdate.hasUnreadMessages;
          this.convestaionSubject.next([...currentConversations]);
          this.convestaionSubject.next(
            currentConversations.sort(
              (a, b) =>
                new Date(b.lastMessageDate).getTime() -
                new Date(a.lastMessageDate).getTime(),
            ),
          );
        } else {
          this.appUserService
            .getUserById(conversationUpdate.userId!)
            .subscribe({
              next: (user) => {
                const conversation: ConversationsBoxModel = {
                  conversationId: conversationUpdate.id!,
                  lastMessage: conversationUpdate.lastMessage,
                  lastMessageDate: conversationUpdate.lastMessageDate,
                  userId: user.id,
                  userName: user.name,
                  userProfilePicture: user.profilePicture,
                };
                this.convestaionSubject.next([
                  conversation,
                  ...currentConversations,
                ]);
              },
            });
        }
      },
    );
    this.hubConnection.on(
      'ConversationCreated',
      (conversationUpdate: ConversationsBoxModel) => {
        const currentconversations = this.convestaionSubject.getValue();
        this.convestaionSubject.next([
          conversationUpdate,
          ...currentconversations,
        ]);
      },
    );
    this.hubConnection.on('ConversationCount', (data: { count: number }) => {
      this.unreadCountSubject.next(data.count);
    });
    this.hubConnection.on(
      'ConversationCountWithUnreadMessages',
      (count: number) => {
        this.unreadCountSubject.next(count);
      },
    );
    this.hubConnection.on(
      'updateConversationStatus',
      (conversationId: number) => {
        const conversations = this.convestaionSubject.getValue();
        const conversationIndex = conversations.findIndex(
          (c) => c.conversationId === conversationId,
        );
        if (conversationIndex !== -1) {
          conversations[conversationIndex].hasUnreadMessages = false;
          this.convestaionSubject.next([...conversations]);
        }
      },
    );

    this.hubConnection.on('MessageSent', (senderId, message: MessageModel) => {
      // Check if the message belongs to the selected conversation
      const selectedConversation = this.selectedConversationSubject.getValue();
      if (
        selectedConversation &&
        selectedConversation.conversationId === message.conversationId
      ) {
        let currentMessages = this.messageRecieved.getValue();
        currentMessages = [...currentMessages, message];
        this.messageRecieved.next(currentMessages);
        this.newMessageReceived.next(message);
      }

      const conversations = this.convestaionSubject.getValue();
      const conversationIndex = conversations.findIndex(
        (c) => c.conversationId === message.conversationId,
      );
      if (conversationIndex !== -1) {
        if (message.senderUserId !== localStorage.getItem('userId')) {
          conversations[conversationIndex].hasUnreadMessages = true;
        }
        this.convestaionSubject.next([...conversations]);
      }
    });
  }
  requestUnreadCount() {
    const userId = localStorage.getItem('userId'); // Assuming you store userId in localStorage
    if (userId) {
      this.hubConnection
        .invoke('GetUnreadCount', userId)
        .catch((err) => console.error('Error invoking GetUnreadCount:', err));
    }
  }
  getConversationsForUser(userId: string): Observable<ConversationsBoxModel[]> {
    //update conversation subject before returning

    const params = new HttpParams().set('userId', userId);
    var x = this.http.get<ConversationsBoxModel[]>(
      `${this.baseUrl}/getconversationsforuser`,
      { params },
    );
    x.subscribe((data) => {
      this.convestaionSubject.next(data);
    });
    return x;
  }
  loadMessages(
    conversationId: number,
    pageNumber: number = 1,
    pageSize: number = 20,
  ): void {
    this.getAllMessagesByConversationId(conversationId, pageNumber, pageSize)
      .pipe(map((messages) => messages.reverse()))
      .subscribe({
        next: (messages) => {
          console.log('messages', messages);
          this.messageRecieved.next(messages);
        },
        error: (error) => console.error('Error loading messages:', error),
      });
  }

  loadMoreMessages(
    conversationId: number,
    pageNumber: number = 1,
    pageSize: number = 20,
  ): Observable<any> {
    return this.getAllMessagesByConversationId(
      conversationId,
      pageNumber,
      pageSize,
    ).pipe(
      map((messages) => messages.reverse()), // Reverse to maintain order when prepending
      tap((reversedMessages) => {
        const currentMessages = this.messageRecieved.getValue();
        this.messageRecieved.next([...reversedMessages, ...currentMessages]);
      }),
      catchError((error) => {
        console.error('Failed to load more messages:', error);
        return throwError(() => new Error('Failed to load more messages'));
      }),
    );
  }

  getAllMessagesByConversationId(
    conversationId: number,
    pageNumber: number = 1,
    pageSize: number = 20,
  ): Observable<MessageModel[]> {
    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return this.http
      .get<
        MessageModel[]
      >(`${this.baseUrl}/${conversationId}/messages`, { params })
      .pipe(
        catchError((error) => {
          console.error('Failed to load messages:', error);
          return throwError(() => new Error('Failed to load messages'));
        }),
      );
  }

  selectConversation(conversation: ConversationsBoxModel): void {
    this.selectedConversationSubject.next(conversation);
  }

  postMessage(message: MessageModel): void {
    console.log(
      'posting message' + message.conversationId,
      message.senderUserId,
      message.receiverUserId,
    );
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SendMessage', message);
    }
  }
  MarkConversationAsRead(conversationId: number, userId: string): void {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke(
        'MarkConversationAsRead',
        conversationId,
        userId,
      );
    }
  }

  createConversation(
    conversationData: ConversationWithIdsModel,
  ): Observable<any> {
    return this.http.post(`${this.baseUrl}/NewConversation`, conversationData);
  }

  getUnreadConversationCount(userId: string): Observable<number> {
    return this.http
      .get<number>(
        `${this.baseUrl}/ConversationCountWithUnreadMessages?userId=${userId}`,
      )
      .pipe(
        map((response) => {
          this.unreadConversationCount.next(response);
          return response;
        }),
      );
  }
}
