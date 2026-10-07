export interface GetPagedCommentsRequest {
    pageNumber: number,
    pageSize?: number,
    sortBy?: string,
    desc?: boolean,
}
