import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CommentResponse } from '../../core/models/responses/comment-response';
import { CommentsApiService } from '../../core/services/api/comments-api.service';
import { GetPagedCommentsRequest } from '../../core/models/requests/get-paged-comments-request';
import { CommentItemComponent } from './comment-item/comment-item.component';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, switchMap, timer } from 'rxjs';
import { CommentFormComponent } from './comment-form/comment-form.component';

@Component({
  selector: 'app-comments-list',
  imports: [CommentItemComponent, CommentFormComponent],
  templateUrl: './comments-list.component.html',
  styleUrls: ['./comments-list.component.css']
})
export class CommentsListComponent implements OnInit {
  private readonly commentsApi = inject(CommentsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly POLL_INTERVAL_MS = 5000;

  commentsList = signal<CommentResponse[]>([]);
  hasNextPage = signal<boolean>(false);
  hasPrevPage = signal<boolean>(false);
  page = signal<number>(1)

  paginationRequest = signal<GetPagedCommentsRequest>({
    pageNumber: 1,
    pageSize: 25,
    sortBy: 'createdAt',
    desc: true,
  });
  private readonly request$ = toObservable(this.paginationRequest);

  replyingToId = signal<string | null>(null);

  ngOnInit() {
    this.request$
      .pipe(
        switchMap(req =>
          timer(0, this.POLL_INTERVAL_MS).pipe(
            switchMap(() =>
              this.commentsApi.get(req).pipe(
                catchError(err => {
                  console.error(err);
                  return EMPTY;
                }),
              ),
            ),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(res => {
        this.commentsList.set(res.items);
        this.hasNextPage.set(res.hasNextPage);
        this.hasPrevPage.set(res.hasPreviousPage);
        this.page.set(res.page);
      });
  }

  changePage(newPage: number): void {
    this.paginationRequest.update(cur => ({ ...cur, pageNumber: newPage }));
  }

  handleReplyTo(comment: CommentResponse) {
    this.replyingToId.update(id => id === comment.id ? null : comment.id);
  }

  onCommentCreated() {
    this.replyingToId.set(null);
  }
}