using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using TheMasonator.RESTApiResponseWrapper.Net.Enums;
using TheMasonator.RESTApiResponseWrapper.Net.Extensions;
using TheMasonator.RESTApiResponseWrapper.Net.Wrappers;

namespace TheMasonator.RESTApiResponseWrapper.Net.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            ApiError apiError = null;
            APIResponse apiResponse = null;
            int code;

            if (context.Exception is ApiException)
            {
                // handle explicit 'known' API errors
                var ex = context.Exception as ApiException;
                apiError = new ApiError(ex.Message)
                {
                    ValidationErrors = ex.Errors,
                    ReferenceErrorCode = ex.ReferenceErrorCode,
                    ReferenceDocumentLink = ex.ReferenceDocumentLink
                };
                code = ex.StatusCode;
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");
                code = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                // Unhandled errors
#if !DEBUG
                var msg = "An unhandled error occurred.";
                string stack = null;
#else
                var msg = context.Exception.GetBaseException().Message;
                string stack = context.Exception.StackTrace;
#endif

                apiError = new ApiError(msg)
                {
                    Details = stack
                };
                code = (int)HttpStatusCode.InternalServerError;


                // handle logging here
            }

            apiResponse = new APIResponse(code, ResponseMessageEnum.Exception.GetDescription(), null, apiError);

            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = code
            };

            // Mark the exception as handled
            context.ExceptionHandled = true;
        }
    }
}