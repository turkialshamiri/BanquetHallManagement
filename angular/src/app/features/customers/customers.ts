import {
  AfterViewInit,
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomersTableComponent } from './components/customers-table/customers-table';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CustomersTableComponent],
  templateUrl: './customers.html',
  styleUrl: './customers.scss',
})
export class Customers implements AfterViewInit {
  @ViewChild(CustomersTableComponent)
  customersTable!: CustomersTableComponent;

  private route = inject(ActivatedRoute);
  private router = inject(Router);

  ngAfterViewInit(): void {
    const shouldOpenAddDialog =
      this.route.snapshot.data['openAddDialog'] === true;

    if (!shouldOpenAddDialog) {
      return;
    }

    this.customersTable.openAddCustomerDialog();

    this.router.navigate(['/customers'], { replaceUrl: true });
  }
}
