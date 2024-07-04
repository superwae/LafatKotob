import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { MessageModel } from '../../models/Message';
import { UserInMessages } from '../../models/UserInMessages';

@Component({
  selector: 'app-message',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message.component.html',
  styleUrl: './message.component.css',
})
export class MessageComponent {
  @Input() message: MessageModel | undefined;
  @Input() currentUserInfo: UserInMessages | null = null;
  @Input() otherUserInfo: UserInMessages | null = null;

  isSender = true;

  constructor(private appUserService: AppUsereService) {}

  ngOnInit(): void {
    this.isSender = this.message?.senderUserId === this.currentUserInfo!.userid;
  }
}
