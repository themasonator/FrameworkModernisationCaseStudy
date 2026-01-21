using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheMasonator.RESTApiResponseWrapper.Net.Enums;
using TheMasonator.RESTApiResponseWrapper.Net.Extensions;
using TheMasonator.RESTApiResponseWrapper.Net.Wrappers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TheMasonator.RESTApiResponseWrapper.Net
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

        private static async Task<HttpResponseMessage> BuildApiResponseAsync(HttpRequestMessage request, HttpResponseMessage response)
        {
            object data;
            dynamic content = await ReadAndDeserializeAsync(response);

            if (content?.StatusCode != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    response.StatusCode = Enum.Parse(typeof(HttpStatusCode), content.StatusCode.ToString());
                }
                data = content;
            }
            else if (content?.swagger != null)
            {
                data = content;
            }
            else if (!response.IsSuccessStatusCode)
            {
                data = WrapFailedResponse(response, content);
            }
            else
            {
                data = new APIResponse((int)response.StatusCode, ResponseMessageEnum.Success.GetDescription());
            }
            return CreateNewResponse(response, data, request);
        }

        private static HttpResponseMessage CreateNewResponse(
            HttpResponseMessage response,
            object data,
            HttpRequestMessage request)
        {
            var newResponse = new HttpResponseMessage(response.StatusCode)
            {
                RequestMessage = request,
                Content = data != null
                    ? new StringContent(
                        JsonConvert.SerializeObject(data),
                        Encoding.UTF8,
                        "application/json")
                    : null
            };
            newResponse = AddAllHeadersToNewResponse(response, newResponse);
            return newResponse;
        }

        private static HttpResponseMessage AddAllHeadersToNewResponse(
            HttpResponseMessage response, 
            HttpResponseMessage newResponse)
        {
            foreach (var header in response.Headers)
            {
                newResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return newResponse;
        }

        private static async Task<JObject> ReadAndDeserializeAsync(HttpResponseMessage response)
        {
            if (response.Content is null)
                return null;
            string jsonString = await response.Content?.ReadAsStringAsync();
            return TryDeserializeJson(jsonString);
        }

        private static JObject TryDeserializeJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<dynamic>(jsonString);
            }
            catch
            {
                return null;
            }
        }

        private static object WrapFailedResponse(HttpResponseMessage response, dynamic content)
        {
            ApiError apiError;
            if (response.StatusCode == HttpStatusCode.NotFound)
                apiError = new ApiError("The specified URI does not exist. Please verify and try again.");
            else if (response.StatusCode == HttpStatusCode.NoContent)
                apiError = new ApiError("The specified URI does not contain any content.");
            else
            {
                string errorMessage = (string)content.Message;
#if DEBUG
                errorMessage = string.Concat(errorMessage, (string)content.ExceptionMessage, (string)content.StackTrace);
#endif
                apiError = new ApiError(errorMessage);
            }
            return new APIResponse((int)response?.StatusCode, ResponseMessageEnum.Failure.GetDescription(), null, apiError);
        }

        private static bool IsSwagger(HttpRequestMessage request) 
            => request.RequestUri.PathAndQuery.StartsWith("/swagger");
    }
}
