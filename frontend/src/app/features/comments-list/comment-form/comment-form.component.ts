// comment-form.component.ts
import { Component, ElementRef, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommentsApiService } from '../../../core/services/api/comments-api.service';
import { CaptchaApiService } from '../../../core/services/api/captcha-api.service'; // assumed path

@Component({
  selector: 'app-comment-form',
  imports: [ReactiveFormsModule],
  templateUrl: './comment-form.component.html',
  styleUrls: ['./comment-form.component.css'],
})
export class CommentFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly commentsApi = inject(CommentsApiService);
  private readonly captchaApi = inject(CaptchaApiService);

  parentId = input<string | null>(null);
  created = output<void>();
  cancelled = output<void>();

  isSubmitting = signal(false);
  generalError = signal<string | null>(null);
  captchaImage = signal<string>('');
  file = signal<File | null>(null);

  private fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');
  private contentInput = viewChild<ElementRef<HTMLTextAreaElement>>('contentInput');

  form = this.fb.group({
    userName: ['', [Validators.required, Validators.maxLength(50)]],
    userEmail: ['', [Validators.required, Validators.email]],
    homePageUrl: ['', [Validators.pattern(/^https?:\/\/.+/)]],
    content: ['', [Validators.required]],
    captcha: this.fb.group({
      captchaId: [''],
      userInput: ['', [Validators.required]],
    }),
  });

  ngOnInit() {
    this.loadCaptcha();
  }

  loadCaptcha() {
    this.captchaApi.getCaptcha().subscribe(c => {
      this.captchaImage.set(c.image);
      this.form.controls.captcha.patchValue({ captchaId: c.id, userInput: '' });
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.file.set(input.files?.[0] ?? null);
  }

  clearFile() {
    this.file.set(null);
    const input = this.fileInput()?.nativeElement;
    if (input) input.value = '';
  }

  formatFileSize(size: number): string {
    if (size < 1024) return `${size} B`;
    if (size < 1024 * 1024) return `${Math.round(size / 1024)} KB`;
    return `${(size / (1024 * 1024)).toFixed(1)} MB`;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.generalError.set(null);

    this.commentsApi
      .create({
        ...this.form.getRawValue(),
        parentCommentId: this.parentId(),
        file: this.file(),
      })
      .subscribe({
        next: () => {
          this.resetForm();
          this.created.emit();
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.applyServerErrors(err);
          this.loadCaptcha();
        },
      });
  }

  errorOf(path: string): string | null {
    const c = this.form.get(path);
    if (!c || !c.touched || !c.errors) return null;
    if (c.errors['server']) return c.errors['server'];
    if (c.errors['required']) return 'This field is required';
    if (c.errors['email']) return 'Invalid email address';
    if (c.errors['pattern']) return 'Invalid value';
    if (c.errors['maxlength']) return 'Value is too long';
    return null;
  }

  private applyServerErrors(err: HttpErrorResponse) {
    const errors = err.error?.errors as Record<string, string[]> | undefined;

    if (err.status !== 400 || !errors) {
      this.generalError.set('Failed to submit the comment');
      return;
    }

    for (const [key, messages] of Object.entries(errors)) {
      const path = key
        .split('.')
        .map(p => p.charAt(0).toLowerCase() + p.slice(1))
        .join('.');

      const control = this.form.get(path);
      if (control) {
        control.setErrors({ server: messages[0] });
        control.markAsTouched();
      } else {
        this.generalError.set(messages[0]);
      }
    }
  }

  wrapSelection(tag: 'i' | 'strong' | 'code' | 'a') {
    const el = this.contentInput()?.nativeElement;
    if (!el) return;

    const start = el.selectionStart;
    const end = el.selectionEnd;
    const selected = el.value.slice(start, end);

    const hrefPrefix = '<a href="';
    const open = tag === 'a' ? `${hrefPrefix}https://">` : `<${tag}>`;
    const close = `</${tag}>`;

    el.setRangeText(open + selected + close, start, end, 'end');
    this.form.controls.content.setValue(el.value);
    el.focus();

    if (tag === 'a') {
      const urlStart = start + hrefPrefix.length;
      el.setSelectionRange(urlStart, urlStart + 'https://'.length);
    } else if (selected) {
      el.setSelectionRange(start + open.length, start + open.length + selected.length);
    } else {
      const caret = start + open.length;
      el.setSelectionRange(caret, caret);
    }
}

  private resetForm() {
    this.form.reset();
    this.clearFile();
    this.isSubmitting.set(false);
    this.loadCaptcha();
  }
}