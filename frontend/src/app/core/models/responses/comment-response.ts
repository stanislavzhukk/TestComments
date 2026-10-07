import { AttachmentResponse } from "./attachment-response";

export interface CommentResponse {
    id: string,
    userName: string,
    userEmail: string,
    homePageUrl?: string,
    content: string,
    parentId?: string,
    createdAt: Date,
    replies: CommentResponse[],
    attachments: AttachmentResponse[]
}
