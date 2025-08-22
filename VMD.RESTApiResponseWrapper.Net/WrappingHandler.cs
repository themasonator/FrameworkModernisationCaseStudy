using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
                return await BuildApiResponseAsync(request, response);
            }
        }

        private async static Task<HttpResponseMessage> BuildApiResponseAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            object data = null;
            var code = (int)response.StatusCode;

            // Check if there is content to read from the response body.
            if (response.Content?.Headers.ContentLength > 0)
            {
                var contentString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ProblemDetails problem = JsonSerializer.Deserialize<ProblemDetails>(contentString);

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
                                var problemJObject = JsonSerializer.Deserialize<Dictionary<string, object>>(contentString);
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
                        data = await response.Content.ReadAsStringAsync();
                    }
                }
                else
                {
                    var contentStream = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<APIResponse>(contentStream);

                        if (apiResponse != null)
                        {
                            response.StatusCode = (HttpStatusCode)apiResponse.StatusCode;
                            data = apiResponse;
                        }
                    }
                    catch (JsonException)
                    {
                        var reader = new StreamReader(contentStream);
                        var rawContent = await reader.ReadToEndAsync();
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
                var jsonPayload = JsonSerializer.Serialize(data);
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
