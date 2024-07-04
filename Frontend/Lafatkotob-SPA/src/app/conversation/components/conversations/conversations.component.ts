import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { ConversationComponent } from '../conversation/conversation.component';
import { ConversationService } from '../../Services/ConversationService/conversation.service';
import { CommonModule } from '@angular/common';
import { ConversationsBoxModel } from '../../models/ConversationModels';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { Observable, Subscription } from 'rxjs';

@Component({
  selector: 'app-conversations',
  standalone: true,
  imports: [ConversationComponent, CommonModule],
  templateUrl: './conversations.component.html',
  styleUrl: './conversations.component.css',
})
export class ConversationsComponent implements OnInit {
  @Input() conversations: ConversationsBoxModel[] = [];
  selectedConversation: any = null;
  private subscriptions = new Subscription();
  currentUserId = localStorage.getItem('userId');

  constructor(
    public conversationService: ConversationService,
    private appuserservice: AppUsereService,
    private cdRef: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.conversationService.conversations$.subscribe((conversations) => {
      this.conversations = conversations;
      this.cdRef.detectChanges();
    });
    this.loadConversations();
    this.subscriptions.add(
      this.conversationService.selectedConversation$.subscribe({
        next: (conversation) => {
          this.selectedConversation = conversation;
          this.cdRef.detectChanges();
        },
        error: (error) => console.error('Error selecting conversation:', error),
      }),
    );
  }

  loadConversations(): void {
    const userId = localStorage.getItem('userId');
    if (!userId) {
      console.error('User ID not found in local storage');
      return;
    }
    this.subscriptions.add(
      this.conversationService.getConversationsForUser(userId).subscribe({
        next: (data) => {
          this.conversations = data;
          if (this.conversations.length > 0) {
            this.conversationService.selectConversation(this.conversations[0]);
            this.selectConversation(this.conversations[0]);
          }
          this.cdRef.detectChanges();
        },
        error: (error) => console.error('Error fetching conversations:', error),
      }),
    );
  }

  selectConversation(conversation: ConversationsBoxModel): void {
    this.selectedConversation = conversation;

    if (conversation.hasUnreadMessages) {
      conversation.hasUnreadMessages = false;

      this.conversationService.MarkConversationAsRead(
        conversation.conversationId!,
        localStorage.getItem('userId')!,
      );
    }
    this.conversationService.selectConversation(conversation);
    this.cdRef.detectChanges();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
  get conversations$(): Observable<ConversationsBoxModel[]> {
    return this.conversationService.conversation$;
  }
  get selectedConversation$(): Observable<ConversationsBoxModel> {
    return this.conversationService.selectedConversation$;
  }
  trackByConversation(
    index: number,
    conversation: ConversationsBoxModel,
  ): number {
    return conversation.conversationId!;
  }

  prepareNewConversation(receiverId: string): void {
    const existingConversation = this.conversations.find(
      (conversation) => conversation.userId === receiverId,
    );

    if (!existingConversation) {
      this.appuserservice.getUserById(receiverId).subscribe({
        next: (userDetails) => {
          const tempConversation: ConversationsBoxModel = {
            conversationId: null,
            userId: receiverId,
            userName: userDetails.name,
            userProfilePicture: userDetails.profilePicture,
            lastMessage: '',
            lastMessageDate: new Date(),
          };
          this.conversations.push(tempConversation);
          this.selectedConversation = tempConversation;
        },
        error: (error) => {
          console.error('Failed to fetch user details:', error);
        },
      });
    } else {
      this.selectConversation(existingConversation);
    }
  }
}
