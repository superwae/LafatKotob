export interface ResetPasswordModel {
  email: string;
  newPassword: string;
  confirmPassword?: string;
  token: string;
}
