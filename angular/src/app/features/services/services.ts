import {
  AfterViewInit,
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ServicesTableComponent } from './components/services-table/services-table';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [ServicesTableComponent],
  templateUrl: './services.html',
  styleUrl: './services.scss',
})
export class Services implements AfterViewInit {
  @ViewChild(ServicesTableComponent)
  servicesTable!: ServicesTableComponent;

  private route = inject(ActivatedRoute);
  private router = inject(Router);

  ngAfterViewInit(): void {
    const shouldOpenAddDialog =
      this.route.snapshot.data['openAddDialog'] === true;

    if (!shouldOpenAddDialog) {
      return;
    }

    this.servicesTable.openAddServiceDialog();

    this.router.navigate(['/services'], { replaceUrl: true });
  }
}
