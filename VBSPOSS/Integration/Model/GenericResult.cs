using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using VBSPOSS.Integration.ViewModel;

namespace VBSPOSS.Integration.Model
{
    public class GenericResultReportGateway<T>
    {
        [JsonProperty("success")]
        public bool Success;

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        public GenericResultReportGateway()
        {
        }

        public GenericResultReportGateway(T result)
            : this(result, (int)HttpStatusCode.OK, true)
        {
        }

        public GenericResultReportGateway(T result, int code)
            : this(result, code, string.Empty)
        {
        }

        public GenericResultReportGateway(T result, int code, string message)
        {
            Result = result;
            Message = message;
            Code = code;
        }

        public GenericResultReportGateway(T result, int code, bool success)
        {
            Result = result;
            Code = code;
            Success = success;
        }

        public GenericResultReportGateway(int code, string message)
        {
            Message = message;
            Code = code;
        }

        public static GenericResultReportGateway<T> Fail(string message)
        {
            return new GenericResultReportGateway<T>((int)HttpStatusCode.BadRequest, message);
        }

        public static GenericResultReportGateway<T> Fail(string message, int code)
        {
            return new GenericResultReportGateway<T>(code, message);
        }

        public static GenericResultReportGateway<T> SetSuccess(T answer)
        {
            return new GenericResultReportGateway<T>(answer);
        }
    }


    public class GenericResultInternalGateway<T>
    {
        [JsonProperty("isSuccess")]
        public bool Success;

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        public GenericResultInternalGateway()
        {
        }

        public GenericResultInternalGateway(T result)
            : this(result, (int)HttpStatusCode.OK, true)
        {
        }

        public GenericResultInternalGateway(T result, int code)
            : this(result, code, string.Empty)
        {
        }

        public GenericResultInternalGateway(T result, int code, string message)
        {
            Result = result;
            Message = message;
            Code = code;
        }

        public GenericResultInternalGateway(T result, int code, bool success)
        {
            Result = result;
            Code = code;
            Success = success;
        }

        public GenericResultInternalGateway(int code, string message)
        {
            Message = message;
            Code = code;
        }

        public static GenericResultInternalGateway<T> Fail(string message)
        {
            return new GenericResultInternalGateway<T>((int)HttpStatusCode.BadRequest, message);
        }

        public static GenericResultInternalGateway<T> Fail(string message, int code)
        {
            return new GenericResultInternalGateway<T>(code, message);
        }

        public static GenericResultInternalGateway<T> SetSuccess(T answer)
        {
            return new GenericResultInternalGateway<T>(answer);
        }
    }

    public class GenericResultJava<T>
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus;

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        public GenericResultJava()
        {
        }

        public GenericResultJava(T result)
            : this(result, "Success", "00000")
        {
        }

        public GenericResultJava(T result, string txnStatus, string responseCode, string responseMsg)
        {
            Result = result;
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        public GenericResultJava(T result, string txnStatus, string responseCode)
        {
            Result = result;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        public GenericResultJava(string txnStatus, string responseCode, string responseMsg)
        {
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        public static GenericResultJava<T> Fail(string message)
        {
            return new GenericResultJava<T>("Failed", HttpStatusCode.BadRequest.ToString(), message);
        }

        public static GenericResultJava<T> SetSuccess(T answer)
        {
            return new GenericResultJava<T>(answer);
        }
    }



    public class GenericListResultJava<T>
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }

        [JsonProperty("result")]
        public List<T> Result { get; set; }

        public GenericListResultJava()
        {
            Result = new List<T>();
        }

        public GenericListResultJava(List<T> result, string txnStatus = "Success", string responseCode = "00000", string responseMsg = "Success")
        {
            Result = result ?? new List<T>();
            TxnStatus = txnStatus;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
        }

        public GenericListResultJava(string txnStatus, string responseCode, string responseMsg)
        {
            Result = new List<T>();
            TxnStatus = txnStatus;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
        }

        public static GenericListResultJava<T> Success(List<T> result, string responseMsg = "Success")
        {
            return new GenericListResultJava<T>(result, "Success", "00000", responseMsg);
        }

        public static GenericListResultJava<T> Fail(string message, string responseCode = "400")
        {
            return new GenericListResultJava<T>("Failed", responseCode, message);
        }
    }

    public class GenericListRecordJava<T>
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }

        [JsonProperty("record")]
        public List<T> Result { get; set; }

        public GenericListRecordJava()
        {
            Result = new List<T>();
        }

        public GenericListRecordJava(List<T> result, string txnStatus = "Success", string responseCode = "00000", string responseMsg = "Success")
        {
            Result = result ?? new List<T>();
            TxnStatus = txnStatus;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
        }

        public GenericListRecordJava(string txnStatus, string responseCode, string responseMsg)
        {
            Result = new List<T>();
            TxnStatus = txnStatus;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
        }

        public static GenericListRecordJava<T> Success(List<T> result, string responseMsg = "Success")
        {
            return new GenericListRecordJava<T>(result, "Success", "00000", responseMsg);
        }

        public static GenericListRecordJava<T> Fail(string message, string responseCode = "400")
        {
            return new GenericListRecordJava<T>("Failed", responseCode, message);
        }
    }



    public class GenericResultJavaRecord<T>
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus;

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }

        [JsonProperty("record")]
        public T Record { get; set; }

        public GenericResultJavaRecord()
        {
        }

        public GenericResultJavaRecord(T result)
            : this(result, "Success", "00000")
        {
        }

        public GenericResultJavaRecord(T record, string txnStatus, string responseCode, string responseMsg)
        {
            Record = record;
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        public GenericResultJavaRecord(T record, string txnStatus, string responseCode)
        {
            Record = record;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        public GenericResultJavaRecord(string txnStatus, string responseCode, string responseMsg)
        {
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        public static GenericResultJavaRecord<T> Fail(string message)
        {
            return new GenericResultJavaRecord<T>("Failed", HttpStatusCode.BadRequest.ToString(), message);
        }

        public static GenericResultJavaRecord<T> SetSuccess(T answer)
        {
            return new GenericResultJavaRecord<T>(answer);
        }
    }

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(List<T>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>(serializer);
            }
            return new List<T> { token.ToObject<T>(serializer) };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }


    public class GenericStatusListResult
    {
     
        public string TxnStatus;
     
        public string ResponseCode { get; set; }

        public string ResponseMsg { get; set; }


        public GenericStatusListResult(string txnStatus, string responseCode, string responseMsg)
        {
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
            TxnStatus = txnStatus;
        }

        [JsonProperty("respRecord")]
        public List<StatusRecord> StatusList { get; set; }


        public static GenericStatusListResult Fail(string message)
        {
            return new GenericStatusListResult("Failed", HttpStatusCode.BadRequest.ToString(), message);
        }
    }


    public class StatusRecord
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus { get; set; }

        [JsonProperty("reqRecordSl")]
        public string ReqRecordSl { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }
    }
}
