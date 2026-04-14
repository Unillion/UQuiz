using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UQuiz.commands;
using UQuiz.models.users;
using UQuiz.ViewModels.Base;

namespace UQuiz.ViewModels
{
    public class TeacherViewModel : ViewModelBase
    {
        private readonly Teacher _teacher;
        private string _pageTitle;
        private bool _isCreateButtonVisible;
        private string _selectedMenu;
        private ObservableCollection<SurveyCardViewModel> _surveys;

        public TeacherViewModel(Teacher teacher)
        {
            _teacher = teacher;

            TeacherName = teacher.Login;
            TeacherEmail = teacher.Email;

            NavigateCommand = new RelayCommand(ExecuteNavigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            CreateSurveyCommand = new RelayCommand(ExecuteCreateSurvey);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            MaximizeCommand = new RelayCommand(ExecuteMaximize);
            CloseCommand = new RelayCommand(ExecuteClose);

            SelectedMenu = "Surveys";

            LoadTestSurveys();
        }

        #region Properties

        public string TeacherName { get; }
        public string TeacherEmail { get; }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public bool IsCreateButtonVisible
        {
            get => _isCreateButtonVisible;
            set => SetProperty(ref _isCreateButtonVisible, value);
        }

        public string SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (SetProperty(ref _selectedMenu, value))
                {
                    UpdatePageForMenu(value);
                }
            }
        }

        public ObservableCollection<SurveyCardViewModel> Surveys
        {
            get => _surveys;
            set => SetProperty(ref _surveys, value);
        }

        #endregion

        #region Commands

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CreateSurveyCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }

        #endregion

        #region Command Methods

        private void ExecuteNavigate(object parameter)
        {
            if (parameter is string menu)
            {
                SelectedMenu = menu;
            }
        }

        private void ExecuteLogout(object parameter)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            if (parameter is Window window)
            {
                window.Close();
            }
        }

        private void ExecuteCreateSurvey(object parameter)
        {
            MessageBox.Show("Здесь будет конструктор опросов", "Создание опроса",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteMinimize(object parameter)
        {
            if (parameter is Window window)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void ExecuteMaximize(object parameter)
        {
            if (parameter is Window window)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        private void ExecuteClose(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region Private Methods

        private void UpdatePageForMenu(string menu)
        {
            switch (menu)
            {
                case "Surveys":
                    PageTitle = "Мои опросы";
                    IsCreateButtonVisible = true;
                    break;
                case "Analytics":
                    PageTitle = "Аналитика";
                    IsCreateButtonVisible = false;
                    break;
                case "Students":
                    PageTitle = "Ученики";
                    IsCreateButtonVisible = false;
                    break;
                case "Organizations":
                    PageTitle = "Организации";
                    IsCreateButtonVisible = false;
                    break;
            }
        }

        private void LoadTestSurveys()
        {
            Surveys = new ObservableCollection<SurveyCardViewModel>
            {
                new SurveyCardViewModel
                {
                    Title = "Математика 10 класс",
                    Description = "Контрольная работа по алгебре",
                    QuestionsCount = 15,
                    CompletedCount = "32/45",
                    CreatedDate = "10.04.2026"
                },
                new SurveyCardViewModel
                {
                    Title = "История России",
                    Description = "Тест по эпохе Петра I",
                    QuestionsCount = 20,
                    CompletedCount = "28/45",
                    CreatedDate = "08.04.2026"
                },
                new SurveyCardViewModel
                {
                    Title = "Физика 9 класс",
                    Description = "Законы Ньютона",
                    QuestionsCount = 10,
                    CompletedCount = "40/45",
                    CreatedDate = "05.04.2026"
                }
            };
        }

        #endregion
    }

    public class SurveyCardViewModel : ViewModelBase
    {
        private string _title;
        private string _description;
        private int _questionsCount;
        private string _completedCount;
        private string _createdDate;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int QuestionsCount
        {
            get => _questionsCount;
            set => SetProperty(ref _questionsCount, value);
        }

        public string CompletedCount
        {
            get => _completedCount;
            set => SetProperty(ref _completedCount, value);
        }

        public string CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }
    }
}