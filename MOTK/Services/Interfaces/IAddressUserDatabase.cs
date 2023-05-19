using MOTK.Models;

namespace MOTK.Services.Interfaces;

public interface IAddressUserDatabase : IUserDatabase
{
    public bool WriteToDatabase(AddressSettings generalSettings);

    public AddressSettings? AddressSettings { get; }
}