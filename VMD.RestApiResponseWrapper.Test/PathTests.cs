using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
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
            {InnerHandler = mockHttpMessageHandler.Object};
        var client = new HttpClient(wrappingHandler);
        var response = await client.GetAsync("http://localhost/api/test");
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
        var response = await client.GetAsync("http://localhost/api/test");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<APIResponse>(responseString);
        Assert.IsNotNull(responseObject);
        Assert.AreEqual(404, responseObject.StatusCode);
        Assert.AreEqual("Unable to process the request.", responseObject.Message);
    }

    private Mock<HttpMessageHandler> GetWorkingMockHttpMessageHandler()
    {
        var dataToReturn = new { StatusCode = HttpStatusCode.OK, Message = "Test Pass" };
        var jsonDataToReturn = JsonSerializer.Serialize(dataToReturn);
        var workingMockHttpMessageHandler = new Mock<HttpMessageHandler>();
        workingMockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonDataToReturn, Encoding.UTF8, "application/json")
            });
        return workingMockHttpMessageHandler;
    }

    private Mock<HttpMessageHandler> GetBrokenMockHttpMessageHandler()
    {
        var dataToReturn = new { Status = HttpStatusCode.NotFound, Title = "Not Found", Details = "Test Failed Successfully" };
        var jsonDataToReturn = JsonSerializer.Serialize(dataToReturn);
        var workingMockHttpMessageHandler = new Mock<HttpMessageHandler>();
        workingMockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(jsonDataToReturn, Encoding.UTF8, "application/json")
            });
        return workingMockHttpMessageHandler;
    }
}
