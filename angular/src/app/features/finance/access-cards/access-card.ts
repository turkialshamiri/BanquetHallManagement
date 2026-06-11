import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppLocalizationPipe } from 'src/app/core/pipes/app-localization.pipe';
import { AppLocalizationService } from 'src/app/core/services/app-localization.service';
import { HallAccessCardService } from 'src/app/core/services/hall-access-card.service';
import { HallAccessCardPrintData } from 'src/app/core/models/hall-access-card.model';
import { getAbpErrorMessage } from 'src/app/core/utils/abp-error.util';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { toTimeInputValue } from 'src/app/core/models/reservation.model';

@Component({
  selector: 'app-access-card',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, AppLocalizationPipe],
  templateUrl: './access-card.html',
  styleUrl: './access-card.scss',
})
export class AccessCard implements OnInit {
  private route = inject(ActivatedRoute);
  private accessCardService = inject(HallAccessCardService);
  private notification = inject(NotificationService);
  private l10n = inject(AppLocalizationService);

  readonly card = signal<HallAccessCardPrintData | null>(null);
  readonly loading = signal(true);
  readonly formatTime = toTimeInputValue;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }

    this.accessCardService.getPrintData(id).subscribe({
      next: (data) => {
        this.card.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.notification.showError(
          getAbpErrorMessage(error, this.l10n.instant('Finance:AccessCard:LoadFailed'))
        );
      },
    });
  }

  print(): void {
    window.print();
  }
}
