export interface Result<T> {
    isSuccess: boolean;
    isFailure: boolean;
    value: T
}
