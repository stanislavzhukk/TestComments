/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { CaptchaApiService } from './captcha-api.service';

describe('Service: CaptchaApi', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CaptchaApiService]
    });
  });

  it('should ...', inject([CaptchaApiService], (service: CaptchaApiService) => {
    expect(service).toBeTruthy();
  }));
});
