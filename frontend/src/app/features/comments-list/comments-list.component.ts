import { Component, DestroyRef, ElementRef, inject, OnInit, signal, viewChild } from '@angular/core';
import { CommentResponse } from '../../core/models/responses/comment-response';
import { CommentsApiService } from '../../core/services/api/comments-api.service';
import { GetPagedCommentsRequest } from '../../core/models/requests/get-paged-comments-request';
import { CommentItemComponent } from './comment-item/comment-item.component';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, merge, Subject, switchMap, timer } from 'rxjs';
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
  private readonly refresh$ = new Subject<void>();

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
  expandedReplies = signal<Set<string>>(new Set());
  private readonly commentsContainer = viewChild<ElementRef<HTMLElement>>('commentsContainer');

  ngOnInit() {
    this.request$
      .pipe(
        switchMap(req =>
          merge(
            timer(0, this.POLL_INTERVAL_MS),
            this.refresh$,
          ).pipe(
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
    this.commentsContainer()?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  changeSort(sortBy: string): void {
    this.paginationRequest.update(cur => ({
      ...cur,
      pageNumber: 1,
      sortBy,
      desc: cur.sortBy === sortBy ? !cur.desc : true,
    }));
  }

  setSortField(event: Event): void {
    const sortBy = (event.target as HTMLSelectElement).value;
    this.paginationRequest.update(cur => ({
      ...cur,
      pageNumber: 1,
      sortBy,
    }));
  }

  setSortDirection(event: Event): void {
    const desc = (event.target as HTMLSelectElement).value === 'desc';
    this.paginationRequest.update(cur => ({
      ...cur,
      pageNumber: 1,
      desc,
    }));
  }

  toggleReplies(commentId: string): void {
    this.expandedReplies.update(expanded => {
      const next = new Set(expanded);
      next.has(commentId) ? next.delete(commentId) : next.add(commentId);
      return next;
    });
  }

  isRepliesExpanded(commentId: string): boolean {
    return this.expandedReplies().has(commentId);
  }

  handleReplyTo(comment: CommentResponse) {
    this.replyingToId.update(id => id === comment.id ? null : comment.id);
  }

  onCommentCreated() {
    this.replyingToId.set(null);
    this.refresh$.next();
  }
}