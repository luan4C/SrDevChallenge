using System;
using System.Globalization;

namespace SIEG.SrDevChallenge.CrossCutting.Helpers;

public static class DecimalHelpers
{
    public static bool TryParseDecimalCultureAgnostic(string s, out decimal value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(s)) return false;

        s = s.Trim();

        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            return true;

        return decimal.TryParse(s, NumberStyles.Number, new CultureInfo("pt-BR"), out value);
    }
}
