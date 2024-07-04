import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DropdownOption } from '../../Models/DropDownOption';

@Component({
  selector: 'app-drop-down-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drop-down-search.component.html',
  styleUrl: './drop-down-search.component.css',
})
export class DropDownSearchComponent {
  @Input() dropDownList: DropdownOption[] = [];
  @Output() selectedItem = new EventEmitter<DropdownOption>();
  searchTerm: string = '';
  constructor() {}

  ngOnInit() {}

  getSelectedItem(): void {
    const foundItem = this.dropDownList.find(
      (item) => item.name === this.searchTerm,
    );
    if (foundItem) {
      this.selectedItem.emit(foundItem);
    }
  }
}
