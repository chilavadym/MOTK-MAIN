using System.Threading.Tasks;

namespace MOTK.Services.Interfaces;

internal interface ISplashWindowGenerator
{
    public Task RunSetupAsync();
}