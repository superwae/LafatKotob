import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserPreferenceComponent } from './user-preference.component';

describe('UserPreferenceComponent', () => {
  let component: UserPreferenceComponent;
  let fixture: ComponentFixture<UserPreferenceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserPreferenceComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(UserPreferenceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
