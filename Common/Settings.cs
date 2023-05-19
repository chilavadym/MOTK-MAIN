using Common.Units;
using Newtonsoft.Json;
using System.Text;

namespace Common;

[JsonObject]
public class Settings
{
    public async Task LoadAsync(string fileName)
    {
        using var file = new StreamReader(fileName, Encoding.UTF8);
        JsonConvert.PopulateObject(await file.ReadToEndAsync(), this);
    }

    public async Task SaveAsync(string fileName)
    {
        await using var file = new StreamWriter(fileName, false, Encoding.UTF8);
        await file.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    [JsonProperty]
    public Version? DbPromptGen1 { get; set; } = new(0, 0);

    [JsonProperty]
    public Version? DbPromptGen2 { get; set; } = new(0, 0);

    [JsonProperty]
    public string? TemperatureUnit { get; set; } = Temperature.Celsius?.Word;

    [JsonProperty]
    public string? ConditionUnit { get; set; } = OilCondition.TanDeltaNumber?.Word;

    [JsonProperty]
    public bool AllowThirdPartyPorts { get; set; }
}