import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { JournalEntryService } from 'src/app/core/services/journal-entry.service';
import { JournalEntry } from 'src/app/core/models/journal-entry.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';

@Component({
  selector: 'app-journal-entries',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './journal-entries.html',
  styleUrl: './journal-entries.scss',
})
export class JournalEntries implements OnInit {
  private journalEntryService = inject(JournalEntryService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);
  private router = inject(Router);

  readonly entries = signal<JournalEntry[]>([]);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.loadEntries();
  }

  loadEntries(): void {
    this.loading.set(true);

    this.journalEntryService.getList().subscribe({
      next: (result) => {
        this.entries.set(result.items);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(
            error,
            this.l10n.instant('Finance:JournalEntries:LoadFailed')
          )
        );
      },
    });
  }

  viewEntry(id: string): void {
    void this.router.navigate(['/finance/journal-entries', id]);
  }

  sourceTypeLabel(sourceType: string): string {
    const key = `Enum:JournalEntrySourceType:${sourceType}`;
    const translated = this.l10n.instant(key);
    return translated === key ? sourceType : translated;
  }
}
