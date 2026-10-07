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
  fileError = signal<string | null>(null);
  captchaImage = signal<string>('');
  file = signal<File | null>(null);
  isFileDragging = signal(false);

  private fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');
  private contentInput = viewChild<ElementRef<HTMLTextAreaElement>>('contentInput');

  form = this.fb.group({
    userName: ['', [Validators.required, Validators.maxLength(50)]],
    userEmail: ['', [Validators.required, Validators.email]],
    homePageUrl: ['', [Validators.pattern(/^https?:\/\/.+/)]],
    content: ['', [Validators.required]],
    captcha: this.fb.group({
      captchaId: [''],
      userInput: ['', [Validators.maxLength(5), Validators.minLength(5)]],
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
    this.setFile(input.files?.[0] ?? null);
  }

  onFileDragOver(event: DragEvent) {
    event.preventDefault();
    this.isFileDragging.set(true);
  }

  onFileDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isFileDragging.set(false);
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    this.isFileDragging.set(false);
    this.setFile(event.dataTransfer?.files[0] ?? null);
  }

  private setFile(file: File | null) {
    this.fileError.set(null);

    if (!file) {
      this.file.set(null);
      return;
    }

    const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase();
    if (!['.jpg', '.jpeg', '.gif', '.png', '.txt'].includes(extension)) {
      this.file.set(null);
      this.fileError.set('Allowed formats: JPG, GIF, PNG, TXT');
      return;
    }

    if (extension === '.txt' && file.size > 100 * 1024) {
      this.file.set(null);
      this.fileError.set('Text file must be 100 KB or less');
      return;
    }

    this.file.set(file);
  }

  clearFile() {
    this.file.set(null);
    this.fileError.set(null);
    this.isFileDragging.set(false);
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
    if (c.errors['minlength']) return 'Captcha must contain exactly 5 characters';
    return null;
  }

  private applyServerErrors(err: HttpErrorResponse) {
    const response = this.getErrorResponse(err);
    const errors = response?.errors;
    let hasFieldErrors = false;

    if (errors) {
      for (const [key, messages] of Object.entries(errors)) {
        const message = messages.find(Boolean);
        if (!message) continue;

      const path = key
        .split('.')
        .map(p => p.charAt(0).toLowerCase() + p.slice(1))
        .join('.');

      const control = this.form.get(path);
      if (control) {
        hasFieldErrors = true;
        control.setErrors({ server: message });
        control.markAsTouched();
      } else {
        this.generalError.set(message);
      }
      }
    }

    const generalMessage = response?.detail || response?.title || err.message;
    if (!hasFieldErrors) {
      this.generalError.set(generalMessage || 'Failed to submit the comment');
    }
  }

  private getErrorResponse(err: HttpErrorResponse): {
    title?: string;
    detail?: string;
    errors?: Record<string, string[]>;
  } | null {
    if (!err.error || typeof err.error !== 'object') return null;

    const response = err.error as {
      title?: unknown;
      detail?: unknown;
      errors?: unknown;
    };

    const errors: Record<string, string[]> = {};
    if (response.errors && typeof response.errors === 'object') {
      for (const [key, value] of Object.entries(response.errors as Record<string, unknown>)) {
        if (Array.isArray(value)) {
          const messages = value.filter((message): message is string => typeof message === 'string');
          if (messages.length) errors[key] = messages;
        } else if (typeof value === 'string') {
          errors[key] = [value];
        }
      }
    }

    return {
      title: typeof response.title === 'string' ? response.title : undefined,
      detail: typeof response.detail === 'string' ? response.detail : undefined,
      errors: Object.keys(errors).length ? errors : undefined,
    };
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