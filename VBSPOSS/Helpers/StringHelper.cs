namespace VBSPOSS.Helpers
{
    public class StringHelper
    {
        public static List<long> ConvertToLongList(string input, char separator)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<long>();

            return input
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    long value;
                    return long.TryParse(x.Trim(), out value) ? value : (long?)null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();
        }
    }
}
