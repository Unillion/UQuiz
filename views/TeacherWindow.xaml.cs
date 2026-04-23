using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UQuiz.models.users;
using UQuiz.ViewModels;

namespace UQuiz.Views
{
    public partial class TeacherWindow : Window
    {
        public TeacherWindow(Teacher teacher)
        {
            InitializeComponent();
            DataContext = new TeacherViewModel(teacher);
        }
        private void SurveyCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is SurveyCardViewModel survey)
            {
                if (DataContext is TeacherViewModel vm)
                {
                    vm.OpenSurveyCommand.Execute(survey);
                }
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }
    }
}