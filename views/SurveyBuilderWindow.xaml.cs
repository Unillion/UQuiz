using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UQuiz.models.users;
using UQuiz.ViewModels;

namespace UQuiz.views
{
    public partial class SurveyBuilderWindow : Window
    {
        public SurveyBuilderWindow(Teacher teacher)
        {
            InitializeComponent();
            DataContext = new SurveyBuilderViewModel(teacher);
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
