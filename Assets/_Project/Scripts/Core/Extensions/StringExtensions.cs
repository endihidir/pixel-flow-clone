using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly char[] SimplifyAllowedChars = { '_', '-' };

        public static string Simplify(this string x)
        {
            return new string(x.ToLower().Trim()
                .Select(c => char.IsLetterOrDigit(c) || SimplifyAllowedChars.Contains(c) ? c : '_')
                .ToArray());
        }

        public static string SplitCamelCase(this string input) => Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();

        public static int ExtractInt(this string s) => int.Parse(Regex.Match(s, @"\d+").Value);

        public static string ConvertToRanking(this int rank)
        {
            var lastTwoDigits = rank % 100;

            var suffix = lastTwoDigits is > 10 and < 21
                ? "th"
                : (rank % 10) switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                };

            return rank + suffix;
        }
        
        public static string HideBigNumber(this int num, CultureInfo cultureInfo = null) => num switch
        {
            >= 100000000 => (num / 1000000D).ToString("0.#M", cultureInfo),
            >= 1000000 => (num / 1000000D).ToString("0.##M", cultureInfo),
            >= 100000 => (num / 1000D).ToString("0.#k", cultureInfo),
            >= 10000 => (num / 1000D).ToString("0.##k", cultureInfo),
            >= 1000 => (num / 1000D).ToString("0.#k", cultureInfo),
            _ => num.ToString("0")
        };

        public static int ComputeFnv1AHash(this string val)
        {
            uint hash = 2166136261;

            foreach (var c in val)
            {
                hash = (hash ^ c) * 16777619;
            }

            return unchecked((int)hash);
        }
    }
}