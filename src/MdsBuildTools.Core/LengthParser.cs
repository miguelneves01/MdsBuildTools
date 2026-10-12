using System.Globalization;
using System.Text.RegularExpressions;

namespace MdsBuildTools.Core
{
    /// <summary>Parses user-typed wall lengths ("3.47", "3,47 m", "347cm", "3470 mm") into metres.</summary>
    public static class LengthParser
    {
        public const float MinMetres = 0.05f;
        public const float MaxMetres = 50f;

        private static readonly Regex Pattern = new Regex(
            @"^\s*(?<num>\d+(?:[.,]\d+)?)\s*(?<unit>mm|cm|m)?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool TryParseMetres(string? input, out float metres)
        {
            metres = 0f;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var match = Pattern.Match(input);
            if (!match.Success)
                return false;

            var number = match.Groups["num"].Value.Replace(',', '.');
            if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return false;

            switch (match.Groups["unit"].Value.ToLowerInvariant())
            {
                case "mm": value /= 1000f; break;
                case "cm": value /= 100f; break;
            }

            if (value < MinMetres || value > MaxMetres)
                return false;

            metres = value;
            return true;
        }
    }
}
