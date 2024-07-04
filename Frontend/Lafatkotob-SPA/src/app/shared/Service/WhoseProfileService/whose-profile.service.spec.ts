import { TestBed } from '@angular/core/testing';

import { WhoseProfileService } from './whose-profile.service';

describe('WhoseProfileService', () => {
  let service: WhoseProfileService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WhoseProfileService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
