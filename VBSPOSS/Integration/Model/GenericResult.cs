using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json.Serialization;
using VBSPOSS.Constants;
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

    public class SingleOrArrayConverter<T> : Newtonsoft.Json.JsonConverter
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

    /*
        {
            "sessionValReq": "true",
            "prevStatus": 0,
            "responseAttributes": {
                "USR_PASSWD": "s5j5SNHw"
            },
            "responseCode": 0,
            "responseMsg": "User Successfully Registered",
            "status": "true"
        }
     */
    public class UserIDCResponseResult
    {
        [JsonProperty("sessionValReq")]
        public string? SessionValReq { get; set; }

        [JsonProperty("prevStatus")]
        public int PrevStatus { get; set; }

        [JsonProperty("responseAttributes")]
        public ResponseAttributes? ResponseAttributes { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string? ResponseMsg { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        public UserIDCResponseResult(string sessionValReq, string status, string responseCode, string responseMsg, ResponseAttributes responseAttributes)
        {
            SessionValReq = sessionValReq;
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
            Status = status;
            ResponseAttributes = responseAttributes;
        }

        public static UserIDCResponseResult Fail(string message)
        {
            return new UserIDCResponseResult("", ResultValueAPI.ResultValue_Status_Failed, HttpStatusCode.BadRequest.ToString(), message, null);
        }

        public static UserIDCResponseResult SetSuccess(string sessionValReq, string status, string responseCode, string responseMsg, ResponseAttributes responseAttributes)
        {
            return new UserIDCResponseResult(sessionValReq, status, responseCode, responseMsg, responseAttributes);
        }
    }

    public class ResponseAttributes
    {
        [JsonProperty("USR_PASSWD")]//USR_PASSWD
        public string? UsrPasswd { get; set; }
    }


    /// <summary>
    /// Kết quả trả ra của API thay đổi quyền người dùng Intellect iDC
    /// </summary>
    public class TellerRoleAssignResponseResult
    {
        [JsonProperty("txnStatus")]
        public string TxnStatus { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string ResponseMsg { get; set; }

        public TellerRoleAssignResponseResult(string txnStatus, string responseCode, string responseMsg)
        {
            TxnStatus = txnStatus;
            ResponseMsg = responseMsg;
            ResponseCode = responseCode;
        }

        public static TellerRoleAssignResponseResult Fail(string message)
        {
            return new TellerRoleAssignResponseResult(ResultValueAPI.ResultValue_Status_Failed, "-1", message);
        }
    }

    /// <summary>
    /// Model thông tin trả ra khi gọi API Khóa/Mở khóa cho tài khoản người dùng Intellect iDC. Ex:
    ///      {
    ///          "emailAddress": "th@vbsp.vn",
    ///          "mobileNumber": "0983273000",
    ///          "enabled_by": "MOBILE",
    ///          "userId": "DUYEN002",
    ///          "enabled_at": "2026-03-27T10:40:31+00:00",
    ///          "responseCode": 0,
    ///          "responseMsg": "Enable User Done Successfully"
    ///      }
    ///      {
    ///         "sessionValReq": "true",
    ///         "prevStatus": 0,
    ///         "responseAttributes": {},
    ///         "responseCode": 735,
    ///         "responseMsg": "User is already enabled.",
    ///         "status": "true"
    ///     }
    /// </summary>
    public class ChangeUserStatusResponseResult
    {
        [JsonProperty("sessionValReq")]
        public string? SessionValReq { get; set; }

        [JsonProperty("prevStatus")]
        public int? PrevStatus { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string? ResponseMsg { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("mobileNumber")]
        public string? MobileNumber { get; set; }

        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("enabled_at")]
        public string? EnabledAt { get; set; }

        [JsonProperty("enabled_by")]
        public string? EnabledBy { get; set; }

        [JsonProperty("disabled_at")]
        public string? DisabledAt { get; set; }

        [JsonProperty("disabled_by")]
        public string? DisabledBy { get; set; }

        [JsonProperty("statusCode")]
        public string? StatusCode { get; set; }

        public ChangeUserStatusResponseResult(string sessionValReq, int prevStatus, string responseCode, string responseMsg, string status, string emailAddress, string mobileNumber, 
                                string userId, string enabledAt, string enabledBy, string disabledAt, string disabledBy, string statusCode)
        {
            SessionValReq = sessionValReq;
            PrevStatus = prevStatus;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
            Status = status;
            EmailAddress = emailAddress;
            MobileNumber = mobileNumber;
            UserId = userId;
            EnabledAt = enabledAt;
            EnabledBy = enabledBy;
            DisabledAt = disabledAt;
            DisabledBy = disabledBy;
            StatusCode = statusCode;
        }

        public static ChangeUserStatusResponseResult Fail(string sessionValReq, int prevStatus, string responseCode, string message, string status, string emailAddress,
                                string mobileNumber, string userId, string enabledAt, string enabledBy, string disabledAt, string disabledBy, string statusCode)
        {
            return new ChangeUserStatusResponseResult("false", -1, "-1", message, "false", "", "", "", "", "", "", "", ResultValueAPI.ResultValue_Status_Failed);
        }
    }

    /// <summary>
    /// Model thông tin trả ra khi gọi API đặt lại mật khẩu cho tài khoản người dùng Intellect iDC. Ex:
    ///     {
    ///         "emailAddress": "chudv.cctt@gmail.com",
    ///         "mobileNumber": "0908688212",
    ///         "reset_by": "SYSTEMADMIN2",
    ///         "userId": "CHUV12",
    ///         "reset_at": "2026-01-14T21:55:10+00:00",
    ///         "mail_flag": "0",
    ///         "responseCode": "0",
    ///         "responseMsg": "Password Reset Successful"
    ///     }
    /// Nếu không thành công
    ///     {
    ///         "sessionValReq": "true",
    ///         "prevStatus": "0",
    ///         "responseAttributes": { },
    ///         "responseCode": "5317",
    ///         "responseMsg": "ARX-005317: User does not exist.",
    ///         "status": "true"
    ///     }
    /// </summary>
    public class ResetUserPasswordResponseResult
    {
        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("mobileNumber")]
        public string? MobileNumber { get; set; }

        [JsonProperty("reset_by")]
        public string? ResetBy { get; set; }

        [JsonProperty("reset_at")]
        public string? ResetAt { get; set; }

        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("mail_flag")]
        public string? MailFlag { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string? ResponseMsg { get; set; }

        [JsonProperty("sessionValReq")]
        public string? SessionValReq { get; set; }

        [JsonProperty("prevStatus")]
        public int? PrevStatus { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("responseAttributes")]
        public ResponseAttributes? ResponseAttributes { get; set; }

        [JsonProperty("statusCode")]
        public string? StatusCode { get; set; }

        public ResetUserPasswordResponseResult(string sessionValReq, int prevStatus, string responseCode, string responseMsg, string status, string emailAddress, string mobileNumber,
                                string userId, string resetAt, string resetBy, string mailFlag, ResponseAttributes responseAttributes, string statusCode)
        {
            SessionValReq = sessionValReq;
            PrevStatus = prevStatus;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
            Status = status;
            EmailAddress = emailAddress;
            MobileNumber = mobileNumber;
            UserId = userId;
            ResetAt = resetAt;
            ResetBy = resetBy;
            StatusCode = statusCode;
            MailFlag = mailFlag;
            ResponseAttributes = responseAttributes;
        }

        public static ResetUserPasswordResponseResult Fail(string sessionValReq, int prevStatus, string responseCode, string message, string status, string emailAddress,
                                string mobileNumber, string userId, string resetAt, string resetBy, string mailFlag, ResponseAttributes responseAttributes, string statusCode)
        {
            return new ResetUserPasswordResponseResult("false", -1, "-1", message, "false", "", "", "", "", "", "", null, ResultValueAPI.ResultValue_Status_Failed);
        }
    }




    /// <summary>
    /// Model thông tin trả ra khi gọi API thay đổi thông tin tài khoản người dùng Intellect iDC. Ex:
    ///     {
    ///         "sessionValReq": "true",
    ///         "prevStatus": 0,
    ///         "responseAttributes": {},
    ///         "mobileNumber": "0908688212",
    ///         "posCode": "2505",
    ///         "userRole": "POPGD",
    ///         "responseCode": 0,
    ///         "responseMsg": "Modify User Done Successfully",
    ///         "status": "true"
    ///     }
    /// --Hoặc nếu sửa tiếp POS thì trả ra như sau:
    ///     {
    ///         "mobileNumber": "0908688212",
    ///         "posCode": "2502",
    ///         "userRole": "POPGD",
    ///         "status": "true",
    ///         "responseMsg": " BranchCode Modify Done Successfully",
    ///         "responseCode": 0
    ///     }
    /// Không thành công:
    ///     {
    ///         "sessionValReq": "true",
    ///         "prevStatus": "0",
    ///         "responseAttributes": { },
    ///         "responseCode": "5317",
    ///         "responseMsg": "ARX-005317: User does not exist.",
    ///         "status": "true"
    ///     }
    /// </summary>
    public class ModifyUserIDCResponseResult
    {
        [JsonProperty("sessionValReq")]
        public string? SessionValReq { get; set; }

        [JsonProperty("prevStatus")]
        public int? PrevStatus { get; set; }

        [JsonProperty("responseAttributes")]
        public ResponseAttributes? ResponseAttributes { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("mobileNumber")]
        public string? MobileNumber { get; set; }

        [JsonProperty("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonProperty("posCode")]
        public string? PosCode { get; set; }

        [JsonProperty("userRole")]
        public string? UserRole { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("responseMsg")]
        public string? ResponseMsg { get; set; }

        [JsonProperty("statusCode")]
        public string? StatusCode { get; set; }

        public ModifyUserIDCResponseResult(string sessionValReq, int prevStatus, ResponseAttributes responseAttributes, string status, string mobileNumber, string emailAddress,
                                               string posCode, string userRole,string responseCode, string responseMsg,  string statusCode)
        {
            SessionValReq = sessionValReq;
            PrevStatus = prevStatus;
            ResponseAttributes = responseAttributes;
            Status = status;
            MobileNumber = mobileNumber;
            EmailAddress = emailAddress;
            PosCode = posCode;
            UserRole = userRole;
            ResponseCode = responseCode;
            ResponseMsg = responseMsg;
            StatusCode = statusCode;
        }

        public static ModifyUserIDCResponseResult Fail(string sessionValReq, int prevStatus, ResponseAttributes responseAttributes, string status, string mobileNumber, string emailAddress,
                                               string posCode, string userRole, string responseCode, string message, string statusCode)
        {
            return new ModifyUserIDCResponseResult("false", -1, null, "false", "", "", "", "", "-1", message, ResultValueAPI.ResultValue_Status_Failed);
        }
    }



    public class UpdateNotiResult
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

}
