using MOTK.Models;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MOTK.ViewModels
{
    public class TableControlViewModel : ViewModelBase
    {

        private ObservableCollection<HistoricTrendTableModel> _trends;

        public ObservableCollection<HistoricTrendTableModel> Trends
        {
            get { return _trends; }
            set => this.RaiseAndSetIfChanged(ref _trends, value);
        }


        public TableControlViewModel(OilTestResult testResult)
        {
            InitializeData(testResult);
        }
        private void InitializeData(OilTestResult testResult)
        {
            if (Trends == null)
            {
                Trends = new ObservableCollection<HistoricTrendTableModel>();
                MapData(testResult);
            }
        }


        private void MapData(OilTestResult testResult)
        {

            Trends.Add(new HistoricTrendTableModel(testResult));

        }
    }
}