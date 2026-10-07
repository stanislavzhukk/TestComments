import { Component, computed, HostListener, input, output, signal } from '@angular/core';
import { CommentResponse } from '../../../core/models/responses/comment-response';
import { DatePipe } from '@angular/common';
import { AttachmentType } from '../../../core/models/responses/attachment-response';
import { CommentFormComponent } from '../comment-form/comment-form.component';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-comment-item',
  imports: [DatePipe, CommentFormComponent],
  templateUrl: './comment-item.component.html',
  styleUrl: './comment-item.component.css'
})
export class CommentItemComponent {
  readonly apiUrl = environment.apiUrl;
  readonly comment = input.required<CommentResponse>();
  readonly depth = input<number>(0);
  readonly activeReplyId = input<string | null>(null);
  readonly repliesExpanded = input(false);
  readonly initial = computed(() => this.comment().userName.charAt(0).toUpperCase());
  readonly avatarColor = computed(() => {
    let hue = 0;
    for (const ch of this.comment().userName){
      hue = (hue * 31 + ch.charCodeAt(0)) % 360;
    } 
    return `hsl(${hue} 55% 78%)`;
  });
  
  readonly replyTo = output<CommentResponse>();
  readonly created = output<void>();
  readonly cancelled = output<void>();
  readonly toggleReplies = output<string>();
  readonly lightboxIndex = signal<number | null>(null);

  readonly maxIndentDepth = 5;
  readonly AttachmentType = AttachmentType;

  readonly imageAttachments = computed(() =>
    this.comment().attachments.filter(attachment => attachment.type === AttachmentType.Image),
  );

  openLightbox(index: number, event: Event): void {
    event.preventDefault();
    this.lightboxIndex.set(index);
  }

  closeLightbox(): void {
    this.lightboxIndex.set(null);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.lightboxIndex() !== null) this.closeLightbox();
  }
}