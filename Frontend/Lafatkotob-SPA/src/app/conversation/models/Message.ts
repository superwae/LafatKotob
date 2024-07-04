export interface MessageModel {
  id: number;
  conversationId: number;
  senderUserId: string;
  receiverUserId: string;
  messageText: string;
  dateSent: Date;
  isReceived: boolean;
  isRead: boolean;
  isDeletedBySender: boolean;
  isDeletedByReceiver: boolean;
}
