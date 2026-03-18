namespace VBSPOSS.ExceptionHandling
{
    [Serializable]
    public class HttpException : Exception
    {
        //private HttpStatusCode statusCode;

        //public HttpException(HttpStatusCode statusCode)
        //{
        //    this.statusCode = statusCode;
        //}

        public HttpException(int statusCode)
        {
            switch (statusCode)
            {
                case 100:
                    throw new Exception($"{statusCode}: Continue");
                case 101:
                    throw new Exception($"{statusCode}: Switching Protocols");
                case 102:
                    throw new Exception($"{statusCode}: Processing");
                case 200:
                    throw new Exception($"{statusCode}: OK");
                case 201:
                    throw new Exception($"{statusCode}: Created");
                case 202:
                    throw new Exception($"{statusCode}: Accepted");
                case 203:
                    throw new Exception($"{statusCode}: Non-authoritative Information");
                case 204:
                    throw new Exception($"{statusCode}: No Content");
                case 205:
                    throw new Exception($"{statusCode}: Reset Content");
                case 206:
                    throw new Exception($"{statusCode}: Partial Content");
                case 207:
                    throw new Exception($"{statusCode}: Multi-Status");
                case 208:
                    throw new Exception($"{statusCode}: Already Reported");
                case 226:
                    throw new Exception($"{statusCode}: IM Used");
                case 300:
                    throw new Exception($"{statusCode}: Multiple Choices");
                case 301:
                    throw new Exception($"{statusCode}: Moved Permanently");
                case 302:
                    throw new Exception($"{statusCode}: Found");
                case 303:
                    throw new Exception($"{statusCode}: See Other");
                case 304:
                    throw new Exception($"{statusCode}: Not Modified");
                case 305:
                    throw new Exception($"{statusCode}: Use Proxy");
                case 307:
                    throw new Exception($"{statusCode}: Temporary Redirect");
                case 308:
                    throw new Exception($"{statusCode}: Permanent Redirect");
                case 400:
                    throw new Exception($"{statusCode}: Bad Request");
                case 401:
                    throw new Exception($"{statusCode}: Unauthorized");
                case 402:
                    throw new Exception($"{statusCode}: Payment Required");
                case 403:
                    throw new Exception($"{statusCode}: Forbidden");
                case 404:
                    throw new Exception($"{statusCode}: Not Found");
                case 405:
                    throw new Exception($"{statusCode}: Method Not Allowed");
                case 406:
                    throw new Exception($"{statusCode}: Not Acceptable");
                case 407:
                    throw new Exception($"{statusCode}: Proxy Authentication Required");
                case 408:
                    throw new Exception($"{statusCode}: Request Timeout");
                case 409:
                    throw new Exception($"{statusCode}: Conflict");
                case 410:
                    throw new Exception($"{statusCode}: Gone");
                case 411:
                    throw new Exception($"{statusCode}: Length Required");
                case 412:
                    throw new Exception($"{statusCode}: Precondition Failed");
                case 413:
                    throw new Exception($"{statusCode}: Payload Too Large");
                case 414:
                    throw new Exception($"{statusCode}: Request-URI Too Long");
                case 415:
                    throw new Exception($"{statusCode}: Unsupported Media Type");
                case 416:
                    throw new Exception($"{statusCode}: Requested Range Not Satisfiable");
                case 417:
                    throw new Exception($"{statusCode}: Expectation Failed");
                case 418:
                    throw new Exception($"{statusCode}: I'm a teapot");
                case 421:
                    throw new Exception($"{statusCode}: Misdirected Request");
                case 422:
                    throw new Exception($"{statusCode}: Unprocessable Entity");
                case 423:
                    throw new Exception($"{statusCode}: Locked");
                case 424:
                    throw new Exception($"{statusCode}: Failed Dependency");
                case 426:
                    throw new Exception($"{statusCode}: Upgrade Required");
                case 428:
                    throw new Exception($"{statusCode}: Precondition Required");
                case 429:
                    throw new Exception($"{statusCode}: Too Many Requests");
                case 431:
                    throw new Exception($"{statusCode}: Request Header Fields Too Large");
                case 444:
                    throw new Exception($"{statusCode}: Connection Closed Without Response");
                case 451:
                    throw new Exception($"{statusCode}: Unavailable For Legal Reasons");
                case 499:
                    throw new Exception($"{statusCode}: Client Closed Request");
                case 500:
                    throw new Exception($"{statusCode}: Internal Server Error");
                case 501:
                    throw new Exception($"{statusCode}: Not Implemented");
                case 502:
                    throw new Exception($"{statusCode}: Bad Gateway");
                case 503:
                    throw new Exception($"{statusCode}: Service Unavailable");
                case 504:
                    throw new Exception($"{statusCode}: Gateway Timeout");
                case 505:
                    throw new Exception($"{statusCode}: HTTP Version Not Supported");
                case 506:
                    throw new Exception($"{statusCode}: Variant Also Negotiates");
                case 507:
                    throw new Exception($"{statusCode}: Insufficient Storage");
                case 508:
                    throw new Exception($"{statusCode}: Loop Detected");
                case 510:
                    throw new Exception($"{statusCode}: Not Extended");
                case 511:
                    throw new Exception($"{statusCode}: Network Authentication Required");
                case 599:
                    throw new Exception($"{statusCode}: Network Connect Timeout Error");
                default:
                    throw new Exception($"{statusCode}: Error occured while Communication with Server with");
            }

        }
    }
}
