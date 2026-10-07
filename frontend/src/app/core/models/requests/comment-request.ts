export interface CaptchaRequest {
    captchaId: string;
    userInput: string;
}

export interface CommentRequest {
    userName: string;
    userEmail: string;
    homePageUrl?: string;
    content: string;
    parentCommentId?: string | null;
    captcha: CaptchaRequest;
    file?: File | null;
}