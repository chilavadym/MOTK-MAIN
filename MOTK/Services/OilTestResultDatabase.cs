using MOTK.Models;
using MOTK.Services.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MOTK.Services;

public class OilTestResultDatabase : UserDatabase, IOilTestResultDatabase
{
    private List<OilTestResult>? _oilTestResults;

    public OilTestResultDatabase()
    {
        DatabaseFileName = "OilTestResults.json";

        OilTestResultsObservable = new ObservableCollection<OilTestResult>();
    }

    public void ReadFromDatabase()
    {
        var fileInfo = new FileInfo(DatabasePath);

        if (fileInfo.Exists)
        {
            var jsonData = File.ReadAllText(DatabasePath);
            _oilTestResults = JsonConvert.DeserializeObject<List<OilTestResult>>(jsonData);

            if (_oilTestResults != null)
            {
                foreach (var oilTestResult in _oilTestResults)
                {
                    OilTestResultsObservable?.Add(oilTestResult);
                }
            }
        }
    }

    public void WriteToDatabase(OilTestResult oilTestResult)
    {
        ReadFromDatabase();

        var fileInfo = new FileInfo(DatabasePath);

        if (fileInfo.Exists)
        {
            var jsonData = File.ReadAllText(DatabasePath);
            
            _oilTestResults = JsonConvert.DeserializeObject<List<OilTestResult>>(jsonData);
        }
        else
        {
            _oilTestResults = new List<OilTestResult>();
        }

        _oilTestResults?.Add(oilTestResult);

        ConvertToJson();

        OilTestResultsObservable?.Add(oilTestResult);
    }

    public void DeleteFromDatabase(OilTestResult oilTestResult)
    {
        ReadFromDatabase();

        if (_oilTestResults != null)
        {
            foreach (var result in _oilTestResults)
            {
                if (result.OilTest?.TestReferenceName == oilTestResult?.OilTest?.TestReferenceName)
                {
                    _oilTestResults.Remove(result);
                    OilTestResultsObservable?.Remove(result);
                    
                    break;
                }
            }

            ConvertToJson();
        }
    }

    protected override void ConvertToJson()
    {
        var serializer = new JsonSerializer();
        using var streamWriter = new StreamWriter(DatabasePath);
        using JsonWriter writer = new JsonTextWriter(streamWriter);
        serializer.Serialize(writer, _oilTestResults);
    }

    public ObservableCollection<OilTestResult>? OilTestResultsObservable { get; set; }
}