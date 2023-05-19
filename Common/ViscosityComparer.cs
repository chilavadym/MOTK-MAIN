// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common;
using System.Text.RegularExpressions;

public class ViscosityComparer : Comparer<Viscosity>
{
    public ViscosityComparer() { }

    public override int Compare(Viscosity? x, Viscosity? y)
    {
        if (x is null)
            return y is null ? 0 : -1;
        if (y is null)
            return 1;

        var xString = x.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
        var yString = y.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

        if (xString == yString)
            return 0;

        var digitRegex = new Regex(@"^0*(?<digits>\d+)(?<fraction>\.\d+)?");
        var charsRegex = new Regex(@"^([^\d]*)");

        while (xString.Length > 0 && yString.Length > 0)
        {
            int compare;
            { // Sort numerically
                var xRegResults = digitRegex.Match(xString);
                var yRegResults = digitRegex.Match(yString);

                var xDigits = xRegResults.Groups["digits"].Value;
                var yDigits = yRegResults.Groups["digits"].Value;

                if (xDigits.Length > 0 && yDigits.Length == 0)
                    return -1;
                if (xDigits.Length == 0 && yDigits.Length > 0)
                    return 1;
                if (xDigits.Length > 0 && yDigits.Length > 0)
                {
                    compare = xDigits.Length - yDigits.Length;
                        
                    if (compare != 0) return compare;

                    // We don't need to check against both lengths because we know they're the same after
                    // `if(xDigits.Length - yDigits.Length != 0)`
                    for (var index = 0; index < xDigits.Length; ++index)
                    {
                        compare = xDigits[index] - yDigits[index];
                            
                        if (compare != 0) return compare;
                    }

                    var xFraction = xRegResults.Groups["fraction"].Value;
                    var yFraction = yRegResults.Groups["fraction"].Value;

                    var minLen = Math.Min(xFraction.Length, yFraction.Length);
                        
                    for (var index = 0; index < minLen; ++index)
                    {
                        compare = xFraction[index] - yFraction[index];
                            
                        if (compare != 0) return compare;
                    }

                    // If we get here both values were equal, check which had fewest leading zeros
                    compare = xRegResults.Length - yRegResults.Length;
                        
                    if (compare != 0) return compare;

                    xString = xString.Remove(0, xRegResults.Length);
                    yString = yString.Remove(0, yRegResults.Length);
                }
            }

            // Sort alphabetically
            var xChars = charsRegex.Match(xString).Value;
            var yChars = charsRegex.Match(yString).Value;

            if (xChars.Length == 0 && yChars.Length > 0)
                return 1;
            if (yChars.Length == 0 && xChars.Length > 0)
                return -1;

            if (xChars.Length <= 0 || yChars.Length <= 0) continue;
                
            compare = string.Compare(xChars, yChars, StringComparison.InvariantCultureIgnoreCase);
                    
            if (compare != 0) return compare;

            xString = xString.Remove(0, xChars.Length);
            yString = yString.Remove(0, yChars.Length);
        }

        return x.Value.Length.CompareTo(y.Value.Length);
    }
}