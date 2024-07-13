using BackendServices.Models;
using System.Net;

namespace BackendServices.Exceptions
{
    public class NoValidationResultException : HttpResponseException
    {
        public NoValidationResultException(Response response) : base(response)
        {
        }

        public NoValidationResultException() : 
            this(new Response(HttpStatusCode.InternalServerError, error: "Validation error"))
        {

        }
    }
}
