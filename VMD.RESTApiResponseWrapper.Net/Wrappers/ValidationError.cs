

using System.Text.Json.Serialization;

namespace TheMasonator.RESTApiResponseWrapper.Net.Wrappers
{
    public class ValidationError(string field, string message)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Field { get; } = field != string.Empty ? field : null;

        public string Message { get; } = message;
    }
}
