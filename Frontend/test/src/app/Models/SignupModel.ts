export interface register {
    Name: string;
    Password: string;
    ConfirmNewPassword: string;
    Email: string;
    ConfirmNewEmail: string;
    UserName: string;
    ProfilePictureUrl?: string; // Optional property
    DTHDate: string;
    City?: string; // Optional property
    About?: string; // Optional property
  }