namespace VBSPOSS.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public static ServiceResult SuccessResult(
            string message = "Xử lý thành công",
            object data = null)
        {
            return new ServiceResult
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult ErrorResult(
            string message = "Có lỗi xảy ra",
            object data = null)
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}
