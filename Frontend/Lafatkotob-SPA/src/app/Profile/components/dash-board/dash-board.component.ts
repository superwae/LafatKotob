import { AppUsereService } from './../../../Auth/services/appUserService/app-user.service';
import { Component, EventEmitter, OnInit, Output, Input } from '@angular/core';
import { BooksComponent } from '../../../Book/components/books/books.component';
import { ChangeDetectorRef } from '@angular/core';
import { AppUserModel } from '../../../Auth/Models/AppUserModel';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { WhoseProfileService } from '../../../shared/Service/WhoseProfileService/whose-profile.service';
import { ActivatedRoute } from '@angular/router';
import { BadgesModel } from '../../../shared/Models/BadgesModel';
import { BadgesService } from '../../../shared/Service/BadgesService/badges.service';
import { BadgesComponent } from '../../../shared/components/badges/badges.component';
import { ModaleService } from '../../../shared/Service/ModalService/modal.service';
import { BookService } from '../../../Book/Service/BookService';
@Component({
  selector: 'app-dash-board',
  standalone: true,
  imports: [BooksComponent, CommonModule, ReactiveFormsModule, BadgesComponent],
  templateUrl: './dash-board.component.html',
  styleUrl: './dash-board.component.css',
})
export class DashBoardComponent implements OnInit {
  name: string | null = '';
  profilePictureUrl: string | null = '';
  selectedImage: File | null = null;
  selectedImageUrl: string | null = null;
  currentUserInfo: AppUserModel | null = null;
  badges!: BadgesModel[];
  id: string | null = null;
  registrationData: FormData = new FormData();
  autoRefreshInterval: any;
  isEditingBio: boolean = false;
  isEditingCity = false;
  editedBio: string = '';
  editedCity: string = '';
  OtherProfile: boolean = false;
  showModal: boolean = false;
  @Output() profilePictureChanged = new EventEmitter<string>();

  constructor(
    private AppUsereService: AppUsereService,
    private WhoseProfileService: WhoseProfileService,
    private route: ActivatedRoute,
    private BadgesService: BadgesService,
    private modaleService: ModaleService,
    private bookService: BookService,
  ) {}

  ngOnInit(): void {
    window.scrollTo(0, 0);
    this.modaleService.showModal$.subscribe((visible) => {
      this.showModal = visible;
    });
    this.currentUserInfo = {
      id: '',
      isDeleted: false,
      name: '',
      city: '',
      email: '',
      dateJoined: new Date(),
      lastLogin: new Date(),
      profilePicture: '',
      about: '',
      dthDate: new Date(),
    };

    this.route.parent!.paramMap.subscribe((params) => {
      this.name = params.get('username');
      this.route.parent!.paramMap.subscribe((params) => {
        const username = params.get('username');
        if (username) {
          this.bookService.setPageNumber(1);
          console.log('refreshing books');
          //  this.bookService.refreshBooksByUserName(this.name!);
        }
      });

      if (localStorage.getItem('userName') != this.name) {
        this.WhoseProfileService.otherUserName = this.name!;
        this.OtherProfile = true;

        this.AppUsereService.GetUserByUserName(
          this.WhoseProfileService.otherUserName,
        ).subscribe(
          (res?: AppUserModel) => {
            if (res != null) {
              this.name = res.name;
              this.currentUserInfo!.name = res.name;
              this.profilePictureUrl = res.profilePicture;
              this.currentUserInfo!.profilePicture = res.profilePicture;
              this.id = res.id;
              this.currentUserInfo!.id = res.id;
              this.currentUserInfo!.about = res.about;
              this.currentUserInfo!.city = res.city;
              this.currentUserInfo!.dthDate = res.dthDate;
              this.currentUserInfo!.upVotes = res.upVotes;
              this.BadgesService.getBadgesByUserId(res.id).subscribe(
                (badges: BadgesModel[]) => {
                  this.badges = badges;
                },
              );
            } else {
            }
          },
          (error: any) => {},
        );
      } else {
        this.name = localStorage.getItem('userName');
        this.id = localStorage.getItem('userId');
        this.loadUser();
      }
    });
  }
  startAutoRefresh(): void {
    this.stopAutoRefresh();

    this.autoRefreshInterval = setInterval(() => {
      this.loadUser();
    }, 5000);
  }

  stopAutoRefresh(): void {
    if (this.autoRefreshInterval) {
      clearInterval(this.autoRefreshInterval);
      this.autoRefreshInterval = undefined;
    }
  }
  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    const files = target.files;

    if (files && files.length) {
      const file = files[0];

      if (!file.type.startsWith('image/')) {
        console.error('The selected file is not an image.');
        return;
      }

      this.selectedImage = file;
      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        this.selectedImageUrl = e.target?.result as string;
        this.profilePictureChanged.emit(this.selectedImageUrl!);
      };

      reader.readAsDataURL(file);

      if (this.registrationData) {
        if (this.selectedImage) {
          this.registrationData.append(
            'imageFile',
            this.selectedImage,
            this.selectedImage.name,
          );
        }
        this.registrationData.append('UserId', this.id!);

        this.AppUsereService.updateUserProfilePicture(
          this.registrationData,
        ).subscribe(
          (profileImage) => {
            this.profilePictureUrl = profileImage;
            localStorage.setItem('profilePicture', this.profilePictureUrl!);

            this.loadUser();
          },
          (error) => {
            console.error('Error updating profile picture:', error);
            window.location.href = window.location.href;
          },
        );
      } else {
        console.error('registrationData is undefined.');
      }
    }
  }

  private loadUser(): void {
    if (!this.id) return;

    this.AppUsereService.getUserById(this.id).subscribe(
      (user) => {
        if (!this.currentUserInfo) {
          this.currentUserInfo = {
            id: '',
            isDeleted: false,
            name: '',
            city: '',
            email: '',
            dateJoined: new Date(),
            lastLogin: new Date(),
            profilePicture: '',
            about: '',
            dthDate: new Date(),
            upVotes: 0,
          };
        }
        if (user != null) {
          this.profilePictureUrl = user.profilePicture;

          this.currentUserInfo.id = user.id;
          this.currentUserInfo.name = user.name;
          this.currentUserInfo.about = user.about;
          this.currentUserInfo.city = user.city;
          this.currentUserInfo.profilePicture = user.profilePicture;
          this.currentUserInfo.dthDate = user.dthDate;
          this.currentUserInfo.upVotes = user.upVotes;
          this.BadgesService.getBadgesByUserId(user.id).subscribe(
            (badges: BadgesModel[]) => {
              this.badges = badges;
            },
          );
        }
      },
      (error) => {
        console.error('Error loading user:', error);
      },
    );
  }

  editBio() {
    this.isEditingBio = true;
  }

  saveBio() {
    this.isEditingBio = false;
    const edited = document.getElementById('edited') as HTMLInputElement;
    if (!this.id) return;
    if (edited) {
      this.editedBio = edited.value;
      this.currentUserInfo!.about = this.editedBio;
      this.AppUsereService.updateUserBio(this.id, this.editedBio).subscribe();
    } else {
      console.error("Element with id 'myInput' not found.");
    }
  }
  editCity() {
    this.isEditingCity = true;
  }

  saveCity() {
    /*this.isEditingBio = false;
    const edited = document.getElementById('edited') as HTMLInputElement;
    if (!this.id) return;
    if (edited) {
      this.editedBio = edited.value;
      this.currentUserInfo!.about = this.editedBio;
      console.log(this.editedBio, this.id);
      this.AppUsereService.updateUserBio(this.id, this.editedBio).subscribe();
    } else {
      console.error("Element with id 'myInput' not found.");
    }*/

    this.isEditingCity = false;
    const edited2 = document.getElementById('edited2') as HTMLInputElement;
    //this.currentUserInfo!.city = this.editedCity;
    if (!this.id) return;
    if (edited2) {
      this.editedCity = edited2.value;
      this.currentUserInfo!.city = this.editedCity;
      this.AppUsereService.updateUserCity(this.id, this.editedCity).subscribe();
    } else {
      console.error("Element with id 'myInput' not found.");
    }
  }
  isSameUser(): boolean {
    return this.name === localStorage.getItem('userName');
  }
  vote(isUpvote: boolean): void {
    if (!this.currentUserInfo || !this.currentUserInfo.id) {
      console.error('No user information available');
      return;
    }

    this.AppUsereService.vote(
      localStorage.getItem('userName')!,
      this.name!,
      isUpvote,
    ).subscribe({
      next: (response) => {
        if (response.success) {
          if (isUpvote) {
            this.currentUserInfo!.upVotes!++;
          } else {
            this.currentUserInfo!.upVotes!--;
          }
        } else {
          console.error('Failed to update vote:', response.message);
        }
      },
      error: (error) => {
        console.error('Error during voting:', error);
      },
    });
  }
}
