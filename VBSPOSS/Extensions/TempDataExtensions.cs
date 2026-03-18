using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace VBSPOSS.Extensions
{
    public static class TempDataExtensions
    {
        // Lưu object vào TempData
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonSerializer.Serialize(value);
        }

        // Lấy object từ TempData
        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object o;
            if (tempData.TryGetValue(key, out o))
            {
                return o == null ? null : JsonSerializer.Deserialize<T>((string)o);
            }
            return null;
        }
    }
}
