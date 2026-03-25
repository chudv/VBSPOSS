using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VBSPOSS.Integration.Model
{
    public class GenericResultCode<T>
    {
        [JsonPropertyName("code")]
        public string ResponseCode { get; set; }

        [JsonPropertyName("message")]
        public string ResponseMsg { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        public GenericResultCode()
        {
        }

        public GenericResultCode(T result)
            : this(result, "00", "", true)
        {
        }

        public GenericResultCode(T result, string responseCode, string responseMsg, bool success)
        {
            Result = result;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
            Success = success;
        }

        public GenericResultCode(string responseCode, string responseMsg)
        {
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
            Success = false;
        }

        public static GenericResultCode<T> Fail(string message)
        {
            return new GenericResultCode<T>("99", message);
        }

        public static GenericResultJava<T> SetSuccess(T answer)
        {
            return new GenericResultJava<T>(answer);
        }
    }
}
