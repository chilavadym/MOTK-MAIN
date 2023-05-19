using MOTK.Helpers.Interfaces;
using MOTK.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace MOTK.Helpers;

public class ExportAndCopyDataHelper : IExportDataHelper, ICopyToClipboardHelper
{
    private readonly OilTestResultForReporting? _oilTestResultForReporting;
    private readonly string? _location;

    public ExportAndCopyDataHelper(OilTestResultForReporting? oilTestResultForReporting)
    {
        _oilTestResultForReporting = oilTestResultForReporting;
    }

    public ExportAndCopyDataHelper(OilTestResultForReporting? oilTestResultForReporting, string? location)
    {
        _oilTestResultForReporting = oilTestResultForReporting;
        _location = location;
    }

    public bool WriteToFile()
    {
        var fileName = GetFormattedFileName(_oilTestResultForReporting?.TestReferenceName);
        var fileLocation = _location;

        IReportingHelper helper = new ReportingHelper(_oilTestResultForReporting);

        using var writer = new StreamWriter(fileLocation);
        writer.WriteLine(helper.Headers);
        writer.WriteLine(helper.Builder?.ToString());

        return true;
    }

    public async Task<bool> CopyToClipboard()
    {
        if (Application.Current?.Clipboard is not null)
        {
            IReportingHelper helper = new ReportingHelper(_oilTestResultForReporting);

            var builder = new StringBuilder();
            builder.AppendLine(helper.Headers);
            builder.Append(helper.Builder?.ToString());

            await Application.Current.Clipboard.SetTextAsync(builder.ToString());
        }

        return true;
    }

    private string GetFormattedFileName(string? name)
    {
        try
        {
            var then = Convert.ToDateTime(name);
            var time = then.Year + "_" + then.Month + "_" + then.Day + "_" + then.Hour + "_" + then.Minute + "_" + then.Second;
            return $"{time}.tsv";
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}