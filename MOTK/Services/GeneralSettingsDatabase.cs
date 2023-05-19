using System;
using MOTK.Models;
using MOTK.Services.Interfaces;
using Newtonsoft.Json;
using System.IO;

namespace MOTK.Services;

public class GeneralSettingsDatabase : UserDatabase, IGeneralSettingsDatabase
{
    private GeneralSettings? _generalSettings;

    public GeneralSettingsDatabase()
    {
        DatabaseFileName = "GeneralSettings.json";
    }
        
    public void ReadFromDatabase()
    {
        var fileInfo = new FileInfo(DatabasePath);

        if (!fileInfo.Exists) return;
        
        var jsonData = File.ReadAllText(DatabasePath);
        _generalSettings = JsonConvert.DeserializeObject<GeneralSettings>(jsonData);
    }

    public bool WriteToDatabase(GeneralSettings? generalSettings)
    {
        _generalSettings = generalSettings;

        try
        {
            ConvertToJson();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    protected override void ConvertToJson()
    {
        var serializer = new JsonSerializer();
        using var streamWriter = new StreamWriter(DatabasePath);
        using JsonWriter writer = new JsonTextWriter(streamWriter);
        serializer.Serialize(writer, _generalSettings);
    }

    public GeneralSettings? GeneralSettings => _generalSettings;
}