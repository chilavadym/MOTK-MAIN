using System;
using System.Collections.Generic;
using System.IO;

namespace MOTK.Statics;

public static class AlertPresets
{
    private static Dictionary<string, double[]>? _alertPresetsTable;
    private static List<string>? _availableApplications;

    private static void ReadFromAlertPresetsTable()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Tan Delta Systems",
            "Mobile Oil Testing Kit",
            "rsc",
            "AlertPresets.txt");

        _alertPresetsTable = new Dictionary<string, double[]>();
        _availableApplications = new List<string>();

        using (var reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                var lineComponents = line?.Split(',');

                if (lineComponents != null)
                {
                    _alertPresetsTable.Add(lineComponents[0],
                        new[]
                        {
                            Convert.ToDouble(lineComponents[1]), Convert.ToDouble(lineComponents[2]),
                            Convert.ToDouble(lineComponents[3])
                        });

                    _availableApplications.Add(lineComponents[0]);
                }
            }
        }
    }

    public static double[]? GetAlertLevels(string? application)
    {
        ReadFromAlertPresetsTable();

        try
        {
            if (_alertPresetsTable is not null)
            {
                if (application?.ToUpper() == null) return _alertPresetsTable["ENGINE"];

                return _alertPresetsTable[application.ToUpper()];
            }
        }
        catch (KeyNotFoundException)
        {
            if (_alertPresetsTable is not null)
            {
                // Use engine as the default
                return _alertPresetsTable["ENGINE"];
            }
        }

        return null;
    }

    public static List<string>? AvailableApplications => _availableApplications;
}