using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using TheMasonator.RESTApiResponseWrapper.Net.Wrappers;

namespace TheMasonator.RESTApiResponseWrapper.Net.Test;

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
        var response = await client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<APIResponse>(responseString);
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
        var response = await client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<APIResponse>(responseString);
        Assert.IsNotNull(responseObject);
        Assert.AreEqual(404, responseObject.StatusCode);
        Assert.AreEqual("Test Failed Successfully", responseObject.Message);
    }

    private static Mock<HttpMessageHandler> GetWorkingMockHttpMessageHandler()
    {
        var dataToReturn = new APIResponse { StatusCode = (int)HttpStatusCode.OK, Message = "Test Pass" };
        var json = JsonSerializer.Serialize(dataToReturn);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var workingMockHttpMessageHandler = new Mock<HttpMessageHandler>();
        workingMockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = httpContent
            });
        return workingMockHttpMessageHandler;
    }

    private static Mock<HttpMessageHandler> GetBrokenMockHttpMessageHandler()
    {
        var dataToReturn = new APIResponse { StatusCode = (int)HttpStatusCode.NotFound, Message = "Test Failed Successfully" };
        var json = JsonSerializer.Serialize(dataToReturn);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var workingMockHttpMessageHandler = new Mock<HttpMessageHandler>();
        workingMockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = httpContent
            });
        return workingMockHttpMessageHandler;
    }
}
