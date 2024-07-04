import { Component, EventEmitter, Output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './search-bar.component.html',
  styleUrls: ['./search-bar.component.css'],
})
export class SearchBarComponent {
  @Output() search = new EventEmitter<string>();
  searchControl = new FormControl('');

  constructor() {}

  onSearch(event: Event): void {
    event.preventDefault();
    const value = this.searchControl.value;
    this.search.emit(value!.trim());
  }
}
