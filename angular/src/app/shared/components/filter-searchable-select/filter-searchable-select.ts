import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { matchesSearchQuery } from 'src/app/core/utils/search-text.util';
import { FilterSearchableSelectItem } from './filter-searchable-select.model';

@Component({
  selector: 'app-filter-searchable-select',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatButtonModule,
  ],
  templateUrl: './filter-searchable-select.html',
  styleUrl: './filter-searchable-select.scss',
})
export class FilterSearchableSelectComponent implements OnChanges {
  @Input() icon = 'search';
  @Input() placeholder = '';
  @Input() allLabel = '';
  @Input() emptyLabel = '';
  @Input() items: FilterSearchableSelectItem[] = [];
  @Input() selectedId: string | null = null;

  @Output() selectedIdChange = new EventEmitter<string | null>();

  readonly searchText = signal('');
  readonly isFocused = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['selectedId'] || changes['items']) {
      this.syncInputFromSelection();
    }
  }

  filteredItems(): FilterSearchableSelectItem[] {
    const query = this.searchText();

    return this.items.filter((item) =>
      matchesSearchQuery(item.searchText ?? item.label, query)
    );
  }

  showEmptyState(): boolean {
    return (
      !!this.searchText().trim() &&
      this.filteredItems().length === 0
    );
  }

  hasSelection(): boolean {
    return !!this.selectedId;
  }

  onFocus(): void {
    this.isFocused.set(true);

    if (this.selectedId) {
      this.searchText.set('');
    }
  }

  onBlur(): void {
    this.isFocused.set(false);
    this.syncInputFromSelection();
  }

  onSearchInput(value: string): void {
    this.searchText.set(value);

    if (!value.trim() && this.selectedId) {
      this.selectedIdChange.emit(null);
    }
  }

  onOptionSelected(id: string | null): void {
    this.selectedIdChange.emit(id);
    this.searchText.set(id ? this.findLabel(id) : '');
    this.isFocused.set(false);
  }

  clearSelection(event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.selectedIdChange.emit(null);
    this.searchText.set('');
  }

  private syncInputFromSelection(): void {
    if (this.isFocused()) {
      return;
    }

    if (!this.selectedId) {
      this.searchText.set('');
      return;
    }

    this.searchText.set(this.findLabel(this.selectedId));
  }

  private findLabel(id: string): string {
    return this.items.find((item) => item.id === id)?.label ?? '';
  }
}
