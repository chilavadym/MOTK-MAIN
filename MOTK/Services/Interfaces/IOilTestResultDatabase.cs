using MOTK.Models;
using System.Collections.ObjectModel;

namespace MOTK.Services.Interfaces;

public interface IOilTestResultDatabase : IUserDatabase
{
    public void WriteToDatabase(OilTestResult oilTestResult);

    public void DeleteFromDatabase(OilTestResult oilTestResult);

    public ObservableCollection<OilTestResult>? OilTestResultsObservable { get; }
}