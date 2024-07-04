export interface ConversationWithIdsModel {
  userIds: string[];
  LastMessage: string | null;
  LastMessageDate: Date;
  lastMessageSender?: string;
}
