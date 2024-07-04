import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  HostListener,
  OnDestroy,
  OnInit,
} from '@angular/core';
import {
  Router,
  Event as RouterEvent,
  NavigationEnd,
  RouterLink,
} from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ModaleService } from '../../Service/ModalService/modal.service';
import { AppUsereService } from '../../../Auth/services/appUserService/app-user.service';
import { AppUserModel } from '../../../Auth/Models/AppUserModel';
import { TooltipDirective } from '../../directives/tooltip.directive';
import { ConversationService } from '../../../conversation/Services/ConversationService/conversation.service';

import { NotificationModel } from '../../../Notification/Models/Notification';
import { NotificationServiceService } from '../../../Notification/service/notification-service.service';
import { NotificationComponent } from '../../../Notification/componant/notification/notification.component';
@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  standalone: true,
  imports: [CommonModule, RouterLink, TooltipDirective, NotificationComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent implements OnInit, OnDestroy {
  isAffix: boolean = true;
  navExpanded: boolean = false;
  showMenu: boolean = true;
  showModal: boolean = false;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  profilePictureUrl?: string | null = null;
  User: AppUserModel | null = null;
  showDropdown = false;
  showChatDropdown: boolean = false;
  showNotificationsDropdown: boolean = false;
  private unreadCountSubscription!: Subscription;
  showEventsDropdown: boolean = false;
  private subscription: Subscription = new Subscription();
  private authSubscription: Subscription = new Subscription();
  private routerSubscription: Subscription = new Subscription();
  isProfilePage: boolean = false;
  private lastScrollPos = 0;
  show: boolean = true;
  autoRefreshInterval: any;
  chatUnreadCount: number = 0;
  NotificationUnreadCount: number = 0;
  notifications!: NotificationModel[];
  notification!: NotificationModel;
  currentUserInfo: AppUserModel | null = null;
  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router,
    private modalService: ModaleService,
    private appUserService: AppUsereService,
    private conversationService: ConversationService,
    private notificationService: NotificationServiceService,
    private cdRef: ChangeDetectorRef,
  ) {
    this.router.events
      .pipe(
        filter(
          (event: RouterEvent): event is NavigationEnd =>
            event instanceof NavigationEnd,
        ),
      )
      .subscribe((event: NavigationEnd) => {
        this.showMenu = event.urlAfterRedirects !== '/login';
      });

    this.router.events
      .pipe(
        filter(
          (event: RouterEvent): event is NavigationEnd =>
            event instanceof NavigationEnd,
        ),
      )
      .subscribe((event: NavigationEnd) => {
        if (event.urlAfterRedirects.startsWith('/user')) {
          this.isProfilePage = true;
          this.transformNavbar();
        } else {
          this.reverseNavbarTransformation();
          this.isProfilePage = false;
        }
      });
  }

  ngOnInit() {
    this.notificationService.initializeNotifications(
      localStorage.getItem('userId')!,
    );
    this.unreadCountSubscription =
      this.notificationService.unreadCount$.subscribe({
        next: (count) => {
          this.NotificationUnreadCount = count;
          this.cdr.detectChanges();
        },
        error: (error) => console.error('Error updating unread count:', error),
      });

    // Fetch the initial unread count
    const userId = localStorage.getItem('userId');
    if (userId) {
      this.notificationService.fetchInitialUnreadCount(userId);
    }
    this.notificationService.Notifications$.subscribe((newNotifications) => {
      this.notifications = newNotifications;
      if (this.notifications.length > 10) {
        this.notifications = this.notifications.slice(0, 10);
      }
      this.cdRef.detectChanges();
    });

    const role = localStorage.getItem('role');
    if (role == 'Admin') {
      this.isAdmin = true;
    } else {
      this.isAdmin = false;
    }

    this.conversationService
      .getUnreadConversationCount(localStorage.getItem('userId')!)
      .subscribe((count) => {
        this.chatUnreadCount = count;
      });
    this.unreadCountSubscription =
      this.conversationService.unreadCount$.subscribe({
        next: (count) => {
          this.chatUnreadCount = count;
          this.cdr.detectChanges();
        },
        error: (error) => console.error('Error updating unread count:', error),
      });

    this.cdr.detectChanges();

    this.routerSubscription = this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateNavbarVisibility();
      });
    this.notificationService
      .getnotificationsNavBar(localStorage.getItem('userId')!)
      .subscribe((data) => {
        this.notifications = data;
      });

    this.notificationService
      .getnotificationsUnRead(localStorage.getItem('userId')!)
      .subscribe({
        next: (count) => {
          this.NotificationUnreadCount = count;
          this.cdr.detectChanges();
        },
        error: (error) => console.error('Error updating unread count:', error),
      });

    this.cdr.detectChanges();

    this.subscription.add(
      this.modalService.showModal$.subscribe((visible) => {
        if (!visible) {
          this.show = true;
        } else {
          this.show = false;
          this.showModal = visible;
        }
      }),
    );

    this.authSubscription = this.appUserService.isAuthenticated.subscribe(
      (isAuthenticated) => {
        this.isLoggedIn = isAuthenticated;
        if (isAuthenticated) {
          // Fetch user info only if authenticated
          const userId = localStorage.getItem('userId');
          if (userId) {
            this.fetchUserInfo(userId);
          }
        } else {
          // Reset user info on logout
          this.User = null;
          this.profilePictureUrl = null;
        }
      },
    );
  }

  fetchUserInfo(userId: string) {
    this.appUserService.getUserById(userId).subscribe({
      next: (userData: AppUserModel) => {
        this.User = userData;
        this.profilePictureUrl = userData.profilePicture;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error fetching user data:', error);
      },
    });
  }
  ngOnDestroy() {
    this.subscription.unsubscribe();
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
    this.routerSubscription.unsubscribe();
    if (this.unreadCountSubscription) {
      this.unreadCountSubscription.unsubscribe();
    }
  }

  @HostListener('window:scroll', ['$event'])
  onWindowScroll() {
    const currentScrollPos = window.pageYOffset;
    if (!this.isProfilePage) {
      if (this.lastScrollPos < currentScrollPos && currentScrollPos > 50) {
        // Scrolling down
        this.isAffix = false;
        this.show = false;
      } else {
        // Scrolling up
        this.isAffix = true;
        this.show = true;
      }
    }

    this.lastScrollPos = currentScrollPos;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const chatDropdownElement = document.querySelector(
      '.chat-dropdown-content',
    );
    const notificationsDropdownElement = document.querySelector(
      '.notifications-dropdown-content',
    );
    const eventsDropdownElement = document.querySelector(
      '.events-dropdown-content',
    );

    const chatTriggerElement = document.querySelector('.chat-dropdown-trigger');
    const notificationsTriggerElement = document.querySelector(
      '.notifications-dropdown-trigger',
    );
    const eventsTriggerElement = document.querySelector(
      '.events-dropdown-trigger',
    );

    // Checking and closing the Profile Dropdown
    const dropdownElement = document.querySelector('.dropdown-content');
    const triggerElement = document.querySelector('.profile-dropdown');

    if (
      dropdownElement &&
      triggerElement &&
      !dropdownElement.contains(event.target as Node) &&
      !triggerElement.contains(event.target as Node)
    ) {
      this.showDropdown = false;
    }

    // Checking and closing the Chat Dropdown
    if (
      chatDropdownElement &&
      chatTriggerElement &&
      !chatDropdownElement.contains(event.target as Node) &&
      !chatTriggerElement.contains(event.target as Node)
    ) {
      this.showChatDropdown = false;
    }

    // Checking and closing the Notifications Dropdown
    if (
      notificationsDropdownElement &&
      notificationsTriggerElement &&
      !notificationsDropdownElement.contains(event.target as Node) &&
      !notificationsTriggerElement.contains(event.target as Node)
    ) {
      this.showNotificationsDropdown = false;
    }

    // Checking and closing the Events Dropdown
    if (
      eventsDropdownElement &&
      eventsTriggerElement &&
      !eventsDropdownElement.contains(event.target as Node) &&
      !eventsTriggerElement.contains(event.target as Node)
    ) {
      this.showEventsDropdown = false;
    }
  }

  toggleNavbar() {
    this.navExpanded = !this.navExpanded;
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  toggleChatDropdown(event: MouseEvent): void {
    event.stopPropagation();
    this.showChatDropdown = !this.showChatDropdown;

    if (this.showChatDropdown) {
      this.showNotificationsDropdown = false;
      this.showEventsDropdown = false;
      this.showDropdown = false;
    }
  }

  toggleNotificationsDropdown(event: MouseEvent): void {
    event.stopPropagation();
    this.showNotificationsDropdown = !this.showNotificationsDropdown;

    // Close other dropdowns
    if (this.showNotificationsDropdown) {
      this.showChatDropdown = false;
      this.showEventsDropdown = false;
      this.showDropdown = false;
    }
  }

  toggleEventsDropdown(event: MouseEvent): void {
    event.stopPropagation();
    this.showEventsDropdown = !this.showEventsDropdown;

    if (this.showEventsDropdown) {
      this.showChatDropdown = false;
      this.showNotificationsDropdown = false;
      this.showDropdown = false;
    }
  }

  transformNavbar() {
    const navbarElement = document.querySelector('.nav');
    if (navbarElement) {
      navbarElement.classList.add('affix');
    }
  }
  reverseNavbarTransformation() {
    const navbarElement = document.querySelector('.nav');
    if (navbarElement) {
      navbarElement.classList.remove('affix');
    }
  }
  updateNavbarVisibility() {
    const isUserProfilePage = this.router.url.includes('/user');
    this.isAffix = true;
    this.show = true;
    if (isUserProfilePage) {
      this.isAffix = true;
      this.show = true;
    }
  }
  logout() {
    this.isLoggedIn = false;
    this.User = null;
    this.profilePictureUrl = null;
    this.appUserService.logout();
  }
}
