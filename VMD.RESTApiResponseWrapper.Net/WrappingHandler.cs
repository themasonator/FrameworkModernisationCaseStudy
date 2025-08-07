using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VMD.RESTApiResponseWrapper.Net.Enums;
using VMD.RESTApiResponseWrapper.Net.Extensions;
using VMD.RESTApiResponseWrapper.Net.Wrappers;

namespace VMD.RESTApiResponseWrapper.Net
{
    public class WrappingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsSwagger(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }
            else
            {
                var response = await base.SendAsync(request, cancellationToken);
                return BuildApiResponse(request, response);
            }
        }

        private static  HttpResponseMessage BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            object data = null;
            var code = (int)response.StatusCode;

            // Check if there is content to read from the response body.
            if (response.Content?.Headers.ContentLength > 0)
            {
                var contentString = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    ProblemDetails problem = JsonConvert.DeserializeObject<ProblemDetails>(contentString);

                    if (problem != null)
                    {
                        ApiError apiError;
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                apiError = new ApiError("The specified URI does not exist. Please verify and try again.");
                                break;
                            case HttpStatusCode.NoContent:
                                apiError = new ApiError("The specified URI does not contain any content.");
                                break;
                            default:
                                string errorMessage = problem.Detail ?? "An unspecified error occurred.";

#if DEBUG                       
                                var problemJObject = JObject.Parse(contentString);
                                var exMessage = problemJObject["exceptionMessage"]?.ToString();
                                var stackTrace = problemJObject["stackTrace"]?.ToString();

                                errorMessage = $"{errorMessage}{exMessage?.ToString()}{stackTrace?.ToString()}";
#endif
                                apiError = new ApiError(errorMessage);
                                break;
                        }
                        data = new APIResponse(code, ResponseMessageEnum.Failure.GetDescription(), null, apiError);
                    }
                    else
                    {
                        data = response.Content.ReadAsStringAsync().Result;
                    }
                }
                else
                {
                    var contentStream = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        var apiResponse = JsonConvert.DeserializeObject<APIResponse>(contentStream);

                        if (apiResponse != null)
                        {
                            response.StatusCode = (HttpStatusCode)apiResponse.StatusCode;
                            data = apiResponse;
                        }
                    }
                    catch (JsonException)
                    {
                        var reader = new StreamReader(contentStream);
                        var rawContent = reader.ReadToEndAsync().Result;
                        data = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), rawContent);
                    }
                }
            }
            else
            {
                if (response.IsSuccessStatusCode)
                {
                    data = new APIResponse((int)response.StatusCode, ResponseMessageEnum.Success.GetDescription());
                }
            }

            // Create a new response containing the processed 'data' object.
            var newResponse = new HttpResponseMessage(response.StatusCode);
            if (data != null)
            {
                var jsonPayload = JsonConvert.SerializeObject(data);
                newResponse.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            foreach (var header in response.Headers)
            {
                newResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return newResponse;
        }

        private bool IsSwagger(HttpRequestMessage request)
        {
            return request.RequestUri.PathAndQuery.StartsWith("/swagger");
        }
    }
}
