export interface Book {
  id: number;
  title: string;
  author: string;
  description: string;
  coverImage: string;
  userId: string;
  publicationDate: Date;
  isbn: string;
  pageCount?: number;
  condition: string;
  status: string;
  type: string;
  language: string;
  addedDate: Date;
  isLikedByCurrentUser?: boolean;
  bookId?: number;
  userName?: string;
  userProfilePicture?: string;
  HistoryId?: number;
  partnerUserId?: string;
}
