using System.Web.Http.Controllers;
using VMD.RestApiResponseWrapper.Net;
using VMD.RESTApiResponseWrapper.Net.Filters;
using VMD.RESTApiResponseWrapper.Net.Wrappers;

namespace VMD.RestApiResponseWrapper.Net.Test
{
    [TestClass]
    public sealed class FullCoverageTestSuite
    {
        [TestMethod]
        public void ApiResponse_Should_Create_SuccessResponse()
        {
            var data = new List<string> { "value1", "value2" };
            var response = new APIResponse(200, "Request successful.", data);

            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual("Request successful.", response.Message);
            CollectionAssert.AreEqual(data, (List<string>)response.Result);
            Assert.IsNull(response.ResponseException);
            Assert.AreEqual("1.0.0.0", response.Version);
        }

        [TestMethod]
        public void ApiException_Should_Set_Properties_Correctly()
        {
            var errors = new List<ValidationError>
            {
                new ValidationError("Name", "Required")
            };
            var ex = new ApiException("Custom error", 400, errors, "ERR001", "https://docs.link");

            Assert.AreEqual("Custom error", ex.Message);
            Assert.AreEqual(400, ex.StatusCode);
            CollectionAssert.AreEqual(errors, (List<ValidationError>)ex.Errors);
            Assert.AreEqual("ERR001", ex.ReferenceErrorCode);
            Assert.AreEqual("https://docs.link", ex.ReferenceDocumentLink);
        }
    }
}
