using BackendServices.Exceptions;
using BackendServices.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace BackendServer
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            Response response = context.Error switch
            {
                HttpResponseException responseError => responseError.Response,

                Exception error => new Response(HttpStatusCode.InternalServerError, Problem(title: error.Message)),
                _ => new Response(HttpStatusCode.InternalServerError, Problem())
            };

            return StatusCode((int)response.StatusCode, response);
        }
    }
}