export interface NotificationModel {
    id: number,
    message: string,
    dateSent: Date,
    isRead: boolean,
    userId: number, // Foreign key to the user table
    userName: string,
    imgUrl: string,
}