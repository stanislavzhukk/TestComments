import { Component, computed, input, output } from '@angular/core';
import { CommentResponse } from '../../../core/models/responses/comment-response';
import { DatePipe } from '@angular/common';
import { AttachmentType } from '../../../core/models/responses/attachment-response';
import { CommentFormComponent } from '../comment-form/comment-form.component';

@Component({
  selector: 'app-comment-item',
  imports: [DatePipe, CommentFormComponent],
  templateUrl: './comment-item.component.html',
  styleUrl: './comment-item.component.css'
})
export class CommentItemComponent {
  readonly comment = input.required<CommentResponse>();
  readonly depth = input<number>(0);
  readonly activeReplyId = input<string | null>(null);
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

  readonly maxIndentDepth = 5;
  readonly AttachmentType = AttachmentType;
}