import {
  Component,
  ElementRef,
  Inject,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import {
  FormGroup,
  FormBuilder,
  Validators,
  ReactiveFormsModule,
  FormsModule,
} from '@angular/forms';
import { Book } from '../../Models/bookModel';
import { BookService } from '../../Service/BookService';
import { Genre } from '../../../Auth/Models/Genre';
import { CommonModule, DOCUMENT, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DropdownOption } from '../../../shared/Models/DropDownOption';

@Component({
  selector: 'app-edit-book',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, FormsModule],
  templateUrl: './edit-book.component.html',
  styleUrl: './edit-book.component.css',
})
export class EditBookComponent implements OnInit {
  bookForm!: FormGroup;
  selectedGenres: Genre[] = [];
  labels: DropdownOption[] = [];
  selectedImage: File | null = null;
  selectedImageUrl: string | null = null;
  currentSelection: DropdownOption | null = null;
  selectedOptions: number[] = [];
  bookId: number | null = null;
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
    { id: 15, name: 'Comics' },
    { id: 16, name: 'Religion' },
    { id: 17, name: 'Philosophy' },
    { id: 18, name: 'Education' },
    { id: 19, name: 'Politics' },
    { id: 20, name: 'Business' },
    { id: 21, name: 'Technology' },
    { id: 22, name: 'Sports' },
    { id: 23, name: 'True Crime' },
    { id: 24, name: 'Poetry' },
    { id: 25, name: 'Drama' },
    { id: 26, name: 'Adventure' },
    { id: 27, name: 'Nature' },
    { id: 28, name: 'Humor' },
    { id: 29, name: 'Lifestyle' },
    { id: 30, name: 'Economics' },
    { id: 31, name: 'Astronomy' },
    { id: 32, name: 'Linguistics' },
    { id: 33, name: 'Literature' },
    { id: 34, name: 'Short Story' },
    { id: 35, name: 'Novel' },
    { id: 36, name: 'Medicine' },
    { id: 37, name: 'Psychology' },
    { id: 38, name: 'Anime' },
  ];

  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private fb: FormBuilder,
    private bookService: BookService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
  ) {}

  ngOnInit(): void {
    this.document.body.classList.add('hide-scrollbar');
    const bookId = this.route.snapshot.params['id'];
    this.initForm();
    this.handleTypeChanges();
    this.bookService.getBookById(bookId).subscribe((book) => {
      if (book) {
        this.bookId = book.id;
        this.loadBookData(book);
        this.fetchGenresForBook(book.id);
      } else {
        this.router.navigate(['/books']);
      }
    });
  }

  initForm(): void {
    this.bookForm = this.fb.group({
      Title: ['', Validators.required],
      Author: ['', Validators.required],
      Description: [''],
      CoverImage: [null],
      PublicationDate: [null],
      ISBN: ['', [Validators.required, Validators.pattern(/^\d{13}$/)]],
      PageCount: [null],
      Condition: ['', Validators.required],
      Status: ['Available', Validators.required],
      Type: ['', Validators.required],
      PartnerUserId: [null],
      Language: ['', Validators.required],
      AddedDate: [new Date().toISOString()],
      GenreIds: [[]],
    });
  }

  loadBookData(book: Book): void {
    this.bookForm.patchValue({
      Title: book.title,
      Author: book.author,
      Description: book.description,
      PublicationDate: book.publicationDate
        ? new Date(book.publicationDate).toISOString().split('T')[0]
        : null,
      ISBN: book.isbn,
      PageCount: book.pageCount,
      Condition: book.condition,
      Status: book.status,
      Type: book.type,
      Language: book.language,
      AddedDate: new Date(book.addedDate).toISOString().split('T')[0], // Assuming AddedDate is also a DateTime
      GenreIds: this.selectedGenres.map((g) => g.id), // Assume you handle genre IDs elsewhere
    });
    this.selectedImageUrl = book.coverImage;
  }

  fetchGenresForBook(bookId: number): void {
    this.bookService.getGenresForBook(bookId).subscribe((genres) => {
      for (var genre of genres) {
        var DropdownOption: DropdownOption = { id: genre.id, name: genre.name };
        this.labels.push(DropdownOption);
      }
      this.currentSelection = null;
    });
  }

  onGenreToggle(genre: Genre): void {
    if (this.selectedGenres.includes(genre)) {
      this.selectedGenres = this.selectedGenres.filter(
        (g) => g.id !== genre.id,
      );
    } else {
      this.selectedGenres.push(genre);
    }
    this.bookForm
      .get('GenreIds')
      ?.setValue(this.selectedGenres.map((g) => g.id));
  }

  saveBook(): void {
    if (this.bookForm.valid) {
      const formData = new FormData();
      const userId = localStorage.getItem('userId');
      if (!userId) {
        console.error('User ID is missing in localStorage');
        return;
      }
      formData.append('UserId', userId);

      // Append book form fields to the FormData object
      Object.keys(this.bookForm.controls).forEach((key) => {
        const value = this.bookForm.get(key)?.value;
        if (key === 'GenreIds' && typeof value === 'object') {
          formData.append(key, JSON.stringify(value));
        } else {
          formData.append(key, value);
        }
      });
      formData.delete('GenreIds');
      // Append selected image if present
      if (this.selectedImage) {
        formData.append(
          'imageFile',
          this.selectedImage,
          this.selectedImage.name,
        );
      }
      formData.append('BookId', this.bookId!.toString());
      // Append genre IDs (from labels) as JSON
      const GenreIds = this.labels.map((label) => label.id);
      var ids = JSON.stringify(GenreIds);
      formData.append('GenreIds', ids.toString());

      // Send the FormData to the server using your existing book service
      this.bookService.updateBook(formData).subscribe({
        next: (response) => {
          this.router.navigate(['/home']); // Redirect or some other action
        },
        error: (error) => {
          console.error('Book update failed:', error);
        },
      });
    } else {
      console.error('Form is not valid');
    }
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedImage = file;
      const reader = new FileReader();
      reader.onload = (e: any) => (this.selectedImageUrl = e.target.result);
      reader.readAsDataURL(file);
    } else {
      // Reset to the original image URL if no file is selected
      this.selectedImage = null;
      this.selectedImageUrl = this.bookForm.get('CoverImage')!.value;
    }
  }

  removeSelectedImage(): void {
    this.selectedImage = null;
    this.selectedImageUrl = null;
  }

  close(): void {
    this.location.back();
  }

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
  onOptionSelected(option: DropdownOption): void {
    this.currentSelection = option;
  }
  handleTypeChanges(): void {
    const typeControl = this.bookForm.get('Type');
    if (typeControl) {
      typeControl.valueChanges.subscribe((type) => {});
    } else {
      console.error('Type control does not exist in the form');
    }
  }
  ngOnDestroy(): void {
    this.document.body.classList.remove('hide-scrollbar');
  }
}
