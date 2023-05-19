using Common;
using System;

namespace MOTK.Models;

public class OilTestResult
{
    public OilTestResult(OilTest? oilTest)
    {
        OilTest = oilTest;
    }

    public OilTest? OilTest { get; set; }
    public Reading? SensorCondition { get; set; }
    public string? RemainingUsefulLife { get; set; }
    public bool CanRepeat { get; set; }
    public DateTime DateCreated { get; set; }
}