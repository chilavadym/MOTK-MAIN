using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MOTK.Services;

public abstract class UserDatabase
{
    protected string? DatabaseFileName;

    protected string DatabasePath
    {
        get
        {
            if (DatabaseFileName == null) return string.Empty;

            string path;

            if (DatabaseFileName == "GeneralSettings.json")
            {
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Tan Delta Systems",
                    "UserData",
                    DatabaseFileName);
            }
            else
            {
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Tan Delta Systems",
                    "UserData",
                    "Mobile Oil Tester (MOT) Kit",
                    DatabaseFileName);
            }

            var directory = Path.GetDirectoryName(path);

            if (Directory.Exists(directory)) return path;

            if (directory != null) Directory.CreateDirectory(directory);

            return path;
        }
    }

    protected virtual void ConvertToJson() { }

    protected static bool NoUnwantedCharacters(string text)
    {
        //text = Regex.Replace(text, @"\s", "");

        //text = text.Replace("-", "");

        text = text.Replace(" ", "");

        //var hasBadCharacters = text.Any(ch => !char.IsLetterOrDigit(ch));

        //if (hasBadCharacters) return false;
        if (string.IsNullOrEmpty(text)) return false;

        return true;
    }
}