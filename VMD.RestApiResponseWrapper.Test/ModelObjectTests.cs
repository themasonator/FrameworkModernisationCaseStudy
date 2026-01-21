using TheMasonator.RESTApiResponseWrapper.Net.Wrappers;

namespace TheMasonator.RestApiResponseWrapper.Net.Test
{
    [TestClass]
    public sealed class ModelObjectTests
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
                new("Name", "Required")
            };
            var ex = new ApiException("Custom error", 400, errors, "ERR001", "https://docs.link");

            Assert.AreEqual("Custom error", ex.Message);
            Assert.AreEqual(400, ex.StatusCode);
            CollectionAssert.AreEqual(errors, (List<ValidationError>)ex.Errors);
            Assert.AreEqual("ERR001", ex.ReferenceErrorCode);
            Assert.AreEqual("https://docs.link", ex.ReferenceDocumentLink);
        }

        [TestMethod]
        public void ApiError_Should_Set_Properties_Correctly()
        {
            var apiError = new ApiError("Custom error");
            Assert.IsTrue(apiError.IsError);
            Assert.AreEqual("Custom error", apiError.ExceptionMessage);
        }

        [TestMethod]
        public void ValidationError_Should_Set_Properties_Correctly()
        {
            var validationError = new ValidationError("field", "Custom message");
            Assert.AreEqual("field", validationError.Field);
            Assert.AreEqual("Custom message", validationError.Message);
        }
    }
}
