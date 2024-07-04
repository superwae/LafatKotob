import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalGenreComponent } from './modal-genre.component';

describe('ModalGenreComponent', () => {
  let component: ModalGenreComponent;
  let fixture: ComponentFixture<ModalGenreComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModalGenreComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ModalGenreComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
