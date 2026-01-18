using Moq;
using Moq.Protected;
using System.Net;
using System.Web.Http;
using VMD.RESTApiResponseWrapper.Net.Wrappers;

namespace VMD.RESTApiResponseWrapper.Net.Test;

[TestClass]
public class PathTests
{
    [TestMethod]
    public async Task HappyPathTest()
    {
        var mockHttpMessageHandler = GetWorkingMockHttpMessageHandler();
        var wrappingHandler = new WrappingHandler
        { InnerHandler = mockHttpMessageHandler.Object };
        var client = new HttpClient(wrappingHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
        request.SetConfiguration(new HttpConfiguration());
        var response = await client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = System.Text.Json.JsonSerializer.Deserialize<APIResponse>(responseString);
        Assert.IsNotNull(responseObject);
        Assert.AreEqual(200, responseObject.StatusCode);
        Assert.AreEqual("Test Pass", responseObject.Message);
    }

    [TestMethod]
    public async Task SadPathTest()
    {
        var mockHttpMessageHandler = GetBrokenMockHttpMessageHandler();
        var wrappingHandler = new WrappingHandler
        { InnerHandler = mockHttpMessageHandler.Object };
        var client = new HttpClient(wrappingHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
        request.SetConfiguration(new HttpConfiguration());
        var response = await client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = System.Text.Json.JsonSerializer.Deserialize<APIResponse>(responseString);
        Assert.IsNotNull(responseObject);
        Assert.AreEqual(404, responseObject.StatusCode);
        Assert.AreEqual("Test Failed Successfully", responseObject.Message);
    }

    private Mock<HttpMessageHandler> GetWorkingMockHttpMessageHandler()
    {
        var dataToReturn = new APIResponse { StatusCode = (int)HttpStatusCode.OK, Message = "Test Pass" };
        var workingMockHttpMessageHandler = new Mock<HttpMessageHandler>();
        workingMockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent<APIResponse>(dataToReturn, new System.Net.Http.Formatting.JsonMediaTypeFormatter())
            });
        return workingMockHttpMessageHandler;
    }

    private Mock<HttpMessageHandler> GetBrokenMockHttpMessageHandler()
    {
        var dataToReturn = new APIResponse { StatusCode = (int)HttpStatusCode.NotFound, Message = "Test Failed Successfully" };
        var workingMockHttpMessageHandler = new Mock<HttpMessageHandler>();
        workingMockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new ObjectContent<APIResponse>(
                    dataToReturn,
                    new System.Net.Http.Formatting.JsonMediaTypeFormatter())
            });
        return workingMockHttpMessageHandler;
    }
}
