export interface LoginResponse {
  token: string;
  userId: string;
  userName: string;
  profilePicture: string;
  expiration: Date;
  refreshToken: string;
  success: boolean;
  errorMessage: string;
  role: string;
  emailConfirmed: boolean;
}
