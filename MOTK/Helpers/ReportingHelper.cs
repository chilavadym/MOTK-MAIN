using MOTK.Helpers.Interfaces;
using MOTK.Models;
using System.Text;

namespace MOTK.Helpers;

public class ReportingHelper : IReportingHelper
{
    private readonly OilTestResultForReporting? _oilTestResultForReporting;
    private StringBuilder? _builder;

    public ReportingHelper(OilTestResultForReporting? oilTestResultForReporting)
    {
        _oilTestResultForReporting = oilTestResultForReporting;

        PrepareBuilder();
    }
    private void PrepareBuilder()
    {
        _builder = new StringBuilder();
        _builder.Append(_oilTestResultForReporting?.SiteName);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.AssetName);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.AssetDescription);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.SamplePointName);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.TestReferenceName);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.Application);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.Hours);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.Manufacturer);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.OilName);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.Viscosity);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.SensorSerialNumber);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.TdnOrLossFactor);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.RemainingUsefulLife);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.VisualIndication);
        _builder.Append("\t");
        _builder.Append(_oilTestResultForReporting?.OilTestResult.DateCreated);
    }

    public string Headers => "Site Name\tAsset Name\tAsset Description\tSample Point Name\tTest Reference Name\tApplication\tHours\tManufacturer\tOil Name\tViscosity\tSensor Serial Number\tOil Condition\tRemaining Useful Life\tVisual Indication\tDate Created";

    public StringBuilder? Builder => _builder;
}