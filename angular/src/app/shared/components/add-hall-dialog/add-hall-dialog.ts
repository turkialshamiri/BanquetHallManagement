import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MatDialogRef,
  MAT_DIALOG_DATA
} from '@angular/material/dialog';

@Component({
  selector: 'app-add-hall-dialog',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './add-hall-dialog.html',
  styleUrl: './add-hall-dialog.scss'
})
export class AddHallDialog implements OnInit {

  dialogRef = inject(MatDialogRef<AddHallDialog>);

  data = inject(MAT_DIALOG_DATA, {
    optional: true
  });

  isEditMode = false;

  hall = {

    name: '',
    description: '',
    capacity: 0,
    location: '',
    pricePerHour: 0,
    status: 1,
    type: 1

  };

  ngOnInit(): void {

    if (this.data) {

      this.isEditMode = true;

      this.hall = {

        name: this.data.name ?? '',
        description: this.data.description ?? '',
        capacity: this.data.capacity ?? 0,
        location: this.data.location ?? '',
        pricePerHour: this.data.pricePerHour ?? 0,
        status: this.data.status ?? 1,
        type: this.data.type ?? 1

      };

    }

  }

  save(): void {

    this.dialogRef.close(this.hall);

  }

  close(): void {

    this.dialogRef.close();

  }

}