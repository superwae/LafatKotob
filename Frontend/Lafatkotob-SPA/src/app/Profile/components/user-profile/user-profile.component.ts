import { Component, OnInit } from '@angular/core';
import { BooksComponent } from '../../../Book/components/books/books.component';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar.component';
import { Book } from '../../../Book/Models/bookModel';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [BooksComponent, SidebarComponent, RouterOutlet],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css',
})
export class UserProfileComponent implements OnInit {
  userBooks: Book[] = [];

  constructor() {}

  ngOnInit(): void {}
}

