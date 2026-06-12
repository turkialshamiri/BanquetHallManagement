import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { JournalEntryService } from 'src/app/core/services/journal-entry.service';
import { JournalEntry } from 'src/app/core/models/journal-entry.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';

@Component({
  selector: 'app-journal-entry-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './journal-entry-detail.html',
  styleUrl: './journal-entry-detail.scss',
})
export class JournalEntryDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private journalEntryService = inject(JournalEntryService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);

  readonly entry = signal<JournalEntry | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }

    this.journalEntryService.get(id).subscribe({
      next: (data) => {
        this.entry.set(data);
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

  sourceTypeLabel(sourceType: string): string {
    const key = `Enum:JournalEntrySourceType:${sourceType}`;
    const translated = this.l10n.instant(key);
    return translated === key ? sourceType : translated;
  }

  print(): void {
    window.print();
  }
}
