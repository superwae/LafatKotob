export interface ConversationModel {
  id?: number;
  lastMessage: string;
  lastMessageDate: Date;
  userId?: string;
  hasUnreadMessages?: boolean;
  lastMessageSender?: string;
}

export interface ConversationsBoxModel {
  conversationId: number | null;
  lastMessage: string;
  lastMessageDate: Date;
  userId: string;
  userName: string;
  userProfilePicture: string;
  hasUnreadMessages?: boolean;
  lastMessageSender?: string;
}
