// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Units;

namespace MOTK.Services;
public class OilDatabase
{
    private static OilDatabase? _activeDatabase;
    private static OilDatabase? _defaultDatabase;
    internal Version? Version { get; }
    internal DateTime ReleaseDate { get; }

    private static OilInfo[]? _oils;
    internal IEnumerable<OilInfo>? Oils => _oils;

    public string? AppVersion { get; set; }

    internal OilDatabase? ActiveDatabase
    {
        get
        {
            if (_activeDatabase is not null) return _activeDatabase;

            Install();

            _activeDatabase = new OilDatabase(DatabasePath);

            return _activeDatabase;
        }
    }

    internal OilDatabase DefaultDatabase => _defaultDatabase ??= new OilDatabase(new FileInfo(DatabasePath));

    internal const string DatabaseFileExtension = ".oils";
    internal const string DatabaseFileName = "OilDb2" + DatabaseFileExtension;
    internal const string DatabaseDateFormat = "dd/MM/yyyy";
    internal const string DatabaseLineDelimiter = "\n";
    internal const string DatabaseFieldDelimiter = ",";
    //internal const string DatabaseFileFilters = "Oil Database File|*" + DatabaseFileExtension + "| Comma Separated Values|*.csv|All Files|*.*";

    internal static string DatabasePath
    {
        get
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tan Delta Systems",
                "UserData",
                DatabaseFileName);

            var directory = Path.GetDirectoryName(path);

            if (Directory.Exists(directory)) return path;

            if (directory != null) Directory.CreateDirectory(directory);

            return path;
        }
    }

    internal static string DatabaseInstallPath =>
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "rsc",
            DatabaseFileName);

    internal OilDatabase()
    {
    }

    internal OilDatabase(string databaseFile) : this(new FileInfo(databaseFile)) { }

    internal OilDatabase(FileInfo? databaseFile = null)
    {
        if (databaseFile is null)
        {
            throw new ArgumentNullException(nameof(databaseFile));
        }

        if (!databaseFile.Exists)
        {
            throw new FileNotFoundException("Cannot find the required database file.", databaseFile.FullName);
        }

        string[]? lines;

        using (var reader = databaseFile.OpenText())
        {
            lines = reader.ReadToEnd().Replace("\r", string.Empty).Split(new[] { DatabaseLineDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        if (lines.Length > 0)
        {// Parse header
            try
            {
                var headerCells = lines[0].Split(',');
                Version = new Version(headerCells[0]);
                ReleaseDate = DateTime.ParseExact(headerCells[1], DatabaseDateFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new FormatException("Database header is not valid.", ex);
            }

            var newOils = new List<OilInfo>();

            for (var i = 1; i < lines.Length; ++i)
            {
                if (TryParseEntry(Version, lines[i], out var parsedOils))
                {
                    newOils.AddRange(parsedOils);
                }
            }

            _oils = newOils.ToArray();
        }
        else
        {
            throw new ArgumentException("File is empty.", nameof(databaseFile));
        }
    }

    internal static void Update(string? databaseFile)
    {
        Update(databaseFile is null ? null : new FileInfo(databaseFile));
    }
    internal static void Update(FileInfo? databaseFile)
    {
        try
        {
            var db = new OilDatabase(databaseFile);

            if (_oils != null && !_oils.Any())
            {
                throw new ArgumentException("File contains no valid oil entries, keeping existing database.",
                    nameof(databaseFile));
            }

            if (db.Version != null && db.Version.Major == 5)
            {
                if (databaseFile != null) File.Copy(databaseFile.FullName, DatabasePath, true);
                _activeDatabase = db;
            }
            else
            {
                if (db.Version != null)
                    throw new NotSupportedException(
                        string.Format("Database version {0} is not supported by this application.",
                            db.Version.Major));
            }

        }
        catch (Exception ex)
        {
            Debug.DefaultLogger.Log(ex.Message);
            throw;
        }
    }
    internal static void Install()
    {
        if (!File.Exists(DatabasePath))
        {
            File.Copy(DatabaseInstallPath, DatabasePath);
        }
    }

    internal IEnumerable<OilInfo>? OrderBy(string property, bool descending = false)
    {
        var prop = typeof(OilInfo).GetProperty(property);

        IEnumerable<OilInfo>? ordered;

        if (_oils == null) return Oils;

        if (property != nameof(OilInfo.Viscosity))
        {
            ordered = _oils.OrderBy(oil => prop?.GetValue(oil));
        }
        else
        {
            ordered = _oils.OrderBy(oil => oil.Viscosity, new ViscosityComparer());
        }

        _oils = descending ? ordered.Reverse().ToArray() : ordered.ToArray();

        return Oils;
    }
    internal IEnumerable<object?> Select(string property)
    {
        var prop = typeof(OilInfo).GetProperty(property);

        var set = new HashSet<object?>();

        if (_oils == null) yield break;

        foreach (var oil in _oils)
        {
            if (prop == null) continue;

            var o = prop.GetValue(oil);

            if (set.Add(o)) yield return o;
        }
    }

    internal static OilInfo[] ParseEntry(Version? dbVersion, string databaseEntry)
    {
        databaseEntry = databaseEntry.Replace('\\' + DatabaseFieldDelimiter, $"{{{nameof(DatabaseFieldDelimiter)}}}");

        var cells = databaseEntry.Split(new string[] { DatabaseFieldDelimiter }, StringSplitOptions.None);

        for (var i = 0; i < cells.Length; ++i)
        {
            cells[i] = cells[i].Replace($"{{{nameof(DatabaseFieldDelimiter)}}}", DatabaseFieldDelimiter);
        }

        if (cells.Length < 17)
        {
            throw new FormatException("Entry does not contain enough fields.");
        }

        var manufacturer = cells[0];
        var oilName = cells[1];
        var viscosity = cells[2];

        if (!double.TryParse(cells[3], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double minTemp))
        {
            minTemp = double.NaN;
        }

        if (!double.TryParse(cells[4], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double maxTemp))
        {
            maxTemp = double.NaN;
        }

        var oilBase = cells[6];
        var application = cells[7];
        var oilMeta = uint.Parse(cells[8], System.Globalization.NumberStyles.HexNumber);

        var oilZero = BitConverter.ToSingle(BitConverter.GetBytes(int.Parse(cells[12], System.Globalization.NumberStyles.HexNumber)), 0);

        var polynomial = cells[5];
        var matrix = cells[13];
        var polyInd = cells[14];
        var matrixInd = cells[15];
        var dateProfiled = DateTime.Parse(cells[16]);

        //Inductor sensors can use both inductor and non-inductor oils, otherwise would only see ~20 profiles.
        //Non-inductor sensors cannot see inductor oils, due to low quality profiles.
        //The below assignments are ordered to choose the best profile for each sensor.
        //Correct profile > Upgraded from poly on same sensor > copied from diffierent sensor

        var oils = new Dictionary<OilInfo.EProfileType, OilInfo>();

        string snFormat;
        var profileLocations = new List<(OilInfo.EProfileType type, int cellIndex)>();

        if (dbVersion != null && dbVersion.Major != 4)
        {
            if (dbVersion.Major == 5)
            {
                snFormat = $"G2 :{cells[10]}{{0:X1}}0";
                profileLocations.Add((OilInfo.EProfileType.Unsupported, 5));
                profileLocations.Add((OilInfo.EProfileType.Gen2_0, 13));
                profileLocations.Add((OilInfo.EProfileType.Unsupported, 14));
                profileLocations.Add((OilInfo.EProfileType.Unsupported, 15));
            }
            else
            {
                throw new ArgumentException("Database version cannot be opened by this software");
            }
        }
        else
        {
            snFormat = $"OIL:{cells[10]}{{0:X1}}0";
            profileLocations.Add((OilInfo.EProfileType.Gen1_0, 5));
            profileLocations.Add((OilInfo.EProfileType.Gen1_3, 13));
            profileLocations.Add((OilInfo.EProfileType.Gen1_1, 14));
            profileLocations.Add((OilInfo.EProfileType.Gen1_2, 15));
        }

        var baseTypes = cells[11].Split(';').Select(s => (OilInfo.EProfileType)byte.Parse(s, System.Globalization.NumberStyles.HexNumber)).ToArray();

        if (baseTypes.Length < profileLocations.Count)
        {
            throw new FormatException("Database was generated for an older version of software");
        }

        for (var i = 0; i < profileLocations.Count; ++i)
        {
            var (type, cellIndex) = profileLocations.ElementAt(i);

            if (baseTypes[i] == 0 || string.IsNullOrWhiteSpace(cells[cellIndex])) continue;

            oils[type] = new OilInfo(
                OilInfo.SerialNumber.Parse(string.Format(snFormat, (byte)baseTypes[i])),
                manufacturer,
                oilName,
                new Viscosity(viscosity),
                new Temperature(minTemp, Temperature.BaseUnit),
                new Temperature(maxTemp, Temperature.BaseUnit),
                application,
                type,
                cells[cellIndex],
                dateProfiled
            );
        }

        return oils.Values.ToArray();
    }

    internal static bool TryParseEntry(Version? dbVersion, string databaseEntry, out OilInfo[] result)
    {
        try
        {
            result = ParseEntry(dbVersion, databaseEntry);
            return true;
        }
        catch
        {
            result = Array.Empty<OilInfo>();
            return false;
        }
    }
}