import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DEFAULT_PAGE_SIZE } from 'src/app/core/constants/pagination.constants';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import {
  getRangeEnd,
  getRangeStart,
  getTotalPages,
  getVisiblePages,
} from 'src/app/core/utils/pagination.util';

@Component({
  selector: 'app-data-table-pagination',
  standalone: true,
  imports: [CommonModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './data-table-pagination.html',
  styleUrl: './data-table-pagination.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTablePaginationComponent {
  private readonly l10n = inject(AppLocalizationService);

  readonly totalCount = input.required<number>();
  readonly pageIndex = input(0);
  readonly pageSize = input(DEFAULT_PAGE_SIZE);

  readonly pageChange = output<number>();

  readonly totalPages = computed(() =>
    getTotalPages(this.totalCount(), this.pageSize())
  );
  readonly visiblePages = computed(() =>
    getVisiblePages(this.pageIndex(), this.totalPages())
  );

  readonly rangeStart = computed(() =>
    getRangeStart(this.pageIndex(), this.totalCount(), this.pageSize())
  );

  readonly rangeEnd = computed(() =>
    getRangeEnd(this.pageIndex(), this.totalCount(), this.pageSize())
  );

  readonly isFirstPage = computed(() => this.pageIndex() <= 0);
  readonly isLastPage = computed(() => {
    const totalPages = this.totalPages();
    return totalPages === 0 || this.pageIndex() >= totalPages - 1;
  });

  rangeLabel(): string {
    return this.l10n.instant(
      'Pagination:ShowingRange',
      String(this.rangeStart()),
      String(this.rangeEnd()),
      String(this.totalCount)
    );
  }

  pageMetaLabel(): string {
    const currentPage = this.totalCount() > 0 ? this.pageIndex() + 1 : 0;

    return this.l10n.instant(
      'Pagination:PageMeta',
      String(this.totalCount()),
      String(currentPage),
      String(this.totalPages())
    );
  }

  goToPage(page: number): void {
    if (page === this.pageIndex() || page < 0 || page >= this.totalPages()) {
      return;
    }

    this.pageChange.emit(page);
  }

  goPrevious(): void {
    if (!this.isFirstPage()) {
      this.pageChange.emit(this.pageIndex() - 1);
    }
  }

  goNext(): void {
    if (!this.isLastPage()) {
      this.pageChange.emit(this.pageIndex() + 1);
    }
  }

  isEllipsis(token: number | 'ellipsis'): token is 'ellipsis' {
    return token === 'ellipsis';
  }
}
