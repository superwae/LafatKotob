export interface AppUserModel {
  id: string;
  isDeleted: boolean;
  name: string;
  city: string;
  email: string;
  dateJoined: Date;
  lastLogin: Date;
  profilePicture: string;
  about: string;
  dthDate: Date;
  historyId?: number;
  upVotes?: number;
}
