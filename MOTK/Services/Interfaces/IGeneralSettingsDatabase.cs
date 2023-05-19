using MOTK.Models;

namespace MOTK.Services.Interfaces;

public interface IGeneralSettingsDatabase : IUserDatabase
{
    public bool WriteToDatabase(GeneralSettings generalSettings);
    public GeneralSettings? GeneralSettings { get; }
}