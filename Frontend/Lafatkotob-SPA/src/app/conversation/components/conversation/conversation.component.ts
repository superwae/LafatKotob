import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { MessageComponent } from '../message/message.component';
import { CommonModule } from '@angular/common';
import { MessageModel } from '../../models/Message';
import { UserInMessages } from '../../models/UserInMessages';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPaperPlane } from '@fortawesome/free-solid-svg-icons';
import { ConversationService } from '../../Services/ConversationService/conversation.service';
import { ConversationsBoxModel } from '../../models/ConversationModels';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { Observable, Subject } from 'rxjs';

@Component({
  selector: 'app-conversation',
  standalone: true,
  imports: [MessageComponent, CommonModule, FormsModule, FontAwesomeModule],
  templateUrl: './conversation.component.html',
  styleUrls: ['./conversation.component.css'],
})
export class ConversationComponent implements OnInit, OnChanges, AfterViewInit {
  @ViewChild('scrollContainer') private scrollContainer: ElementRef | null =
    null;
  @Input() conversation: ConversationsBoxModel | null = null;
  newMessage: MessageModel = {} as MessageModel;
  faPaperPlane = faPaperPlane;
  currentUserInfo: UserInMessages | null = null;
  otherUserInfo: UserInMessages | null = null;
  messages: MessageModel[] = [];
  profilePictureUrl: string | null = '';
  isLoadingMoreMessages = false;
  currentPage = 1;
  totalPages = 20;
  conversationId = 0;
  newMessageIndicatorText = '';
  showNewMessageIndicator = false;

  private scrollPositions = new Map<number, number>();
  public newMessageReceived = new Subject<MessageModel>();
  profilePicture: any;
  chatStarted: boolean = false; // التأكد من تعريف هذه الخاصية
  OtherProfile: boolean = false; // التأكد من تعريف هذه الخاصية

  constructor(
    private dialog: MatDialog,
    private AppUsereService: AppUsereService,
    private conversationService: ConversationService,
    private cdRef: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.conversationService.messages$.subscribe((messages) => {
      const isScrolledToBottom = this.isUserAtBottom();
      this.messages = messages;
      this.cdRef.detectChanges();
      if (isScrolledToBottom) {
        this.scrollToBottom();
      } else {
        this.showNewMessageIndicator = true;
      }
    });
    this.loadCurrentUser();
    this.loadOtherUser();
    this.conversationService.newMessageReceived.subscribe({
      next: (message: MessageModel) => {
        const isScrolledToBottom = this.isUserAtBottom();
        this.cdRef.detectChanges();
        if (!isScrolledToBottom) {
          this.newMessageIndicatorText = 'New Message';
          this.showNewMessageIndicator = true;
        } else {
          this.scrollToBottom();
        }
      },
      error: (err) => console.error('Error receiving message:', err),
      complete: () => console.log('Message stream complete'),
    });
    if (this.conversation?.conversationId) {
      this.loadMessages(this.conversation?.conversationId);
      this.conversationService.messageRecieved.subscribe((message) => {
        this.messages = message;
        this.cdRef.detectChanges();
        this.scrollToBottom();
      });
    }
  }

  isUserAtBottom(): boolean {
    const element = this.scrollContainer!.nativeElement;
    return (
      element.scrollHeight - element.scrollTop <= element.clientHeight + 10
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.scrollToBottom();
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
    if (changes['conversation']) {
      this.resetComponentState();
      this.loadMessages(this.conversation?.conversationId!);
    }
  }

  private resetComponentState(): void {
    this.messages = [];
    this.currentPage = 1;
    this.loadOtherUser();
    this.isLoadingMoreMessages = false;
  }

  ngAfterViewInit(): void {
    this.listenToScroll();
  }

  loadMessages(conversationId: number): void {
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
    this.conversationId = conversationId;
    this.conversationService.loadMessages(conversationId, 1, 20);
  }

  private loadCurrentUser(): void {
    this.currentUserInfo = {
      userid: '',
      userName: '',
      profilePicture: '',
    };
    const toekn = localStorage.getItem('token');
    if (toekn) {
      this.currentUserInfo.userid = localStorage.getItem('userId') || '';
      this.currentUserInfo.userName =
        localStorage.getItem('userName') || 'Unknown User';
      this.currentUserInfo.profilePicture =
        localStorage.getItem('profilePicture') || 'path/to/default/image';
    }
  }

  private loadOtherUser(): void {
    this.AppUsereService.getUserById(this.conversation?.userId!).subscribe(
      (user) => {
        this.otherUserInfo = {
          userid: user.id,
          userName: user.name,
          profilePicture: user.profilePicture,
        };
      },
    );
  }

  sendMessage(): void {
    this.scrollToBottom();
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
    if (this.newMessage.messageText === '' || !this.newMessage.messageText)
      return;
    this.newMessage.conversationId = this.conversation?.conversationId!;
    this.newMessage.senderUserId = localStorage.getItem('userId') || '';
    this.newMessage.receiverUserId = this.conversation?.userId!;

    this.conversationService.postMessage(this.newMessage);
    this.newMessage.messageText = '';
  }

  get messages$(): Observable<MessageModel[]> {
    return this.conversationService.messages$;
  }

  trackByMessage(index: number, message: MessageModel): number {
    return message.id!;
  }

  scrollToBottom(): void {
    setTimeout(() => {
      const element = this.scrollContainer!.nativeElement;
      element.scrollTop = element.scrollHeight;

      // Reset the indicator only if at the bottom
      if (this.isUserAtBottom()) {
        this.showNewMessageIndicator = false;
        this.newMessageIndicatorText = '';
      }
    }, 100);
  }

  private listenToScroll(): void {
    this.scrollContainer!.nativeElement.addEventListener('scroll', () => {
      if (this.conversation) {
        this.scrollPositions.set(
          this.conversation.conversationId!,
          this.scrollContainer!.nativeElement.scrollTop,
        );
      }
      if (!this.isUserAtBottom()) {
        this.showNewMessageIndicator = true;
      } else {
        this.showNewMessageIndicator = false;
        this.newMessageIndicatorText = '';
      }
    });
    const container = this.scrollContainer!.nativeElement;
    container.addEventListener('scroll', () => {
      if (
        container.scrollTop === 0 &&
        this.currentPage < this.totalPages &&
        !this.isLoadingMoreMessages
      ) {
        this.loadMoreMessages();
      }
    });
  }

  private loadMoreMessages(): void {
    if (this.isLoadingMoreMessages || this.currentPage >= this.totalPages) {
      return;
    }
    this.isLoadingMoreMessages = true;
    this.currentPage++;

    const oldScrollHeight = this.scrollContainer!.nativeElement.scrollHeight;

    this.conversationService
      .loadMoreMessages(this.conversationId, this.currentPage, 20)
      .subscribe(() => {
        // Delay the scroll adjustment until Angular completes the DOM update
        setTimeout(() => {
          const newScrollHeight =
            this.scrollContainer!.nativeElement.scrollHeight;
          this.scrollContainer!.nativeElement.scrollTop +=
            newScrollHeight - oldScrollHeight;
          this.isLoadingMoreMessages = false;
        });
      });
  }

  @HostListener('scroll', ['$event'])
  onScroll(event: any): void {
    const atBottom = this.isUserAtBottom();
    if (atBottom) {
      this.showNewMessageIndicator = false;
      if (this.conversation && this.conversation.hasUnreadMessages) {
        this.conversationService.MarkConversationAsRead(
          this.conversation!.conversationId!,
          localStorage.getItem('userId')!,
        );
        this.conversation.hasUnreadMessages = false;
      }
    } else {
      this.showNewMessageIndicator = true;
    }
  }

  startChat(): void {
    this.chatStarted = true;
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files![0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.currentUserInfo!.profilePicture = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }
  errorHandler(event: Event) {
    const target = event.target as HTMLImageElement; // Type assertion
    if (target) {
      target.src = 'assets/images/default-profile.svg';
    }
  }
}
