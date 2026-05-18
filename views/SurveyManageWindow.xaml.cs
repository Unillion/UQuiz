using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UQuiz.models.users;
using UQuiz.services;
using UQuiz.ViewModels;

namespace UQuiz.views
{
    public partial class SurveyManageWindow : Window
    {
        public SurveyManageWindow(Teacher teacher, int surveyId, string surveyTitle)
        {
            InitializeComponent();
            DataContext = new SurveyManageViewModel(teacher, surveyId, surveyTitle);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }

        private void ResponseItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is StudentResponseInfo response)
            {
                if (DataContext is SurveyManageViewModel vm)
                {
                    vm.ViewResponseCommand.Execute(response);
                }
            }
        }
    }
}