import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class WhoseProfileService {
  isOtherProfile = false;
  otherUserName = "";

  constructor() { }
}
