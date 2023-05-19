using System.Text;

namespace MOTK.Helpers.Interfaces;

public interface IReportingHelper
{
    public string? Headers { get; }
    public StringBuilder? Builder { get; }
}