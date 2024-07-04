import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
@Injectable({
  providedIn: 'root',
})
export class ConnectionsService {
  private chatHubUrl = 'https://localhost:7139/chat';
  private hubConnection!: signalR.HubConnection;

  constructor() {
    this.buildConnection();
    this.startConnection();
  }
  private buildConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.chatHubUrl, {
        accessTokenFactory: () => {
          var token = localStorage.getItem('token');
          return token || '';
        },
      })
      .build();
  }

  public startConnection(): void {
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      this.hubConnection
        .start()
        .then(() => console.log('Connection started'))
        .catch((err) =>
          console.error('Error while establishing connection: ', err),
        );
    }
  }
  public stopConnection(): void {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection
        .stop()
        .then(() => console.log('Connection stopped'))
        .catch((err) =>
          console.error('Error while stopping connection: ', err),
        );
    }
  }

  public isConnected(): boolean {
    return this.hubConnection.state === signalR.HubConnectionState.Connected;
  }
  get HubConnection(): signalR.HubConnection {
    return this.hubConnection;
  }
}
