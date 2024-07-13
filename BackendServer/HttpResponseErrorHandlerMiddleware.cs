using BackendServices.Exceptions;
using BackendServices.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BackendServer
{

    public class HttpResponseErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpResponseErrorHandlerMiddleware> logger;

        public HttpResponseErrorHandlerMiddleware(RequestDelegate next, ILogger<HttpResponseErrorHandlerMiddleware> logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }catch(Exception error)
            {
                logger.LogError(error.Message);
                logger.LogError(error.StackTrace);

                Response response;
                if (error is HttpResponseException responseError)
                    response = responseError.Response;
                else
                    response = new Response(HttpStatusCode.InternalServerError, error: "Something went wrong!");

                context.Response.StatusCode = (int)response.StatusCode;
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}