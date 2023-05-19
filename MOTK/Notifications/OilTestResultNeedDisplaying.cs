using MOTK.Models;
using MOTK.Statics;

namespace MOTK.Notifications
{
    public class OilTestResultNeedDisplaying
    {
        private OilTestResult? _oilTestResult;

        public delegate void OilTestResultNeedDisplayingEventHandler(object? sender, OilTestResultNeedDisplayingEventArgs e);

        protected void OnPropertyChanged(OilTestResultNeedDisplayingEventArgs e)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new OilTestResultNeedDisplayingEventArgs(propertyName, OilTestResult));
        }

        public OilTestResult? OilTestResult
        {
            get => _oilTestResult;
            set
            {
                if (value != _oilTestResult)
                {
                    _oilTestResult = value;
                    OnPropertyChanged(Constants.OilTestResultNeedDisplaying);
                }
            }
        }

        public event OilTestResultNeedDisplayingEventHandler? PropertyChanged;
    }
}
