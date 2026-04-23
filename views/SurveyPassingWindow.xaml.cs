using System.Windows;
using System.Windows.Input;
using UQuiz.ViewModels;

namespace UQuiz.views
{
    public partial class SurveyPassingWindow : Window
    {
        public SurveyPassingWindow(int surveyId)
        {
            InitializeComponent();
            DataContext = new SurveyPassingViewModel(surveyId);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }
    }
}