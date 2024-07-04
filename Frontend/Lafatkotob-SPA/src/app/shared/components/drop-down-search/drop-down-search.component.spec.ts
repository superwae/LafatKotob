import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DropDownSearchComponent } from './drop-down-search.component';

describe('DropDownSearchComponent', () => {
  let component: DropDownSearchComponent;
  let fixture: ComponentFixture<DropDownSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DropDownSearchComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DropDownSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
