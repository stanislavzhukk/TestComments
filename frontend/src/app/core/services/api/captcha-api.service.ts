import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CaptchaResponse } from '../../models/responses/captcha-response';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CaptchaApiService {
  private readonly http = inject(HttpClient);

  getCaptcha(): Observable<CaptchaResponse> {
    return this.http.get<CaptchaResponse>(`${environment.apiUrl}/api/captcha/generate`);
  }
}