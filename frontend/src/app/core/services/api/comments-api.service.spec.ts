/* tslint:disable:no-unused-variable */

import { TestBed, inject } from '@angular/core/testing';
import { CommentsApiService } from './comments-api.service';

describe('Service: CommentsApi', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CommentsApiService]
    });
  });

  it('should ...', inject([CommentsApiService], (service: CommentsApiService) => {
    expect(service).toBeTruthy();
  }));
});
