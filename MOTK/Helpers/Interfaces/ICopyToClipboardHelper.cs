using System.Threading.Tasks;

namespace MOTK.Helpers.Interfaces;

public interface ICopyToClipboardHelper
{
    public Task<bool> CopyToClipboard();
}