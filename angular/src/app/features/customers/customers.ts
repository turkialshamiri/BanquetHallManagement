import {
  AfterViewInit,
  Component,
  inject,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Navbar } from 'src/app/layout/navbar/navbar';
import { Sidebar } from 'src/app/layout/sidebar/sidebar';
import { CustomersTableComponent } from './components/customers-table/customers-table';

@Component({
  selector: 'app-customers',
  imports: [Sidebar, Navbar, CustomersTableComponent],
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
