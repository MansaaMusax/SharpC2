using System;

namespace Client
{
    public class Helpers
    {
        public static string CalculateTimeDiff(DateTime checkinTime)
        {
            var diff = (DateTime.UtcNow - checkinTime).TotalSeconds;

            var result = default(string);

            if (diff < 1)
            {
                result = $"{Math.Round(diff, 2)}s";
            }
            else if (diff > 1 && diff <= 59)
            {
                result = $"{Math.Round(diff, 0)}s";
            }
            else if (diff >= 60 && diff <= 3659)
            {
                var time = diff / 60;
                result = $"{Math.Round(time, 1)}m";
            }
            else if (diff >= 3600)
            {
                var time = diff / 3600;
                result = $"{Math.Round(time, 1)}h";
            }

            return result;
        }
    }
}