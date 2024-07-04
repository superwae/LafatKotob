export interface ConversationsUserModel {
  senderId: string;
  reciverId: string;
  lastMessage: string;
  lastMessageDate: Date;
  conversationId: number;
}
