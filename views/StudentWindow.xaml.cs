using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UQuiz.models.users;
using UQuiz.ViewModels;
using static UQuiz.ViewModels.StudentViewModel;

namespace UQuiz.views
{
    public partial class StudentWindow : Window
    {
        public StudentWindow(RegularUser student)
        {
            InitializeComponent();
            DataContext = new StudentViewModel(student);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }

        private void SurveyCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is StudentSurveyCardViewModel survey)
            {
                if (!string.IsNullOrEmpty(survey.Score))
                    return;

                if (DataContext is StudentViewModel vm)
                {
                    vm.OpenSurveyCommand.Execute(survey);
                }
            }
        }
    }
}