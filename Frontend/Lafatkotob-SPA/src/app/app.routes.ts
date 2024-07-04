import { NotificationComponent } from './Notification/componant/notification/notification.component';
import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';
import { UserProfileComponent } from './Profile/components/user-profile/user-profile.component';
import { EventsComponent } from './Event/components/events/events.component';
import { BooksComponent } from './Book/components/books/books.component';
import { wishlistComponent } from './Profile/components/wishlist/wishlist.component';
import { DashBoardComponent } from './Profile/components/dash-board/dash-board.component';
import { UserSettingsComponent } from './Profile/components/user-settings/user-settings.component';
import { NotificationPageComponent } from './Notification/componant/notification-page/notification-page.component';
import { ContactUsComponent } from './shared/components/contact-us/contact-us.component';
export const routes: Route[] = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },

  {
    path: 'user/:username',
    component: UserProfileComponent,
    children: [
      { path: '', redirectTo: 'dash-board', pathMatch: 'full' },
      { path: '', pathMatch: 'full', redirectTo: 'books' },
      { path: 'books', component: BooksComponent },
      { path: 'events', component: EventsComponent },
      { path: 'wishlist', component: wishlistComponent },
      { path: 'dash-board', component: DashBoardComponent },
      { path: 'user-settings', component: UserSettingsComponent },
      { path: 'Notifications', component: NotificationPageComponent },
      { path: 'contact-us', component: ContactUsComponent },
      
    ],
  },
  {
    path: 'conversations',
    loadComponent: () =>
      import(
        './conversation/components/conversations/conversations.component'
      ).then((m) => m.ConversationsComponent),
  },

  {
    path: 'contact-us',
    loadComponent: () =>
      import(
        './shared/components/contact-us/contact-us.component'
      ).then((m) => m.ContactUsComponent),
  },

  
  
  {
    path: 'badges',
    loadComponent: () =>
      import('./shared/components/badges/badges.component').then(
        (m) => m.BadgesComponent,
      ),
  },

  {
    path: 'edit-book/:id',
    loadComponent: () =>
      import('./Book/components/edit-book/edit-book.component').then(
        (m) => m.EditBookComponent,
      ),
  },
  {
    path: 'event/:id',
    loadComponent: () =>
      import('./Event/components/event/event.component').then(
        (m) => m.EventComponent,
      ),
  },
  {
    path: 'conversation',
    loadComponent: () =>
      import(
        './conversation/components/conversation/conversation.component'
      ).then((m) => m.ConversationComponent),
  },

  {
    path: 'events',
    loadComponent: () =>
      import('./Event/components/events/events.component').then(
        (m) => m.EventsComponent,
      ),
  },
  {
    path: 'event-details/:id',
    loadComponent: () =>
      import('./Event/components/event-details/event-details.component').then(
        (m) => m.EventDetailsComponent,
      ),
  },
  {
    path: 'adminpage',
    loadComponent: () =>
      import('./shared/components/admin-page/admin-page.component').then(
        (m) => m.AdminPageComponent,
      ),
  },

  {
    path: 'userPreferences',
    loadComponent: () =>
      import(
        './Auth/components/user-preference/user-preference.component'
      ).then((m) => m.UserPreferenceComponent),
  },

  {
    path: 'login',
    loadComponent: () =>
      import('./Auth/components/login-register/login-register.component').then(
        (m) => m.LoginRegisterComponent,
      ),
  },

  {
    path: 'notifications',
    loadComponent: () =>
      import(
        './Notification/componant/notification-page/notification-page.component'
      ).then((m) => m.NotificationPageComponent),
  },

  {
    path: 'forgot-password',
    loadComponent: () =>
      import(
        './Auth/components/forgot-password/forgot-password.component'
      ).then((m) => m.ForgotPasswordComponent),
  },

  {
    path: 'reset-password',
    loadComponent: () =>
      import('./Auth/components/reset-password/reset-password.component').then(
        (m) => m.ResetPasswordComponent,
      ),
  },

  {
    path: 'book-details/:id',
    loadComponent: () =>
      import('./Book/components/book-details/book-details.component').then(
        (m) => m.BookDetailsComponent,
      ),
  },
  {
    path: 'sidebar',
    loadComponent: () =>
      import('./shared/components/sidebar/sidebar.component').then(
        (m) => m.SidebarComponent,
      ),
  },

  {
    path: 'home',
    loadComponent: () =>
      import('./Home/components/home-page/home-page.component').then(
        (m) => m.HomePageComponent,
      ),
  },

  {
    path: '**',
    loadComponent: () =>
      import('./Auth/components/login-register/login-register.component').then(
        (m) => m.LoginRegisterComponent,
      ),
  },
];
@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
