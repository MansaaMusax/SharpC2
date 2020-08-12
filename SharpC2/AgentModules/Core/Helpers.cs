namespace Agent
{
    class Helpers
    {
        public static string ConvertFileLength(long size)
        {
            var result = size.ToString();

            if (size < 1024) { result = $"{size}b"; }
            else if (size > 1024 && size <= 1048576) { result = $"{size / 1024}kb"; }
            else if (size > 1048576 && size <= 1073741824) { result = $"{size / 1048576}mb"; }
            else if (size > 1073741824 && size <= 1099511627776) { result = $"{size / 1073741824}gb"; }
            else if (size > 1099511627776) { result = $"{size / 1099511627776}tb"; }

            return result;
        }
    }
}