namespace VBSPOSS.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveVietnameseDiacritics(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string[] vietnameseChars = new string[]
            {
                "aàảãáạăằẳẵắặâầẩẫấậ",
                "AÀẢÃÁẠĂẰẲẴẮẶÂẦẨẪẤẬ",
                "dđ", "DĐ",
                "eèẻẽéẹêềểễếệ",
                "EÈẺẼÉẸÊỀỂỄẾỆ",
                "iìỉĩíị",
                "IÌỈĨÍỊ",
                "oòỏõóọôồổỗốộơờởỡớợ",
                "OÒỎÕÓỌÔỒỔỖỐỘƠỜỞỠỚỢ",
                "uùủũúụưừửữứự",
                "UÙỦŨÚỤƯỪỬỮỨỰ",
                "yỳỷỹýỵ",
                "YỲỶỸÝỴ"
            };

            for (int i = 0; i < vietnameseChars.Length; i++)
            {
                for (int j = 1; j < vietnameseChars[i].Length; j++)
                {
                    text = text.Replace(vietnameseChars[i][j], vietnameseChars[i][0]);
                }
            }

            return text;
        }
    }
}