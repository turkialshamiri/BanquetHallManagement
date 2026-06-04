import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HallsTable } from './halls-table';

describe('HallsTable', () => {
  let component: HallsTable;
  let fixture: ComponentFixture<HallsTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HallsTable]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HallsTable);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
