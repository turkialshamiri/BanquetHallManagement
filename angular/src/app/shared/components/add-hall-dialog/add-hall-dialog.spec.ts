import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddHallDialog } from './add-hall-dialog';

describe('AddHallDialog', () => {
  let component: AddHallDialog;
  let fixture: ComponentFixture<AddHallDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddHallDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddHallDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
