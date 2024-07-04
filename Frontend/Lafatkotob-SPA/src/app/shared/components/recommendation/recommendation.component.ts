import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Book } from '../../../Book/Models/bookModel';
import { BookService } from '../../../Book/Service/BookService';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { GetGenreForUserModel } from '../../../Book/Models/GenresForMyBook';
@Component({
  selector: 'app-recommendation',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './recommendation.component.html',
  styleUrls: ['./recommendation.component.css'],
})
export class RecommendationComponent implements OnInit, AfterViewInit {
  books: Book[] = [];
  defaultImageUrl: string = 'assets/images/defaultCover.jpg';
  isDragging: boolean = false;
  dragStartX: number = 0;
  dragThreshold: number = 1;
  significantDragOccurred: boolean = false;
  usreId = localStorage.getItem('userId')?.toString();
  Genres: GetGenreForUserModel | undefined;
  bookCoverImageAlt: string = 'Default book cover image';

  constructor(
    private bookService: BookService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.bookService.getMyBookGenres(this.usreId!).subscribe({
      next: (genres: GetGenreForUserModel) => {
        this.Genres = genres;
        this.bookService.getRecommendedBooks(this.Genres).subscribe({
          next: (recommendedBooks: any[]) => {
            const isbnList = recommendedBooks.map((book) => book.ISBN);

            this.bookService.GetBooksByIsbn(isbnList).subscribe({
              next: (books: Book[]) => {
                this.books = books;
              },
              error: (err) =>
                console.error('Error fetching books by ISBN:', err),
            });
          },
          error: (err) =>
            console.error('Error fetching recommended books:', err),
        });
      },
      error: (err) => console.error('Error fetching genres:', err),
    });
  }

  ngAfterViewInit(): void {
    this.initializeTrackScroll();
    this.setInitialImagePositions();
    this.addImagePressFeedback();
  }
  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/defaultCover.jpg';
    this.bookCoverImageAlt = 'Default book cover image';
  }
  onImageLoad(event: any): void {
    const img: HTMLImageElement = event.target;

    // Check if the image is too small
    if (img.naturalWidth <= 1 || img.naturalHeight <= 1) {
      img.src = this.defaultImageUrl;
      return;
    }

    // Use a canvas to check if the image is blank
    const canvas = document.createElement('canvas');
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    const ctx = canvas.getContext('2d');
    ctx!.drawImage(img, 0, 0);

    const imageData = ctx!.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData.data;
    let isBlank = true;

    for (let i = 0; i < data.length; i += 4) {
      if (data[i] !== 255 || data[i + 1] !== 255 || data[i + 2] !== 255) {
        // not white
        isBlank = false;
        break;
      }
    }

    if (isBlank) {
      img.src = this.defaultImageUrl;
    }
  }

  private addImagePressFeedback(): void {
    const images = document.querySelectorAll('#image-track img');
    images.forEach((img) => {
      img.addEventListener('mousedown', (e) => {
        e.preventDefault();
        img.classList.add('image-pressed');
      });

      img.addEventListener('mouseup', () => {
        img.classList.remove('image-pressed');
      });

      img.addEventListener('touchstart', (e) => {
        e.preventDefault();
        img.classList.add('image-pressed');
      });

      img.addEventListener('touchend', () => {
        img.classList.remove('image-pressed');
      });
    });
  }

  private setInitialImagePositions(): void {
    const track = document.getElementById('image-track');
    if (track) {
      const images = Array.from(
        track.getElementsByClassName('image') as HTMLCollectionOf<HTMLElement>,
      );
      images.forEach((image, index) => {
        const initialRightOffset = 100 - index * 10;
        image.setAttribute(
          'data-initial-right-offset',
          `${initialRightOffset}%`,
        );
        image.style.objectPosition = `${initialRightOffset}% center`;
      });
    }
  }

  private initializeTrackScroll(): void {
    const track = document.getElementById('image-track');

    const handleOnDown = (e: MouseEvent | TouchEvent) => {
      const clientX =
        e instanceof MouseEvent ? e.clientX : e.touches[0].clientX;
      this.dragStartX = clientX;
      this.significantDragOccurred = false;
      if (track) track.setAttribute('data-mouse-down-at', String(clientX));
    };

    const handleOnUp = () => {
      if (!track) return;
      this.dragStartX = 0;
      this.isDragging = false;

      const currentPercentage = track.getAttribute('data-percentage') || '0';

      track.setAttribute('data-prev-percentage', currentPercentage);

      track.setAttribute('data-mouse-down-at', '0');

      if (
        Math.abs(
          this.dragStartX -
            parseFloat(track.getAttribute('data-mouse-down-at') || '0'),
        ) <= this.dragThreshold
      ) {
        this.isDragging = false;
      }

      this.dragStartX = 0;
    };
    const handleOnLeave = () => {
      handleOnUp();
    };

    const handleOnMove = (e: MouseEvent | TouchEvent) => {
      if (!track) return;
      this.isDragging = true;

      const mouseDownAt = parseFloat(
        track.getAttribute('data-mouse-down-at') || '0',
      );
      if (mouseDownAt === 0) return;

      const clientX =
        e instanceof MouseEvent ? e.clientX : e.touches[0].clientX;
      const mouseDelta = mouseDownAt - clientX;
      const maxDelta = window.innerWidth / 2;
      const percentage = (mouseDelta / maxDelta) * -50;
      const prevPercentage = parseFloat(
        track.getAttribute('data-prev-percentage') || '0',
      );
      let nextPercentage = prevPercentage + percentage;

      // Limit the movement to the right
      nextPercentage = Math.max(nextPercentage, -65);

      nextPercentage = Math.min(Math.max(nextPercentage, -100), 0);

      track.setAttribute('data-percentage', String(nextPercentage));
      track.style.transform = `translate(${nextPercentage}%, -50%)`;

      const images = Array.from(
        track.getElementsByClassName('image') as HTMLCollectionOf<HTMLElement>,
      );
      images.forEach((image) => {
        const initialRightOffset = parseFloat(
          image.getAttribute('data-initial-right-offset') || '100',
        );
        const adjustedPosition = initialRightOffset + nextPercentage;
        image.style.objectPosition = `${adjustedPosition}% center`;

        const movementX = Math.abs(clientX - this.dragStartX);
        if (movementX > this.dragThreshold) {
          this.significantDragOccurred = true;
        }
      });
    };

    if (track) {
      track.addEventListener('mousedown', handleOnDown);
      track.addEventListener('mouseup', handleOnUp);
      track.addEventListener('mousemove', handleOnMove);
      track.addEventListener('mouseleave', handleOnLeave);
      track.addEventListener('touchstart', handleOnDown as any);
      track.addEventListener('touchend', handleOnUp as any);
      track.addEventListener('touchmove', handleOnMove as any);
      track.addEventListener('touchcancel', handleOnLeave as any);
    }
    track?.classList.remove('track-animate');
  }

  scrollLeft(): void {
    this.scrollTrack(10);
  }

  scrollRight(): void {
    this.scrollTrack(-10);
  }

  private scrollTrack(delta: number): void {
    const track = document.getElementById('image-track');
    if (track) {
      let currentPercentage = parseFloat(
        track.getAttribute('data-percentage') || '0',
      );
      currentPercentage += delta;

      const maxScroll = this.calculateMaxScroll();
      currentPercentage = Math.min(Math.max(currentPercentage, maxScroll), 0);

      track.classList.add('track-animate');
      track.style.transform = `translate(${currentPercentage}%, -50%)`;
      track.setAttribute('data-percentage', `${currentPercentage}`);

      this.updateImagePositions(currentPercentage);

      setTimeout(() => track.classList.remove('track-animate'), 500);

      track.removeAttribute('data-mouse-down-at');
    }
  }

  private calculateMaxScroll(): number {
    const maxScroll = -((this.books.length - 1) * 10);
    return maxScroll;
  }

  private updateImagePositions(currentPercentage: number): void {
    const track = document.getElementById('image-track');
    if (track) {
      const images = Array.from(
        track.getElementsByClassName('image') as HTMLCollectionOf<HTMLElement>,
      );
      images.forEach((image) => {
        const initialRightOffset = parseFloat(
          image.getAttribute('data-initial-right-offset') || '100',
        );
        const adjustedPosition = initialRightOffset + currentPercentage;
        image.style.objectPosition = `${adjustedPosition}% center`;
      });
    }
  }

  handleImageClick(event: MouseEvent, book: Book): void {
    if (this.significantDragOccurred) {
      event.preventDefault();
      event.stopPropagation();
    } else {
      this.router.navigate(['/book-details', book.id]);
    }
    this.significantDragOccurred = false;
  }
}
