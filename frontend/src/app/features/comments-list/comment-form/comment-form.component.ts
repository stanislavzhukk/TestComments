import { Component, ElementRef, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommentsApiService } from '../../../core/services/api/comments-api.service';
import { CaptchaApiService } from '../../../core/services/api/captcha-api.service';
import { isValidationProblem, ProblemDetails } from '../../../core/models/problem-details.model';

const CAPTCHA_LENGTH = 5;
const ALLOWED_EXTENSIONS = ['.jpg', '.jpeg', '.gif', '.png', '.txt'];
const MAX_TEXT_FILE_BYTES = 100 * 1024;

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

  readonly captchaLength = CAPTCHA_LENGTH;

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
      userInput: ['', [Validators.minLength(CAPTCHA_LENGTH), Validators.maxLength(CAPTCHA_LENGTH)]],
    }),
  });

  ngOnInit() {
    this.loadCaptcha();
  }

  loadCaptcha(serverError?: string | null) {
    this.captchaApi.getCaptcha().subscribe({
      next: c => {
        this.captchaImage.set(c.image);

        const captcha = this.form.controls.captcha;
        captcha.patchValue({ captchaId: c.id, userInput: '' });

        if (serverError) {
          captcha.controls.userInput.setErrors({ server: serverError });
          captcha.controls.userInput.markAsTouched();
        }
      },
      error: () => {
        if (serverError) this.generalError.set(serverError);
      },
    });
  }

  openFilePicker() {
    this.fileInput()?.nativeElement.click();
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

    const dot = file.name.lastIndexOf('.');
    const extension = dot >= 0 ? file.name.slice(dot).toLowerCase() : '';

    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      this.file.set(null);
      this.fileError.set('Allowed formats: JPG, GIF, PNG, TXT');
      return;
    }

    if (extension === '.txt' && file.size > MAX_TEXT_FILE_BYTES) {
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
          const captchaMessage = this.applyServerErrors(err);
          this.loadCaptcha(captchaMessage);
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
    if (c.errors['minlength']) return `Captcha must contain exactly ${CAPTCHA_LENGTH} characters`;
    return null;
  }

  private applyServerErrors(err: HttpErrorResponse): string | null {
    const body = typeof err.error === 'object' ? (err.error as ProblemDetails | null) : null;
    let handled = false;
    let captchaMessage: string | null = null;

    if (isValidationProblem(body)) {
      for (const [key, messages] of Object.entries(body.errors)) {
        const message = messages[0];
        if (!message) continue;
        handled = true;

        if (key === 'captcha.userInput') {
          captchaMessage = message;
          continue;
        }

        const control = this.form.get(key);
        if (control) {
          control.setErrors({ server: message });
          control.markAsTouched();
        } else {
          this.generalError.set(message);
        }
      }
    }

    if (!handled) {
      this.generalError.set(body?.detail || body?.title || 'Failed to submit the comment');
    }

    return captchaMessage;
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