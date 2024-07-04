export interface AppUserAdminPageModel {
  showCheck: boolean;
  id: string;
  isDeleted: boolean;
  name: string;
  city: string;
  email: string;
  dateJoined: Date;
  lastLogin: Date;
  profilePicture: string;
  about: string;
  dTHDate: Date;
  historyId?: number;
  roles: string[];
  saving?: boolean;
  upVotes?: number;
}
