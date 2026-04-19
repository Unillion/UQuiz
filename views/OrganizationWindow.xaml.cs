using System.Windows;
using System.Windows.Input;
using UQuiz.models.users;
using UQuiz.ViewModels;

namespace UQuiz.views
{
    public partial class OrganizationWindow : Window
    {
        public OrganizationWindow(Organization organization)
        {
            InitializeComponent();
            DataContext = new OrganizationViewModel(organization);
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