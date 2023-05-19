using MOTK.Enums;
using MOTK.Statics;
using System;
using System.Globalization;

namespace MOTK.Models
{
    public class HistoricTrendTableModel
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string OilCondition { get; set; }

        public string RemainingLife { get; set; }
        public bool AlertResult { get; set; }
        public bool CautionResult { get; set; }
        public bool OkayResult { get; set; }
        public string? TdnBackgroundColor { get; set; }
        public HistoricTrendTableModel()
        {

        }
        public HistoricTrendTableModel(OilTestResult result)
        {
            DateTime dateTime = DateTime.ParseExact(result.OilTest.TestReferenceName, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
            DateTime date = dateTime.Date;
            TimeSpan time = dateTime.TimeOfDay;

            Date = date.ToString("dd/MM/yyyy");
            Time = time.ToString(@"hh\:mm");
            OilCondition = result?.SensorCondition != null ? result.SensorCondition.Value.OilCond.ToString() : "No data"; 
            RemainingLife = result?.RemainingUsefulLife != null ? result?.RemainingUsefulLife?.ToString() : "No data";
            GetGeneralCondition(result);

        }
        private void GetGeneralCondition(OilTestResult oilTestResult)
        {
            var result = GeneralConditionInfo.GetGeneralConditionInfo(oilTestResult);

            if (result == EOilCondition.Alert)
            {
                AlertResult = true;
                TdnBackgroundColor = Constants.TdnRed;
            }
            else if (result == EOilCondition.Caution)
            {
                CautionResult = true;
                TdnBackgroundColor = Constants.TdnAmber;
            }
            else if (result == EOilCondition.Okay)
            {
                OkayResult = true;
                TdnBackgroundColor = Constants.TdnGreen;
            }
        }
    }
}
