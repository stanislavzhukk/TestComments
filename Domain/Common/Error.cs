using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Common
{
    public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure, 
        IReadOnlyDictionary<string, string[]>? Fields = null)
    {
        public static Error NotFound(string code, string message, IReadOnlyDictionary<string, string[]>? fields = null) => new(code, message, ErrorType.NotFound, fields);
        public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]>? fields = null) => new(code, message, ErrorType.Validation, fields);
        public static Error Conflict(string code, string message, IReadOnlyDictionary<string, string[]>? fields = null) => new(code, message, ErrorType.Conflict, fields);
        public static Error Failure(string code, string message, IReadOnlyDictionary<string, string[]>? fields = null) => new(code, message, ErrorType.Failure, fields);
    }

    public enum ErrorType
    {
        Failure,
        Validation,
        NotFound,
        Conflict
    }

}
