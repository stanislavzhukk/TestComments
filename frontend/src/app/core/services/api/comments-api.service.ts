import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommentResponse } from '../../models/responses/comment-response';
import { PagedResult } from '../../models/paged-result.model';
import { GetPagedCommentsRequest } from '../../models/requests/get-paged-comments-request';
import { CommentRequest } from '../../models/requests/comment-request';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CommentsApiService {
  http = inject(HttpClient);

  constructor() { }

  create(req: CommentRequest) {
    const form = new FormData();
    form.append('UserName', req.userName);
    form.append('UserEmail', req.userEmail);
    form.append('Content', req.content);
    form.append('Captcha.CaptchaId', req.captcha.captchaId);
    form.append('Captcha.UserInput', req.captcha.userInput);

    if (req.homePageUrl){
      form.append('HomePageUrl', req.homePageUrl);
    } 
    if (req.parentCommentId){
      form.append('ParentCommentId', req.parentCommentId);
    } 
    if (req.file){
      form.append('File', req.file, req.file.name);
    } 
    return this.http.post<CommentResponse>(`${environment.apiUrl}/api/Comments`, form);
  }

  get(request: GetPagedCommentsRequest): Observable<PagedResult<CommentResponse[]>> {
    const params = new HttpParams({ fromObject: request as Record<string, any> });

    return this.http.get<PagedResult<CommentResponse[]>>(`${environment.apiUrl}/api/comments`, { params });
  }
}
