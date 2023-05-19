namespace MOTK.Helpers;

public class VersionArgs
{
    public VersionArgs(string? propertyName, string? value, bool isUpToDate)
    {
        PropertyName = propertyName;
        Value = value;
        IsUpToDate = isUpToDate;
    }

    public string? PropertyName { get; }
    public string? Value { get; }
    public bool IsUpToDate { get; }
}