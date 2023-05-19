using System.Text;
// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common;

public static class Hex
{
    public static byte[] Decode(byte[] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));

        var strBuffer = Encoding.ASCII.GetString(data);

        var result = new byte[data.Length / 2];
        var converted = 0;

        for (var i = 0; i < strBuffer.Length - 1; i += 2)
        {
            bool success = byte.TryParse(
                strBuffer.Substring(i, 2),
                System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture,
                out result[converted]
            );

            if (success)
                ++converted;
            else
                --i;
        }

        return result.Take(converted).ToArray();
    }

    public static byte[] Encode(byte[] data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var result = new StringBuilder(data.Length * 2);
        foreach (byte b in data)
        {
            result.Append(b.ToString("X2"));
        }
        return Encoding.ASCII.GetBytes(result.ToString());
    }
}
