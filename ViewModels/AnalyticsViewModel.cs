using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using UQuiz.services;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels
{
    public class AnalyticsViewModel : ViewModelBase
    {
        private readonly ISurveyService _surveyService;
        private SurveyAnalytics _analytics;
        private PlotModel _scoreDistributionModel;
        private PlotModel _questionStatsModel;
        private PlotModel _timeDistributionModel;

        public PlotModel TimeDistributionModel
        {
            get => _timeDistributionModel;
            set => SetProperty(ref _timeDistributionModel, value);
        }

        public AnalyticsViewModel(int surveyId)
        {
            _surveyService = new SurveyService();
            LoadData(surveyId);
        }

        public SurveyAnalytics Analytics
        {
            get => _analytics;
            set => SetProperty(ref _analytics, value);
        }

        public PlotModel ScoreDistributionModel
        {
            get => _scoreDistributionModel;
            set => SetProperty(ref _scoreDistributionModel, value);
        }

        public PlotModel QuestionStatsModel
        {
            get => _questionStatsModel;
            set => SetProperty(ref _questionStatsModel, value);
        }

        private void LoadData(int surveyId)
        {
            Analytics = _surveyService.GetSurveyAnalytics(surveyId);
            BuildQuestionStatsChart();
            BuildScoreDistributionChart();
            BuildTimeDistributionChart();
        }

        private void BuildQuestionStatsChart()
        {
            var model = new PlotModel
            {
                Title = null,
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var barSeries = new BarSeries
            {
                FillColor = OxyColor.FromRgb(63, 81, 181),
                StrokeThickness = 0,
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0:F1}"
            };

            var labels = new List<string>();

            foreach (var q in Analytics.QuestionStats)
            {
                barSeries.Items.Add(new BarItem((double)q.AverageScore));
                labels.Add($"Вопрос {q.QuestionNumber}\n{q.QuestionText}");
            }

            model.Series.Add(barSeries);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "Questions",
                FontSize = 11
            });
            ((CategoryAxis)model.Axes[0]).Labels.AddRange(labels);

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = (double)Analytics.QuestionStats.Max(q => q.MaxPoints) * 1.1,
                Title = "Баллы",
                FontSize = 11
            });

            QuestionStatsModel = model;
        }
        private void BuildTimeDistributionChart()
        {
            var model = new PlotModel
            {
                Title = null,
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var barSeries = new BarSeries
            {
                FillColor = OxyColor.FromRgb(33, 150, 243),
                StrokeThickness = 0,
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0}"
            };

            var labels = new List<string>();

            foreach (var t in Analytics.TimeDistribution)
            {
                barSeries.Items.Add(new BarItem(t.Count));
                labels.Add(t.Label);
            }

            model.Series.Add(barSeries);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                FontSize = 11
            });
            ((CategoryAxis)model.Axes[0]).Labels.AddRange(labels);

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Title = "Количество учеников",
                FontSize = 11
            });

            TimeDistributionModel = model;
        }

        private void BuildScoreDistributionChart()
        {
            var model = new PlotModel
            {
                Title = null,
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var colors = new[]
            {
                OxyColor.FromRgb(76, 175, 80),
                OxyColor.FromRgb(139, 195, 74),
                OxyColor.FromRgb(255, 193, 7),
                OxyColor.FromRgb(255, 152, 0),
                OxyColor.FromRgb(244, 67, 54)
            };

            var pieSeries = new PieSeries
            {
                InsideLabelFormat = "{1}",
                InsideLabelPosition = 0.5,
                OutsideLabelFormat = "{0}: {2:F0}%",
                StrokeThickness = 2,
                Stroke = OxyColors.White,
                FontSize = 12
            };

            var total = Analytics.ScoreDistribution.Sum(d => d.Count);
            if (total == 0) total = 1;

            for (int i = 0; i < Analytics.ScoreDistribution.Count; i++)
            {
                var item = Analytics.ScoreDistribution[i];
                if (item.Count > 0)
                {
                    pieSeries.Slices.Add(new PieSlice(item.Label, item.Count)
                    {
                        Fill = colors[i % colors.Length]
                    });
                }
            }

            model.Series.Add(pieSeries);
            ScoreDistributionModel = model;
        }
    }
}