using MOTK.Models;
using System.ComponentModel;

namespace MOTK.Notifications;

public class OilTestResultNeedDisplayingEventArgs : PropertyChangedEventArgs
{
    public OilTestResultNeedDisplayingEventArgs(string? propertyName, OilTestResult? oilTestResult) : base(propertyName)
    {
        OilTestResult = oilTestResult;
    }

    public OilTestResult? OilTestResult { get; set; }
}