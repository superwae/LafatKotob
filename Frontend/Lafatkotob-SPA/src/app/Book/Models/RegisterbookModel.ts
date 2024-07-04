export interface RegisterBook {
  Title: string;
  Author: string;
  Description: string;
  CoverImage: string;
  UserId: string;
  HistoryId?: number;
  PublicationDate: Date;
  ISBN: string;
  PageCount?: number;
  Condition: string;
  Status: string;
  Type: string;
  PartnerUserId: string;
  Language: string;
  bookId?: number;
}
