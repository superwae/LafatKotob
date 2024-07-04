import { Component, OnInit } from '@angular/core';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppUserAdminPageModel } from '../../../Auth/Models/AppUserAdminPageModel';
import { SearchBarComponent } from '../search-bar/search-bar.component';
import { RouterLink } from '@angular/router';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-admin-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    SearchBarComponent,
    RouterLink,
  ],
  templateUrl: './admin-page.component.html',
  styleUrls: ['./admin-page.component.css'],
})
export class AdminPageComponent implements OnInit {
  allUsers: AppUserAdminPageModel[] | null = null;
  pageNumber = 1;
  pageSize = 10;
  roles: string[] = ['Admin', 'User', 'Premium'];
  deletemessage: string = 'Delete';
  allUsersLoaded = false;
  loadingUsers = false;
  currentUsername: string | null = null;

  private usersSubscription: Subscription = new Subscription();
  constructor(
    private AppUsereService: AppUsereService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const role = localStorage.getItem('role');
    if (role != 'Admin') {
      this.router.navigate(['/home']);
      return;
    }

    this.AppUsereService.users$.subscribe((users) => {
      this.allUsers = users;
    });
    window.addEventListener('scroll', () => {
      const scrollableHeight = document.documentElement.scrollHeight;
      const currentBottomPosition = window.scrollY + window.innerHeight;
      const threshold = 100;
      const distanceFromBottom = scrollableHeight - currentBottomPosition;
      if (
        distanceFromBottom <= threshold &&
        !this.allUsersLoaded &&
        !this.loadingUsers
      ) {
        this.loadMoreUsers();
      }
    });

    this.AppUsereService.refreshUsers(this.pageNumber, this.pageSize);
  }

  ngAfterViewInit(): void {
    window.addEventListener('scroll', () => {
      const scrollableHeight = document.documentElement.scrollHeight;
      const currentBottomPosition = window.scrollY + window.innerHeight;
      const threshold = 100;
      const distanceFromBottom = scrollableHeight - currentBottomPosition;

      if (
        distanceFromBottom <= threshold &&
        !this.allUsersLoaded &&
        !this.loadingUsers
      ) {
        this.loadMoreUsers();
      }
    });
  }

  loadMoreUsers(): void {
    if (!this.allUsersLoaded && !this.loadingUsers) {
      this.loadingUsers = true;
      this.AppUsereService.getALlUser(this.pageNumber, this.pageSize).subscribe(
        (users) => {
          if (users.length < this.pageSize) {
            this.allUsersLoaded = true; // No more books to load
          }

          this.pageNumber++;

          this.AppUsereService.refreshUsers(this.pageNumber, this.pageSize);

          this.loadingUsers = false;
        },
        () => (this.loadingUsers = false),
      );
    }
  }

  ngOnDestroy(): void {
    if (this.usersSubscription) {
      this.usersSubscription.unsubscribe();
    }
  }

  saveUser(user: AppUserAdminPageModel): void {
    const userId = user.id;
    const role = user.roles.toString();

    user.saving = true;

    this.AppUsereService.updateUserRole(userId, role).subscribe({
      next: (response) => {
        const updatedRole = response.Role;

        user.saving = false;
        user.showCheck = true;

        setTimeout(() => {
          user.showCheck = false;
        }, 3000);
      },
      error: (err) => {
        console.error('Error updating user role:', err);
        user.saving = false;
      },
    });
  }
  deleteUser(user: AppUserAdminPageModel): void {
    this.AppUsereService.toggleDelete(user.id).subscribe({
      next: (isDeleted) => {
        user.isDeleted = isDeleted;
      },
      error: (err) => console.error('Error deleting user', err),
    });
  }

  handleSearch(query: string): void {
    if (query) {
      this.AppUsereService.searchUsers(query).subscribe({
        next: (users) => (this.allUsers = users),
        error: (err) => console.error('Error fetching search results:', err),
      });
    } else {
      this.AppUsereService.refreshUsers(this.pageNumber, this.pageSize);
    }
  }
}

export class YourComponent {
  constructor(private router: Router) {}

  navigateToPersonalPage(user: any): void {
    // Assuming you have a route defined for the user's personal page
    this.router.navigate(['/users', user.id]); // Adjust the route and user id as per your application
  }
}
