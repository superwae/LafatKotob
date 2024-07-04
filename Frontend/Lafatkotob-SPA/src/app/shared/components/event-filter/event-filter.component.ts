import { EventService } from './../../../Event/Service/event.service';
import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EventModel } from '../../../Event/Models/EventModels';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-filter',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './event-filter.component.html',
  styleUrl: './event-filter.component.css',
})
export class EventFilterComponent {
  @Output() citySelect = new EventEmitter<string>();
  selectedCity: string = '';
  events: EventModel[] = [];

  constructor(
    private eventService: EventService,
    private cdRef: ChangeDetectorRef,
  ) {}

  onCityChange(): void {
    this.citySelect.emit(this.selectedCity);
  }
}
