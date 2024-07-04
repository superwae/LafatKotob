import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MagicTextComponent } from './magic-text.component';

describe('MagicTextComponent', () => {
  let component: MagicTextComponent;
  let fixture: ComponentFixture<MagicTextComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MagicTextComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MagicTextComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
