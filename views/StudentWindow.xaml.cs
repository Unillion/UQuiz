using System.Windows;
using System.Windows.Input;
using UQuiz.models.users;
using UQuiz.ViewModels;

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
    }
}