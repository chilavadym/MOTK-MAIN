// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Drawing;

namespace Common;

public static class Style
{
    public static Color ColorFromArgbString(string ArgbString)
    {
        return Color.FromArgb(int.Parse(ArgbString, System.Globalization.NumberStyles.HexNumber));
    }
    public static Color CalculateTextColor(Color BackColor)
    {
        var contrastWithBlack = CalculateContrast(BackColor, Color.Black);
        var contrastWithWhite = CalculateContrast(BackColor, Color.White);

        return contrastWithBlack >= contrastWithWhite ? Color.Black : Color.White;
    }
    public static double CalculateContrast(Color Color1, Color Color2)
    {
        var lum1 = CalculateLuminance(Color1);
        var lum2 = CalculateLuminance(Color2);

        var lightest = Math.Max(lum1, lum2);
        var darkest = Math.Min(lum1, lum2);

        //W3C recommended calculation for contrast
        return ((lightest + 0.05) / (darkest + 0.05));
    }
    public static double CalculateLuminance(Color TestColor)
    {
        //These values are the ITU-R BT.709 recommendations
        const double WeightRed = 0.2126;
        const double WeightGreen = 0.7152;
        const double WeightBlue = 0.0722;

        //Luminanace calculation formula as recommended by W3C, with alpha added in.
        var alpha = (double)TestColor.A / byte.MaxValue;
        var lumR = (double)TestColor.R / byte.MaxValue * alpha;
        var lumG = (double)TestColor.G / byte.MaxValue * alpha;
        var lumB = (double)TestColor.B / byte.MaxValue * alpha;

        lumR = lumR <= 0.03928 ? lumR / 12.92 : Math.Pow(((lumR + 0.055) / 1.055), 2.4);
        lumG = lumG <= 0.03928 ? lumG / 12.92 : Math.Pow(((lumG + 0.055) / 1.055), 2.4);
        lumB = lumB <= 0.03928 ? lumB / 12.92 : Math.Pow(((lumB + 0.055) / 1.055), 2.4);

        return (lumR * WeightRed) + (lumG * WeightGreen) + (lumB * WeightBlue);
    }

    public static float GetMaxFontSize(Graphics graphicsObject, int boundsHeight)
    {
        const float ptPerInch = 72.0f;
#pragma warning disable CA1416 // Validate platform compatibility
        return (boundsHeight / graphicsObject.DpiY * ptPerInch) - 1.0f;
#pragma warning restore CA1416 // Validate platform compatibility
    }
}