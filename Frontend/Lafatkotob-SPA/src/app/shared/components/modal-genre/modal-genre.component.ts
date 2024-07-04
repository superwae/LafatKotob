import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DropDownSearchComponent } from '../drop-down-search/drop-down-search.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DropdownOption } from '../../Models/DropDownOption';
import { ModaleService } from '../../Service/ModalService/modal.service';
import { BookService } from '../../../Book/Service/BookService';

@Component({
  selector: 'app-modal-genre',
  standalone: true,
  imports: [
    DropDownSearchComponent,
    FormsModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './modal-genre.component.html',
  styleUrl: './modal-genre.component.css',
})
export class ModalGenreComponent {
  finish: boolean = false;
  finish2: boolean = true;
  currentSelection: DropdownOption | null = null;
  labels: DropdownOption[] = [];
  selectedImage: File | null = null;
  selectedImageUrl: string | null = null;
  @Output() closeEvent2 = new EventEmitter<void>();
  @Output() addAnotherBookEvent = new EventEmitter<void>();
  @Input() registrationData: any;

  allOptions: DropdownOption[] = [
    { id: 1, name: 'History' },
    { id: 2, name: 'Romance' },
    { id: 3, name: 'Science Fiction' },
    { id: 4, name: 'Fantasy' },
    { id: 5, name: 'Thriller' },
    { id: 6, name: 'Young Adult' },
    { id: 7, name: 'Children' },
    { id: 8, name: 'Science' },
    { id: 9, name: 'Horror' },
    { id: 10, name: 'Nonfiction' },
    { id: 11, name: 'Health' },
    { id: 12, name: 'Travel' },
    { id: 13, name: 'Cooking' },
    { id: 14, name: 'Art' },
    { id: 38, name: 'Comics' },
    { id: 15, name: 'Religion' },
    { id: 16, name: 'Philosophy' },
    { id: 17, name: 'Education' },
    { id: 18, name: 'Politics' },
    { id: 19, name: 'Business' },
    { id: 20, name: 'Technology' },
    { id: 37, name: 'Sports' },
    { id: 21, name: 'True Crime' },
    { id: 36, name: 'Poetry' },
    { id: 22, name: 'Drama' },
    { id: 23, name: 'Adventure' },
    { id: 24, name: 'Nature' },
    { id: 25, name: 'Humor' },
    { id: 26, name: 'Lifestyle' },
    { id: 27, name: 'Economics' },
    { id: 28, name: 'Astronomy' },
    { id: 29, name: 'Linguistics' },
    { id: 30, name: 'Literature' },
    { id: 31, name: 'Short Story' },
    { id: 32, name: 'Novel' },
    { id: 33, name: 'Medicine' },
    { id: 34, name: 'Psychology' },
    { id: 35, name: 'Anime' },
  ];
  selectedOptions: number[] = [];
  constructor(
    private modalService: ModaleService,
    private bookService: BookService,
  ) { }

  addLabel(): void {
    if (this.labels.length >= 8) {
      alert('You can select at most 8 genres.');
      return;
    }

    if (this.currentSelection && !this.labels.includes(this.currentSelection)) {
      this.labels.push(this.currentSelection);
      this.currentSelection = null;
    } else {
      alert('Please select a valid option not already added.');
    }
  }

  removeLabel(index: number): void {
    this.labels.splice(index, 1);
  }

  saveSelections(): void {
    if (this.labels.length < 1) {
      alert('Please select at least 1 genre.');
      return;
    }
    if (!this.selectedImage) {
      alert('Please select a picture.');
      return;
    }
    if (this.selectedImage) {
      this.registrationData.append(
        'imageFile',
        this.selectedImage,
        this.selectedImage.name,
      );
    }

    const genreIds = this.labels.map((label) => label.id);
    this.registrationData.append('GenreIds', JSON.stringify(genreIds));

    this.bookService.registerBookWithGenres(this.registrationData).subscribe({
      next: (response) => {
        this.showConfirmationPopup();
      },
      error: (error) => {
        console.error('Registration failed:', error);
      },
    });
  }
  onOptionSelected(option: DropdownOption): void {
    this.currentSelection = option;
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    const files = target.files;

    if (files && files.length) {
      const file = files[0];
      this.selectedImage = file;

      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        this.selectedImageUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }
  removeSelectedImage($event: Event) {
    $event.stopPropagation();
    this.selectedImage = null;
    this.selectedImageUrl = null;
  }

  showConfirmationPopup(): void {
    this.finish = true;
  }
  close() {
    this.closeEvent2.emit();
  }

  addAnotherBook(): void {
    this.addAnotherBookEvent.emit();
  }

  closePopup(): void {
    this.closeEvent2.emit();
  }
}
