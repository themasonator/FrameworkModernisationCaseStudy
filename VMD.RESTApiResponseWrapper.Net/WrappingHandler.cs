using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
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
            dynamic content = null;
            object data = null;
            string errorMessage = null;
            ApiError apiError = null;

            var code = (int)response.StatusCode;

            string jsonString = response.Content != null ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult() : null;
            // Check if there is content to read from the response body.
            if (!string.IsNullOrEmpty(jsonString))
            {
                try { content = JsonConvert.DeserializeObject<dynamic>(jsonString); }
                catch
                { }
            }

            if (content != null && !response.IsSuccessStatusCode)
            {
                if (content.StatusCode != null)
                {
                    data = content;
                }
                else 
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    apiError = new ApiError("The specified URI does not exist. Please verify and try again.");
                    else if (response.StatusCode == HttpStatusCode.NoContent)
                        apiError = new ApiError("The specified URI does not contain any content.");
                    else
                    {
                        errorMessage = (string)content.Message;

    #if DEBUG
                        errorMessage = string.Concat(errorMessage, (string)content.ExceptionMessage, (string)content.StackTrace);
    #endif

                        apiError = new ApiError(errorMessage);
                    }
                    data = new APIResponse((int)code, ResponseMessageEnum.Failure.GetDescription(), null, apiError);
                }
            }
            else
            {
                if (content != null)
                {
                    if (content.StatusCode != null)
                    {
                        response.StatusCode = Enum.Parse(typeof(HttpStatusCode), content.StatusCode.ToString());
                        data = content;
                    }
                    else if (content.swagger != null)
                        data = content;
                    else
                        data = new APIResponse(code, ResponseMessageEnum.Success.GetDescription(), content);
                }
                else
                {
                    if (response.IsSuccessStatusCode)
                        data = new APIResponse((int)response.StatusCode, ResponseMessageEnum.Success.GetDescription());
                }
            }
            var newResponse = new HttpResponseMessage(response.StatusCode)
            {
                RequestMessage = request,
                Content = data != null
                    ? new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                    : null
            };

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
