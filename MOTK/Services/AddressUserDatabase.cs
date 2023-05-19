using MOTK.Models;
using MOTK.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MOTK.Services;

public class AddressUserDatabase : UserDatabase, IAddressUserDatabase
{
    private AddressSettings? _addressSettings;

    public AddressUserDatabase()
    {
        DatabaseFileName = "AddressSettings.json";
    }

    public void ReadFromDatabase()
    {
        var fileInfo = new FileInfo(DatabasePath);

        if (!fileInfo.Exists) return;

        var jsonData = File.ReadAllText(DatabasePath);
        _addressSettings = JsonConvert.DeserializeObject<AddressSettings>(jsonData);
    }

    public bool WriteToDatabase(AddressSettings addressSettings)
    {
        _addressSettings = addressSettings;

        if (addressSettings.SiteName == null || addressSettings.SiteAddress == null ||
            !NoUnwantedCharacters(addressSettings.SiteName) ||
            !NoUnwantedCharacters(addressSettings.SiteAddress)) return false;
        
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
        serializer.Serialize(writer, _addressSettings);
    }
    

    public AddressSettings? AddressSettings => _addressSettings;
}