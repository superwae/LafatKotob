import { TestBed } from '@angular/core/testing';

import { AppUsereService } from './app-user.service';

describe('AppUserServiceService', () => {
  let service: AppUsereService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppUsereService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
