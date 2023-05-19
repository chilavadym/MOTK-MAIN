using MOTK.Enums;
using MOTK.Events;
using MOTK.Models;
using MOTK.Services;
using MOTK.Services.Interfaces;
using OxyPlot;
using OxyPlot.Avalonia;
using OxyPlot.Axes;
using Prism.Events;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MOTK.ViewModels
{

    public class GraphControlViewModel : ViewModelBase, IDisposable
    {

        enum Unit
        {
            TDN,
            LF
        }
        public event EventHandler<EventArgs> ComboBoxValueChanged;

        private PlotView _plotView;

        public PlotView PlotView
        {
            get { return _plotView; }
            set => this.RaiseAndSetIfChanged(ref _plotView, value);
        }


        private PlotModel _plotModel;

        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set => this.RaiseAndSetIfChanged(ref _plotModel, value);
        }

        public List<double> OilCondition = new List<double>();
        private ObservableCollection<OilTestResult> oilTestResults;

        public ObservableCollection<OilTestResult> OilTestResults

        {
            get { return oilTestResults; }
            set => this.RaiseAndSetIfChanged(ref oilTestResults, value);
        }

        readonly Dictionary<DateTime, double> GraphData = new Dictionary<DateTime, double>();
        private ObservableCollection<string> _selectTime;
        public ObservableCollection<string> SelectTime
        {
            get => _selectTime;
            set => this.RaiseAndSetIfChanged(ref _selectTime, value);
        }
        public string GraphUnitText { get; set; }
        private bool? _isLossFactor;

        public bool? IsLossFactor
        {
            get { return _isLossFactor; }
            set => this.RaiseAndSetIfChanged(ref _isLossFactor, value);
        }

        private string format { get; set; } = "dd/MM/yyyy";
        private OxyPlot.Axes.DateTimeAxis _xAxis;

        public OxyPlot.Axes.DateTimeAxis XAxis
        {
            get { return _xAxis; }
            set => this.RaiseAndSetIfChanged(ref _xAxis, value);
        }


        private readonly IGeneralSettingsDatabase _db;

        // Event service for subscribing or publishing events
        private IEventAggregator @event = Bootstrapper.GetRequiredService<IEventAggregator>(Locator.Current);
        public GraphControlViewModel()
        {
            InitializeGraph();
        }
        public GraphControlViewModel(Dictionary<DateTime, double> data)
        {
            GraphData = data;
            PlotModel = new PlotModel();

            // IMPORTANT: Dont change order of these methods
            _db = new GeneralSettingsDatabase();
            _db.ReadFromDatabase();
            InitializeUnitFromDb();
            InitializeGraphUnits();
            InitializeGraph();
            SubscribeToEvent();

        }


        private void SubscribeToEvent()
        {
            @event.GetEvent<GraphEventArgs>().Subscribe(OnGraphEvent);
        }

        private void InitializeUnitFromDb()
        {


            if (_db.GeneralSettings != null)
            {
                IsLossFactor = _db.GeneralSettings.LossFactor;
            }
            else
            {
                IsLossFactor = null;
            }
        }
        // Define the InitializeGraphUnits method, which sets the unit label for the graph's Y axis based on the IsLossFactor property
        private void InitializeGraphUnits()
        {
            // If the IsLossFactor property is not null, set the Y axis label and the graph unit text accordingly
            if (IsLossFactor != null)
            {

                GraphUnitText = (bool)IsLossFactor ? Unit.LF.ToString() + " %" : Unit.TDN.ToString();
            }
        }

        private void OnGraphEvent(object obj)
        {
            Dictionary<DateTime, double> data = new Dictionary<DateTime, double>();
            if (obj is XAxisRange selectedItem)
            {
                switch ((XAxisRange)selectedItem)
                {
                    case XAxisRange.OneMonth:
                        format = "dd/MM/yyyy";
                        data = GroupedData(selectedItem);
                        break;
                    case XAxisRange.ThreeMonths:
                        format = "MM/yyyy";
                        data = GroupedData(selectedItem);
                        break;
                    case XAxisRange.SixMonths:
                        format = "MM/yyyy";
                        data = GroupedData(selectedItem);
                        break;
                    case XAxisRange.OneYear:
                        format = "yyyy";
                        data = GroupedData(selectedItem);
                        break;
                    default:
                        format = "yyyy";
                        data = GroupedData(selectedItem);
                        break;
                }
                InitializeGraph(data);

            }

        }
        // Define the GroupedData method, which groups the data based on the specified X axis range
        public Dictionary<DateTime, double> GroupedData(XAxisRange range)
        {
            int interval = 0;
            switch (range)
            {

                case XAxisRange.OneMonth:
                    XAxis.IntervalType = DateTimeIntervalType.Months;
                    interval = 1;
                    break;
                case XAxisRange.ThreeMonths:
                    XAxis.IntervalType = DateTimeIntervalType.Months;
                    interval = 3;
                    break;
                case XAxisRange.SixMonths:
                    XAxis.IntervalType = DateTimeIntervalType.Months;
                    interval = 6;
                    break;
                case XAxisRange.OneYear:
                    XAxis.IntervalType = DateTimeIntervalType.Years;
                    interval = 1;
                    break;
            }
            // Create a dictionary to store the grouped data
            var groupedDataDict = new Dictionary<DateTime, double>();
            var graphData = new Dictionary<DateTime, double>(GraphData);

            // Group the data in the GraphData dictionary by month and year, using the interval
            if (interval == 1 && XAxis.IntervalType == DateTimeIntervalType.Months)
            {
                return GraphData;
            }
            graphData.GroupBy(x => new DateTime(
                            x.Key.Year,
                            ((x.Key.Month - 1) / interval) * interval + 1,
                            1))
                     .ToList()
                     .ForEach(g =>
                     {
                         double totalValue = 0;
                         foreach (var item in g)
                         {
                             totalValue = item.Value;
                         }
                         groupedDataDict.Add(g.Key, totalValue);
                     });

            // Return the dictionary of grouped data
            return groupedDataDict;

        }

        // Define the InitializeGraph method, which initializes the graph with the given data
        private void InitializeGraph(Dictionary<DateTime, double> data = null)
        {

            PlotModel.Series.Clear();
            if (data == null)
            {
                data = GraphData;
            }
            var series = new OxyPlot.Series.LineSeries()
            {
                Color = OxyColors.DarkBlue, // Set the series color to blue
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1


            };
            foreach (var d in data)
            {
                var xValue = d.Key.ToOADate();
                var yValue = d.Value;
                series.Points.Add(new DataPoint(xValue, yValue));
            }
            PlotModel.Axes.Clear();
            InitializeAxes();
            PlotModel.Series.Add(series);
            PlotModel.InvalidatePlot(true);

        }

        private void InitializeAxes()
        {

            XAxis = new OxyPlot.Axes.DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = format,
                Title = "Date",
                AxislineStyle = LineStyle.Solid,
                IsZoomEnabled = false, //(bool)IsLossFactor,
                IntervalType = DateTimeIntervalType.Auto,
                MajorStep = 20,
                MinorIntervalType = DateTimeIntervalType.Auto,
                MinorStep = 30,
                IntervalLength = 75, // Set the interval length to a suitable value
                Font = "OpenSans",
                // Set the major tick size to a suitable value
                MajorTickSize = 0,
                // Set the minor tick size to a suitable value
                MinorTickSize = 0.5,
                // Set the interval for each range
                Minimum = GraphData.Keys.Min().ToOADate(),
                Maximum = GraphData.Keys.Max().ToOADate(),
                StartPosition = 0,
                MaximumRange = GraphData.Keys.Max().ToOADate(),
                PositionAtZeroCrossing = (bool)IsLossFactor,
                IsPanEnabled = false
            };

            var yAxis = new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Left,
                Title = (bool)IsLossFactor ? $"Oil Condition (%{Unit.LF})" : $"Oil Condition ({Unit.TDN})",
                LabelFormatter = (double arg) =>
                {
                    if ((bool)IsLossFactor)
                    {
                        return arg + " %LF";
                    }
                    else
                    {
                        return arg + " TDN";
                    }
                },
                AbsoluteMinimum = (bool)IsLossFactor ? -15 : 0,
                AbsoluteMaximum = (bool)IsLossFactor ? 45 : 1200,
                MajorStep = (bool)IsLossFactor ? 5 : 100,
                AxislineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineThickness = 0.5,
                MajorGridlineColor = OxyColor.Parse("#000000"),
                Font = "OpenSans",
                MinorTickSize = 0,
                StartPosition = 0,
                IsPanEnabled = false,
                MinimumRange = (bool)IsLossFactor ? 60 : 1200,
                MaximumRange = (bool)IsLossFactor ? 60 : 1200,
            };

            PlotModel.Axes.Add(XAxis);
            PlotModel.Axes.Add(yAxis);
            PlotModel.PlotType = PlotType.XY;
            PlotModel.PlotAreaBorderThickness = new OxyThickness(0);
        }
        public void Dispose()
        {
            @event.GetEvent<GraphEventArgs>().Unsubscribe(OnGraphEvent);
        }
    }
}
