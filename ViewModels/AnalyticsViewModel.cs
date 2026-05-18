using System;
using System.Collections.Generic;
using System.Linq;
using UQuiz.services;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels
{
    public class AnalyticsViewModel : ViewModelBase
    {
        private readonly ISurveyService _surveyService;
        private SurveyAnalytics _analytics;
        private OxyPlot.PlotModel _scoreDistributionModel;
        private OxyPlot.PlotModel _questionStatsModel;

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

        public OxyPlot.PlotModel ScoreDistributionModel
        {
            get => _scoreDistributionModel;
            set => SetProperty(ref _scoreDistributionModel, value);
        }

        public OxyPlot.PlotModel QuestionStatsModel
        {
            get => _questionStatsModel;
            set => SetProperty(ref _questionStatsModel, value);
        }

        private void LoadData(int surveyId)
        {
            Analytics = _surveyService.GetSurveyAnalytics(surveyId);
            BuildScoreDistributionChart();
            BuildQuestionStatsChart();
        }

        private void BuildScoreDistributionChart()
        {
            var model = new OxyPlot.PlotModel { Title = "Распределение оценок" };
            var series = new OxyPlot.Series.PieSeries { InsideLabelFormat = "{1}", OutsideLabelFormat = "{0}" };

            foreach (var item in Analytics.ScoreDistribution.Where(d => d.Count > 0))
            {
                series.Slices.Add(new OxyPlot.Series.PieSlice(item.Label, item.Count));
            }

            model.Series.Add(series);
            ScoreDistributionModel = model;
        }

        private void BuildQuestionStatsChart()
        {
            var model = new OxyPlot.PlotModel { Title = "Средний балл по вопросам" };
            var series = new OxyPlot.Series.BarSeries();

            var labels = new List<string>();
            foreach (var q in Analytics.QuestionStats)
            {
                series.Items.Add(new OxyPlot.Series.BarItem((double)q.AverageScore));
                labels.Add($"Вопрос {q.QuestionNumber}");
            }

            model.Series.Add(series);
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left
            });
            ((OxyPlot.Axes.CategoryAxis)model.Axes[0]).Labels.AddRange(labels);

            model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Minimum = 0,
                Maximum = (double)Analytics.QuestionStats.Max(q => q.MaxPoints)
            });

            QuestionStatsModel = model;
        }
    }
}