import { ComponentFixture, TestBed } from '@angular/core/testing';
import { wishlistComponent } from './wishlist.component';

describe('WishlistComponent', () => {
  let component: wishlistComponent;
  let fixture: ComponentFixture<wishlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [wishlistComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(wishlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
