using System;
using System.Net;

namespace BackendServices.Models
{
#nullable enable

    public record Response
    {
        public Response(HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? data = null, string? message = null, string? error = null)
        {
            Data = data;
            StatusCode = statusCode;
            Error = error;
            Message = message;
        }

        public Response() : this(HttpStatusCode.BadRequest) { }

        public virtual object? Data { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public string? Error { get; init; }
        public string? Message { get; init; }
        public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    }

    public record Response<TData> : Response
    {
        public Response(TData? data, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = null, string? error = null ) 
            : base(statusCode, data, message, error) { }

        public Response() : base(HttpStatusCode.OK) { }
        public new TData? Data { get => (TData?) base.Data; init => base.Data = value; }

    }

#nullable disable
}
