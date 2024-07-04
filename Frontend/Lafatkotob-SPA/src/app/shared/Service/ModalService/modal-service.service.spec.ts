import { TestBed } from '@angular/core/testing';

import { ModaleService } from './modal.service';

describe('ModalServiceService', () => {
  let service: ModaleService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ModaleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
