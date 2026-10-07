export interface AttachmentResponse {
    type: AttachmentType,
    url: string,
    fileName: string,
}

export enum AttachmentType {
    Image = 'Image',
    Document = 'Document',
}
